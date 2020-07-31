using System;
using System.ComponentModel;
using System.IO;
using System.Text;
using System.Windows.Input;
using System.Xml;
using Glory.SorterInterface.Message;
using Microsoft.Win32;
using System.Windows;
using Glory.SorterInterface.SorterInterfaceClient.Client;
using Glory.SorterInterface.MessageFormat;
using Glory.SorterInterface.MessageFormat.Message;
using Glory.SorterInterface.SorterInterfaceClient.Log;
using WFPresentacion.Tcp;
using WFPresentacion.ViewModel;
//using WFPresentacion.CustomView;
using System.Threading;
using Newtonsoft.Json;
using SorterIotInterfaceSimulator.Common.Logic;
//using DocumentFormat.OpenXml.Vml;
//using System.Windows.Forms;
using System.Windows.Threading;
using OpenFileDialog = System.Windows.Forms.OpenFileDialog;
using WFPresentacion.CustomView;
using WFPresentacion.ClasesEntidades;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using System.Threading.Tasks;

namespace WFPresentacion
{

    public class MainWindowViewModel : INotifyPropertyChanged, IDisposable
    {
        /// <summary>
        /// TCP client
        /// </summary>
        TcpClientProtocol tcpClient = null;

        public List<Task> tasks = new List<Task>();
        public JObject o1 { get; set; }
        public int conteotask;

        /// <summary>
        /// Validate
        /// </summary>
        Validate validate = new Validate();

        CustomTextBox logTextBox;

        Thread polling;

        /// <summary>
        /// 自動実行機能
        /// </summary>
        ViewModel.AutoAction autoAction;

        #region コンストラクタ

        public Root ConteoDineroSeriales { get; set; }
        public List<Root> ListConteoDineroSeriales { get; internal set; }
        public RootTotal ConteoRootTotalSeriales { get; set; }
        public List<RootTotal> ListConteoRootTotalSeriales { get; internal set; }


        /// <summary>
        /// コンストラクタ
        /// </summary>
        public MainWindowViewModel(CustomTextBox logTextBox)
        {
            this.logTextBox = logTextBox;

            // データバインディング
            this.Binding();

            // 設定値読込
            this.ReadSetting();


            this.polling = new Thread(this.KeepAliveRun);
            this.polling.Start();
        }



        /// <summary>
        /// データバインディング
        /// </summary>
        private void Binding()
        {
        }

        /// <summary>
        /// 設定値読込
        /// </summary>
        private void ReadSetting()
        {
            // 設定値読込
            XmlReader xmlReader = XmlReader.Create("SorterIotInterfaceSimulator.xml");
            while (xmlReader.Read())
            {
                if (xmlReader.NodeType == XmlNodeType.Element)
                {
                    switch (xmlReader.LocalName)
                    {
                        case "LeadFileName":
                            this.LeadFileName = xmlReader.ReadString();
                            break;

                        case "TcpPortNo":
                            this.TcpPortNo = xmlReader.ReadString();
                            break;
                        case "TcpAddress":
                            this.TcpAddress = xmlReader.ReadString();
                            break;
                        case "TcpDownloadFile":
                            this.TcpDownloadFile = xmlReader.ReadString();
                            break;
                        case "TcpUploadFolder":
                            this.TcpUploadFolder = xmlReader.ReadString();
                            break;

                        default:
                            break;
                    }
                }
            }
        }

        #endregion

        #region IDisposable interface implement

        /// <summary>
        /// Dispose
        /// </summary>
        public void Dispose()
        {
            this.validate.Dispose();
            this.validate = null;


            this.shoudStop = true;
            this.polling.Join();

            if (this.tcpClient != null)
            {
                this.tcpClient.Dispose();
                this.tcpClient = null;
            }
        }

        #endregion

        #region 更新通知

        /// <summary>
        /// 更新通知
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged(String info)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(info));
            }
        }

        #endregion

        #region 送信メッセージ読込

        /// <summary>
        /// 送信メッセージファイル
        /// </summary>
        private String leadFileName = String.Empty;
        /// <summary>
        /// 送信メッセージファイル
        /// </summary>
        public String LeadFileName
        {
            get { return this.leadFileName; }
            set
            {
                this.leadFileName = value;
                NotifyPropertyChanged("LeadFileName");
            }
        }
        /// <summary>
        /// 送信メッセージ読込処理コマンド
        /// </summary>
        public ICommand LeadMessage { get; set; }
        /// <summary>
        /// 送信メッセージ読込処理
        /// </summary>
        public void LeadMessageFunc()
        {
            try
            {
                StreamReader sr = new StreamReader(this.LeadFileName, Encoding.Default);
                this.SendMessage = string.Empty;
                string line, str = string.Empty;
                if (File.Exists(this.LeadFileName))
                {
                    while ((line = sr.ReadLine()) != null) //テキストファイルを一行づつ読み込む
                    {
                        str = str + line + "\r\n";
                    }
                    this.SendMessage = str;
                }
                sr.Close();
            }
            catch
            {
                return;
            }
        }

        /// <summary>
        /// 送信メッセージ選択処理コマンド
        /// </summary>
        public ICommand SelectMessage { get; set; }
        /// <summary>
        /// 送信メッセージ選択処理
        /// </summary>
        public void SelectMessageFunc()
        {
            try
            {
                // アプリケーション実行ファイルパス
                string exePath = Environment.GetCommandLineArgs()[0];
                string exeFullPath = System.IO.Path.GetFullPath(exePath);
                string startupPath = System.IO.Path.GetDirectoryName(exeFullPath);

                OpenFileDialog dialog = new OpenFileDialog();
                dialog.Title = "Message File Open";
                dialog.Filter = "テキストファイル|*.txt";
                dialog.InitialDirectory = startupPath + "\\Message";

                //if (dialog.ShowDialog() == true)
                //{
                //    this.LeadFileName = dialog.FileName.Replace(startupPath, ".");

                //    //LeadMessageFunc();
                //}
            }
            catch
            {
                return;
            }
        }
        #endregion

        #region メッセージ送信

        /// <summary>
        /// メッセージ送信
        /// </summary>
        private String sendMessage = String.Empty;
        /// <summary>
        /// メッセージ送信
        /// </summary>
        public String SendMessage
        {
            get { return this.sendMessage; }
            set
            {
                this.sendMessage = value;
                NotifyPropertyChanged("SendMessage");
            }
        }

        #endregion

        #region メッセージログ

        /// <summary>
        /// メッセージログ
        /// </summary>
        private String messageLog = String.Empty;
        /// <summary>
        /// メッセージログ
        /// </summary>
        public String MessageLog
        {
            get { return this.messageLog; }
            set
            {
                this.messageLog = value;
                NotifyPropertyChanged("MessageLog");
            }
        }

        #endregion

        #region ログ更新

        /// <summary>
        /// send / receive message log
        /// </summary>
        /// <param name="sendReceive">Publish/Subscribe/UnSubscribe/Receive</param>
        /// <param name="topic">topic</param>
        /// <param name="messageType">Publish/Receive message Type</param>
        /// <param name="messageName">Publish/Receive message Name</param>
        /// <param name="message">Publish/Receive message</param>
        private void LogReceive(MessageLog.LogType logType, string topic, MessageType messageType, MessageName messageName, string message)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("Topic: ");
            sb.Append(topic);

            // validate message
            if (String.IsNullOrEmpty(message) == false)
            {
                string validaeResult = "";
                try
                {
                    validaeResult = validate.ValidateMessage(message, messageType, messageName);
                }
                catch
                {
                }
                sb.Append("\n");
                sb.Append(validaeResult);
            }

            if ((message != null) && (message != String.Empty))
            {
                sb.Append("\n");
                sb.Append("Message: ");
                sb.Append(messageType.ToString());
                sb.Append(" ");
                sb.Append(messageName.ToString());
                sb.Append("\n");
                sb.Append(message);
            }

            this.UpdateLog(logType.ToString(), sb.ToString());
        }

        /// <summary>
        /// ログ更新ロックオブジェクト
        /// </summary>
        private object updateLogLock = new Object();

        /// <summary>
        /// ログ更新
        /// </summary>
        /// <param name="direction"></param>
        /// <param name="message"></param>
        private void UpdateLog(string direction, string message)
        {
            try
            {
                lock (updateLogLock)
                {
                    if (message.Contains("\"GetCurrency\""))
                    {
                        string newmensaje = message.Replace("JSON schema validate : common=OK, detail=OK.", "");
                        RootCurrency cuerpo = JsonConvert.DeserializeObject<RootCurrency>(newmensaje);
                        if (cuerpo.Detail != null)
                        {
                            this.Currencies = cuerpo;
                        }

                    }

                    if (message.Contains("\"GetDenominationInformation\""))
                    {
                        string newmensaje = message.Replace("JSON schema validate : common=OK, detail=OK.", "");
                        RootDenominaciones cuerpo = JsonConvert.DeserializeObject<RootDenominaciones>(newmensaje);
                        if (cuerpo.Detail != null)
                        {
                            this.Denominaciones = JsonConvert.DeserializeObject<RootDenominaciones>(newmensaje);
                        }

                    }

                    if (message.Contains("\"NoteTransported\""))
                    {
                        string newmensaje = message.Replace("JSON schema validate : common=OK, detail=NG.", "");

                        tasks.Add(Task.Run(() =>
                        {
                            //int[] values = new int[10000];
                            //int taskTotal = 0;
                            //int taskN = 0;
                            //Monitor.Enter(o1);
                            //if (o1 == null)
                            //{
                            //    o1 = JObject.Parse(message);
                            //}

                            //Monitor.Exit(o1);

                            //Console.WriteLine("Mean for task {0,2}: {1:N2} (N={2:N0})",
                            //                  Task.CurrentId, (taskTotal * 1.0) / taskN,
                            //                  taskN);
                            //Interlocked.Add(ref conteotask, taskTotal);

                            Monitor.Enter(this.ListConteoDineroSeriales);

                            this.ListConteoDineroSeriales.Add(JsonConvert.DeserializeObject<Root>(newmensaje));


                            Monitor.Exit(this.ListConteoDineroSeriales);

                        }));

                        //JObject o2 = JObject.Parse(message);
                        //if (o1 == null)
                        //{
                        //    o1 = JObject.Parse(message);
                        //}


                        //o1.Merge(o2, new JsonMergeSettings
                        //{
                        //    // union array values together to avoid duplicates
                        //    MergeArrayHandling = MergeArrayHandling.Union
                        //});

                        //this.ListConteoDineroSeriales.Add(JsonConvert.DeserializeObject<Root>(newmensaje));
                    }

                    if (message.Contains("\"TransactionDetected\""))
                    {
                        string newmensaje = message.Replace("JSON schema validate : common=OK, detail=OK.", "");
                        this.ListConteoRootTotalSeriales.Add(JsonConvert.DeserializeObject<RootTotal>(newmensaje));
                    }

                    this.MessageLog = this.GetLogText(direction, message);
                    this.ScrollToEnd();

                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show(ex.Message);
            }
        }

        private void ScrollToEnd()
        {
            try
            {
                this.logTextBox.Dispatcher.InvokeAsync(() => this.logTextBox.ScrollToEnd());
            }
            catch
            {
            }
        }

        /// <summary>
        /// ログ更新
        /// </summary>
        /// <param name="direction"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        private string GetLogText(string direction, string message)
        {
            return this.GetLogText(this.MessageLog, System.DateTime.Now, direction, message);
        }

        /// <summary>
        /// ログ更新
        /// </summary>
        /// <param name="src"></param>
        /// <param name="nowTime"></param>
        /// <param name="direction"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        private string GetLogText(string src, DateTime nowTime, string direction, string message)
        {
            // メッセージログの末尾に追加
            StringBuilder sb = new StringBuilder(src);
            sb.Append(direction);
            sb.Append(" [");
            /* @@@ chg USF-200_機能改善 2018.06.25 by Hemmi ↓ */
            sb.Append(string.Format("{0:yyyy/MM/dd HH:mm:ss.fff}", nowTime));
            /* @@@ chg USF-200_機能改善 2018.06.25 by Hemmi ↑ */
            sb.Append("]");
            sb.Append("\n");
            sb.Append(message);
            sb.Append("\n");
            sb.Append("\n");

            return sb.ToString();
        }

        #endregion

        #region TCPサーバー接続

        /// <summary>
        /// 接続先ポート番号
        /// </summary>
        public String TcpPortNo { get; set; }
        /// <summary>
        /// 接続先IPアドレス
        /// </summary>
        public String TcpAddress { get; set; }

        /// <summary>
        /// TCPサーバー接続
        /// </summary>
        public ICommand TcpConnect { get; set; }
        /// <summary>
        /// TCPサーバー接続処理
        /// </summary>
        public void TcpConnectFunc()
        {
            if (this.tcpClient != null)
            {
                // メッセージログの末尾に追加
                this.MessageLog = this.MessageLog + "<< Please Disconnect TCP server. >>\n";
                this.ScrollToEnd();
                return;
            }

            try
            {
                int portNo;
                if (Int32.TryParse(this.TcpPortNo, out portNo) == false)
                {
                    this.MessageLog = this.MessageLog + "<< TCP portNo error. >>\n";
                    this.ScrollToEnd();
                    return;
                }

                // ログ出力
                string logText = String.Format("PortNo={0}, IP Address={1}", this.TcpPortNo, this.TcpAddress);
                this.UpdateLog("Connect to TCP server", logText);

                // SorterIotInterface オブジェクト生成
                this.tcpClient = new TcpClientProtocol(this.TcpAddress, portNo);

                this.tcpClient.receiveFunc += this.ReceiveFunc;
                this.tcpClient.TcpLogEvent += this.TcpLogUpdate;

                // 自動実行機能
                this.autoAction = new AutoAction(this.tcpClient);
            }
            catch (Exception ex)
            {
                this.MessageLog = this.MessageLog + ex.ToString() + "\n\n";
                this.ScrollToEnd();

                this.TcpDisconnectFunc();
            }
        }

        #endregion

        #region TCPサーバー切断

        /// <summary>
        /// TCPサーバー切断
        /// </summary>
        public ICommand TcpDisconnect { get; set; }
        /// <summary>
        /// TCPサーバー切断処理
        /// </summary>
        public void TcpDisconnectFunc()
        {
            try
            {
                if (this.tcpClient == null)
                {
                    return;
                }

                this.UpdateLog("Disconnect from TCP server", string.Empty);

                // サーバー切断
                this.tcpClient.Dispose();
                this.tcpClient = null;
            }
            catch (Exception ex)
            {
                this.MessageLog = this.MessageLog + ex.ToString() + "\n\n";
                this.ScrollToEnd();
            }
        }

        #endregion

        #region Tcp ダウンロードファイル

        /// <summary>
        /// Tcp ダウンロードファイル
        /// </summary>
        private String tcpDownloadFile = String.Empty;
        /// <summary>
        /// Tcp ダウンロードファイル
        /// </summary>
        public String TcpDownloadFile
        {
            get { return this.tcpDownloadFile; }
            set
            {
                this.tcpDownloadFile = value;
                NotifyPropertyChanged("TcpDownloadFile");
            }
        }

        /// <summary>
        /// Tcp ダウンロードファイル選択処理コマンド
        /// </summary>
        public ICommand SelectTcpDownloadFile { get; set; }
        /// <summary>
        /// Tcp ダウンロードファイル選択処理
        /// </summary>
        public void SelectTcpDownloadFileFunc()
        {
            try
            {
                // アプリケーション実行ファイルパス
                string exePath = Environment.GetCommandLineArgs()[0];
                string exeFullPath = System.IO.Path.GetFullPath(exePath);
                string startupPath = System.IO.Path.GetDirectoryName(exeFullPath);

                OpenFileDialog dialog = new OpenFileDialog();
                dialog.Title = "TCP download File Open";
                dialog.Filter = "全てのファイル|*.*";
                dialog.InitialDirectory = startupPath + "\\TcpDownloadFile";

                //if (dialog.ShowDialog() == true)
                //{
                //    this.TcpDownloadFile = dialog.FileName.Replace(startupPath, ".");
                //}
            }
            catch
            {
                return;
            }
        }

        #endregion

        #region TCP アップロードフォルダ指定

        /// <summary>
        /// TCP アップロードフォルダ
        /// </summary>
        private String tcpUploadFolder = String.Empty;
        /// <summary>
        /// TCP アップロードフォルダ
        /// </summary>
        public String TcpUploadFolder
        {
            get { return this.tcpUploadFolder; }
            set
            {
                this.tcpUploadFolder = value;
                NotifyPropertyChanged("TcpUploadFolder");
            }
        }

        /// <summary>
        /// TCP アップロードフォルダ選択処理コマンド
        /// </summary>
        public ICommand SelectTcpUploadFolder { get; set; }
        /// <summary>
        /// TCP アップロードフォルダ選択処理
        /// </summary>
        public void SelectTcpUploadFolderFunc()
        {
            try
            {
                // アプリケーション実行ファイルパス
                string exePath = Environment.GetCommandLineArgs()[0];
                string exeFullPath = System.IO.Path.GetFullPath(exePath);
                string startupPath = System.IO.Path.GetDirectoryName(exeFullPath);

                System.Windows.Forms.FolderBrowserDialog fbd = new System.Windows.Forms.FolderBrowserDialog();
                fbd.Description = "Upload Folder Slect";
                fbd.RootFolder = Environment.SpecialFolder.Desktop;
                fbd.SelectedPath = startupPath + "\\UploadFolder";
                fbd.ShowNewFolderButton = true;

                //ダイアログを表示する
                if (fbd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    // 選択されたフォルダで更新
                    this.TcpUploadFolder = fbd.SelectedPath.Replace(startupPath, ".");
                }
            }
            catch
            {
                return;
            }
        }
        #endregion

        #region TCPキープアライブ

        /// <summary>
        /// TCPキープアライブ処理コマンド
        /// </summary>
        public ICommand TcpKeepAlive { get; set; }
        /// <summary>
        /// TCPキープアライブ処理
        /// </summary>
        public void TcpKeepAliveFunc()
        {
            if (this.tcpClient == null)
            {
                return;
            }

            try
            {
                this.UpdateLog("Simulator send KeepAlive(TCP). ", "");
                this.tcpClient.Send("");
            }
            catch (Exception ex)
            {
                this.MessageLog = this.MessageLog + ex.ToString() + "\n\n";
                this.ScrollToEnd();
            }
        }

        #endregion

        #region TCPメッセージ送信(JSON生)

        /// <summary>
        /// TCPメッセージ送信(JSON生)処理コマンド
        /// </summary>
        public ICommand TcpSendJson { get; set; }
        public RootDenominaciones Denominaciones { get; internal set; }
        public RootCurrency Currencies { get; internal set; }


        /// <summary>
        /// メッセージ送信(JSON生)処理
        /// </summary>
        public void TcpSendJsonFunc()
        {
            if (this.tcpClient == null)
            {
                return;
            }

            try
            {
                if (String.IsNullOrEmpty(this.sendMessage) == true)
                {
                    this.UpdateLog("send message(TCP) is null or empty", "");
                    return;
                }

                string message = this.sendMessage;
                message = message.Replace("\r\n", "");
                message = message.Replace("\n", "");
                message = message.Replace("\t", "");

                SorterIfMessageFormat obj = SorterIfMessageFormat.DeSerialize<SorterIfMessageFormat>(message);
                MessageType type = obj.Type;
                MessageName name = obj.Name;

                StringBuilder sb = new StringBuilder();
                // validate message
                if (String.IsNullOrEmpty(message) == false)
                {
                    string validateResult = "";
                    try
                    {
                        validateResult = validate.ValidateMessage(message, type, name);
                    }
                    catch
                    {
                    }
                    sb.Append(validateResult);
                    sb.Append("\n");
                }

                {
                    dynamic logConvert = JsonConvert.DeserializeObject(message);
                    string logMessage = JsonConvert.SerializeObject(logConvert, Newtonsoft.Json.Formatting.Indented);
                    sb.Append(logMessage);
                }

                if ((name == MessageName.RequestDownloadSettingFile) ||
                    (name == MessageName.RequestDownloadUpgradeFile))
                {
                    if (File.Exists(this.TcpDownloadFile) == false)
                    {
                        this.UpdateLog(string.Format("({0}) not exist.\nPlease setting file path [Download Src File].", this.TcpDownloadFile), "");
                        return;
                    }

                    this.UpdateLog("Simulator send JSON message(TCP) with file download. ", sb.ToString());
                    this.tcpClient.Send(message, this.TcpDownloadFile);
                }
                else
                {
                    this.UpdateLog("Simulator send JSON message(TCP). ", sb.ToString());
                    this.tcpClient.Send(message);
                }
            }
            catch (Exception ex)
            {
                this.MessageLog = this.MessageLog + ex.ToString() + "\n\n";
                this.ScrollToEnd();
            }
        }

        #endregion

        #region TCPメッセージ受信

        /// <summary>
        /// TCPメッセージ受信
        /// </summary>
        /// <param name="message"></param>
        public void ReceiveFunc(string message, byte[] rawData)
        {
            SorterIfMessageFormat obj = SorterIfMessageFormat.DeSerialize<SorterIfMessageFormat>(message);
            MessageType type = obj.Type;
            MessageName name = obj.Name;

            StringBuilder sb = new StringBuilder();
            // validate message
            if (String.IsNullOrEmpty(message) == false)
            {
                string validateResult = "";
                try
                {
                    validateResult = validate.ValidateMessage(message, type, name);
                }
                catch
                {
                }
                sb.Append(validateResult);
                sb.Append("\n");
            }

            {
                dynamic logConvert = JsonConvert.DeserializeObject(message);
                string logMessage = JsonConvert.SerializeObject(logConvert, Newtonsoft.Json.Formatting.Indented);
                sb.Append(logMessage);
            }

            this.UpdateLog("Simulator receive TCP message.", sb.ToString());

            if (type == MessageType.Response)
            {
                if (name == MessageName.RequestUploadLogFile)
                {
                    string filename = this.TcpUploadFolder + "\\" + string.Format("RequestUploadLogFile_{0}.BIN", DateTime.Now.ToString("yyyyMMddHHmmss"));
                    this.WriteRawData(filename, rawData);
                }
                else if (name == MessageName.RequestUploadSettingFile)
                {
                    string filename = this.TcpUploadFolder + "\\" + string.Format("RequestUploadSettingFile_{0}.BIN", DateTime.Now.ToString("yyyyMMddHHmmss"));
                    this.WriteRawData(filename, rawData);
                }
                else
                {
                    if (rawData.Length > 0)
                    {
                        this.UpdateLog("TCP receive error. " + name + " rawdata.length=" + rawData.Length, this.sendMessage);
                    }
                }
            }

            this.autoAction.ReceiveProccess("", type, name, message);
        }

        private void WriteRawData(string filiPath, byte[] rawData)
        {
            // ファイルsample.datを書き込み用に開く
            using (Stream stream = File.OpenWrite(filiPath))
            {
                // streamに書き込むためのBinaryWriterを作成
                using (BinaryWriter writer = new BinaryWriter(stream))
                {
                    writer.Write(rawData);
                }
            }
        }

        #endregion

        #region TcpLog

        private void TcpLogUpdate(string log)
        {
            this.UpdateLog("TcpLog", log);
        }

        #endregion

        #region 自動キープアライブ

        private bool shoudStop = false;
        private void KeepAliveRun()
        {
            int count = 60;

            while (shoudStop == false)
            {
                Thread.Sleep(1000);

                if (this.tcpClient == null)
                {
                    continue;
                }

                try
                {
                    count--;
                    if (count <= 0)
                    {
                        count = 60;
                        this.tcpClient.Send("");
                    }
                }
                catch (Exception ex)
                {
                    this.MessageLog = this.MessageLog + ex.ToString() + "\n\n";
                    this.ScrollToEnd();
                }
            }
        }

        #endregion

    }
}
