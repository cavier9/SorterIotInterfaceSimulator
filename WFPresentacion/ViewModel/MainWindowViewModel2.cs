using Glory.SorterInterface.Message;
using Glory.SorterInterface.MessageFormat.Message;
using Glory.SorterInterface.SorterInterfaceClient.Log;
using Newtonsoft.Json;
using SorterIotInterfaceSimulator.Common.Logic;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using System.Windows.Input;
using System.Xml;
using WFPresentacion.ClasesEntidades;
using WFPresentacion.Tcp;

namespace WFPresentacion.ViewModel
{
    public class MainWindowViewModel2 : INotifyPropertyChanged, IDisposable
    {
      
        private readonly DataGridView Dgconteo;
        private readonly DataGridView DgTotalesAceptados;
        private readonly DataGridView DgTotalesRechazados;

        private Label LblTotales { get; set; }
        private Button BtnExportar { get; set; }



        private Form Myfrm { get; set; }
        private List<Root> ListaRoot = new List<Root>();
        private List<Note> ListaNote = new List<Note>();
        private RootDeviseStatus rootdevisestatus = new RootDeviseStatus();
        private RootTotal rootTotal = new RootTotal();
        private RootDenominaciones rootDenominaciones = new RootDenominaciones();
        private RootCurrency rootCurrency = new RootCurrency();

        /// <summary>
        /// TCP client
        /// </summary>
        private TcpClientProtocol tcpClient = null;

        /// <summary>
        /// Validate
        /// </summary>
        private Validate validate = new Validate();

        //CustomTextBox LogTextBox;

        private readonly Thread polling;

        /// <summary>
        /// 自動実行機能
        /// </summary>
        private ViewModel.AutoAction autoAction;

        #region コンストラクタ

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public MainWindowViewModel2(DataGridView dgconteo
                                    , DataGridView dgtotalesaceptados
                                    , DataGridView dgTotalesRechazados
                                    , Form myfrm
                                    , Label lbltotales
                                    , Button btnExportar
            )
        {
            //this.LogTextBox = logTextBox;
            Dgconteo = dgconteo;
            DgTotalesAceptados = dgtotalesaceptados;
            DgTotalesRechazados = dgTotalesRechazados;
            Myfrm = myfrm;
            LblTotales = lbltotales;
            BtnExportar = btnExportar;

            // データバインディング
            Binding();

            // 設定値読込
            ReadSetting();


            polling = new Thread(KeepAliveRun);
            polling.Start();
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
                            LeadFileName = xmlReader.ReadString();
                            break;

                        case "TcpPortNo":
                            TcpPortNo = xmlReader.ReadString();
                            break;
                        case "TcpAddress":
                            TcpAddress = xmlReader.ReadString();
                            break;
                        case "TcpDownloadFile":
                            TcpDownloadFile = xmlReader.ReadString();
                            break;
                        case "TcpUploadFolder":
                            TcpUploadFolder = xmlReader.ReadString();
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
            validate.Dispose();
            validate = null;


            shoudStop = true;
            polling.Join();

            if (tcpClient != null)
            {
                tcpClient.Dispose();
                tcpClient = null;
            }
        }

        #endregion

        #region 更新通知

        /// <summary>
        /// 更新通知
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged(string info)
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
        private string leadFileName = string.Empty;
        /// <summary>
        /// 送信メッセージファイル
        /// </summary>
        public string LeadFileName
        {
            get => leadFileName;
            set
            {
                leadFileName = value;
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
                StreamReader sr = new StreamReader(LeadFileName, Encoding.Default);
                SendMessage = string.Empty;
                string line, str = string.Empty;
                if (File.Exists(LeadFileName))
                {
                    while ((line = sr.ReadLine()) != null) //テキストファイルを一行づつ読み込む
                    {
                        str = str + line + "\r\n";
                    }
                    SendMessage = str;
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

                //OpenFileDialog dialog = new OpenFileDialog();
                //dialog.Title = "Message File Open";
                //dialog.Filter = "テキストファイル|*.txt";
                //dialog.InitialDirectory = startupPath + "\\Message";

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
        private string sendMessage = string.Empty;
        /// <summary>
        /// メッセージ送信
        /// </summary>
        public string SendMessage
        {
            get => sendMessage;
            set
            {
                sendMessage = value;
                NotifyPropertyChanged("SendMessage");
            }
        }

        #endregion

        #region メッセージログ

        /// <summary>
        /// メッセージログ
        /// </summary>
        private string messageLog = string.Empty;
        /// <summary>
        /// メッセージログ
        /// </summary>
        public string MessageLog
        {
            get => messageLog;
            set
            {
                messageLog = value;
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
            if (string.IsNullOrEmpty(message) == false)
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

            if ((message != null) && (message != string.Empty))
            {
                sb.Append("\n");
                sb.Append("Message: ");
                sb.Append(messageType.ToString());
                sb.Append(" ");
                sb.Append(messageName.ToString());
                sb.Append("\n");
                sb.Append(message);
            }

            //this.UpdateLog(logType.ToString(), sb.ToString());
        }

        /// <summary>
        /// ログ更新ロックオブジェクト
        /// </summary>
        private readonly object updateLogLock = new object();

        /// <summary>
        /// ログ更新
        /// </summary>
        /// <param name="direction"></param>
        /// <param name="message"></param>
        //private void UpdateLog(string direction, string message)
        //{
        //    lock (updateLogLock)
        //    {
        //        this.MessageLog = this.GetLogText(direction, message);
        //        this.ScrollToEnd();

        //    }
        //}

        //private void ScrollToEnd()
        //{
        //    try
        //    {
        //        this.LogTextBox.Dispatcher.InvokeAsync(() => this.LogTextBox.ScrollToEnd());
        //    }
        //    catch
        //    {
        //    }
        //}

        /// <summary>
        /// ログ更新
        /// </summary>
        /// <param name="direction"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        private string GetLogText(string direction, string message)
        {
            return GetLogText(MessageLog, System.DateTime.Now, direction, message);
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
        public string TcpPortNo { get; set; }
        /// <summary>
        /// 接続先IPアドレス
        /// </summary>
        public string TcpAddress { get; set; }

        /// <summary>
        /// TCPサーバー接続
        /// </summary>
        public ICommand TcpConnect { get; set; }
        /// <summary>
        /// TCPサーバー接続処理
        /// </summary>
        public bool TcpConnectFunc()
        {
            if (tcpClient != null)
            {
                // メッセージログの末尾に追加
                MessageLog = MessageLog + "<< Please Disconnect TCP server. >>\n";
                MessageBox.Show("<< Please Disconnect TCP server. >>\n");
                //this.ScrollToEnd();
                return false    ;
            }

            try
            {
                if (int.TryParse(TcpPortNo, out int portNo) == false)
                {
                    MessageLog = MessageLog + "<< TCP portNo error. >>\n";
                    //this.ScrollToEnd();
                    MessageBox.Show("<< TCP portNo error. >>\n");
                    return false;
                }

                // ログ出力
                string logText = string.Format("PortNo={0}, IP Address={1}", TcpPortNo, TcpAddress);
                //this.UpdateLog("Connect to TCP server", logText);

                // SorterIotInterface オブジェクト生成
                tcpClient = new TcpClientProtocol(TcpAddress, portNo);

                tcpClient.receiveFunc += ReceiveFunc;
                //this.tcpClient.TcpLogEvent += this.TcpLogUpdate;

                // 自動実行機能
                autoAction = new AutoAction(tcpClient);
                return true;
            }
            catch (Exception ex)
            {
                MessageLog = MessageLog + ex.ToString() + "\n\n";
                //this.ScrollToEnd();
                MessageBox.Show(ex.Message);
                TcpDisconnectFunc();
                return false;
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
                if (tcpClient == null)
                {
                    return;
                }

                //this.UpdateLog("Disconnect from TCP server", string.Empty);

                // サーバー切断
                tcpClient.Dispose();
                tcpClient = null;
            }
            catch (Exception ex)
            {
                MessageLog = MessageLog + ex.ToString() + "\n\n";
                //this.ScrollToEnd();
                MessageBox.Show(ex.Message);
            }
        }

        #endregion

        #region Tcp ダウンロードファイル

        /// <summary>
        /// Tcp ダウンロードファイル
        /// </summary>
        private string tcpDownloadFile = string.Empty;
        /// <summary>
        /// Tcp ダウンロードファイル
        /// </summary>
        public string TcpDownloadFile
        {
            get => tcpDownloadFile;
            set
            {
                tcpDownloadFile = value;
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

                //OpenFileDialog dialog = new OpenFileDialog();
                //dialog.Title = "TCP download File Open";
                //dialog.Filter = "全てのファイル|*.*";
                //dialog.InitialDirectory = startupPath + "\\TcpDownloadFile";

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
        private string tcpUploadFolder = string.Empty;
        /// <summary>
        /// TCP アップロードフォルダ
        /// </summary>
        public string TcpUploadFolder
        {
            get => tcpUploadFolder;
            set
            {
                tcpUploadFolder = value;
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

                System.Windows.Forms.FolderBrowserDialog fbd = new System.Windows.Forms.FolderBrowserDialog
                {
                    Description = "Upload Folder Slect",
                    RootFolder = Environment.SpecialFolder.Desktop,
                    SelectedPath = startupPath + "\\UploadFolder",
                    ShowNewFolderButton = true
                };

                //ダイアログを表示する
                if (fbd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    // 選択されたフォルダで更新
                    TcpUploadFolder = fbd.SelectedPath.Replace(startupPath, ".");
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
            if (tcpClient == null)
            {
                return;
            }

            try
            {
                //this.UpdateLog("Simulator send KeepAlive(TCP). ", "");
                tcpClient.Send("");
            }
            catch (Exception ex)
            {
                MessageLog = MessageLog + ex.ToString() + "\n\n";
                //this.ScrollToEnd();
            }
        }

        #endregion

        #region TCPメッセージ送信(JSON生)

        /// <summary>
        /// TCPメッセージ送信(JSON生)処理コマンド
        /// </summary>
        public ICommand TcpSendJson { get; set; }


        /// <summary>
        /// メッセージ送信(JSON生)処理
        /// </summary>
        public void TcpSendJsonFunc()
        {
            if (tcpClient == null)
            {
                return;
            }

            try
            {
                if (string.IsNullOrEmpty(sendMessage) == true)
                {
                    //this.UpdateLog("send message(TCP) is null or empty", "");
                    return;
                }

                string message = sendMessage;
                message = message.Replace("\r\n", "");
                message = message.Replace("\n", "");
                message = message.Replace("\t", "");

                SorterIfMessageFormat obj = SorterIfMessageFormat.DeSerialize<SorterIfMessageFormat>(message);
                MessageType type = obj.Type;
                MessageName name = obj.Name;

                StringBuilder sb = new StringBuilder();
                // validate message
                if (string.IsNullOrEmpty(message) == false)
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
                    if (File.Exists(TcpDownloadFile) == false)
                    {
                        //this.UpdateLog(string.Format("({0}) not exist.\nPlease setting file path [Download Src File].", this.TcpDownloadFile), "");
                        return;
                    }

                    //this.UpdateLog("Simulator send JSON message(TCP) with file download. ", sb.ToString());
                    tcpClient.Send(message, TcpDownloadFile);
                }
                else
                {
                    //this.UpdateLog("Simulator send JSON message(TCP). ", sb.ToString());
                    tcpClient.Send(message);
                }
            }
            catch (Exception ex)
            {
                MessageLog = MessageLog + ex.ToString() + "\n\n";
                //this.ScrollToEnd();
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
            if (string.IsNullOrEmpty(message) == false)
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



            //if (message.Contains("\"GetDeviceStatus\""))
            //{
            //    string newmensaje = message.Replace("JSON schema validate : common=OK, detail=NG.", "");
            //    deviceStatus = JsonConvert.DeserializeObject<RootDeviseStatus>(newmensaje);
            //    if (deviceStatus.Detail.ApplicationStatus != "")
            //    {
            //        if (deviceStatus.Detail.ApplicationStatus == "Counting Ready")
            //        {
            //            cargarinformacion();
            //        }
            //    }
            //}


            if (message.Contains("\"GetDenominationInformation\""))
            {
                string newmensaje = message.Replace("JSON schema validate : common=OK, detail=OK.", "");
                rootDenominaciones = JsonConvert.DeserializeObject<RootDenominaciones>(newmensaje);
            }

            if (message.Contains("\"GetCurrency\""))
            {
                string newmensaje = message.Replace("JSON schema validate : common=OK, detail=OK.", "");
                rootCurrency = JsonConvert.DeserializeObject<RootCurrency>(newmensaje);
            }

            if (message.Contains("\"ButtonTouched\""))
            {
                if (message.Contains("\"CANCEL\""))
                {
                    Limpiar();
                }
            }

            if (message.Contains("\"NoteTransported\""))
            {
                string newmensaje = message.Replace("JSON schema validate : common=OK, detail=NG.", "");
                Myfrm.Invoke((MethodInvoker)delegate
                {
                    Root colection = JsonConvert.DeserializeObject<Root>(newmensaje);
                    ListaRoot.Add(colection);
                    ListaNote.AddRange(colection.Detail.Notes);
                    LblTotales.Text = ListaNote.Count().ToString();

                });
            }

            if (message.Contains("\"TransactionDetected\""))
            {
                string newmensaje = message.Replace("JSON schema validate : common=OK, detail=NG.", "");
                Myfrm.Invoke((MethodInvoker)delegate
                {
                    rootTotal = JsonConvert.DeserializeObject<RootTotal>(message);

                    var listdeno = this.rootDenominaciones.Detail.Denomination;
                    var headerbody = this.rootTotal.Detail;
                    var currencybody = this.rootCurrency.Detail;

                    string trankey = "";
                    DateTime startdatatime = DateTime.Now;
                    DateTime enddatatime = DateTime.Now;

                    if (headerbody != null)
                    {
                        trankey = headerbody.TranKey == null ? string.Empty : headerbody.TranKey;
                        startdatatime = headerbody.TransactionData.StartDateTime;
                        enddatatime = headerbody.TransactionData.EndDateTime;
                    }

                    var liscomple = (from ld in listdeno
                                     join li in ListaNote on new { ld.DenomiIndex } equals new { li.DenomiIndex }
                                     where
                                     ld.CurrencyIndex == currencybody.CurrencyIndex &&
                                     li.Reject == false
                                     select new
                                     {
                                         TranKey = trankey,
                                         StartDateTime = startdatatime,
                                         EndDateTime = enddatatime,
                                         Rechazo = li.Reject,
                                         ld.Value,
                                         li.SerialNumber
                                     }
                        ).ToList();

                    Dgconteo.DataSource = liscomple;


                    var listotalaceptado = (from ld in listdeno
                                            join li in ListaNote on new { ld.DenomiIndex } equals new { li.DenomiIndex }
                                            where ld.CurrencyIndex == currencybody.CurrencyIndex && li.Reject == false
                                            orderby ld.Value
                                            group new { ld.Value } by ld.Value into grupo
                                            select new
                                            {
                                                Denom = "$ " + grupo.Key.ToString(),
                                                Cant = grupo.Count(),
                                                TotalValor = (grupo.Key * grupo.Count())
                                            }).ToList();


                    int totalcant = listotalaceptado.Sum(item => item.Cant);
                    int totalvalor = listotalaceptado.Sum(item => item.TotalValor);

                    var o1 = new { Denom = "TOTALES ", Cant = totalcant, TotalValor = totalvalor };
                    listotalaceptado.Add(o1);
                    DgTotalesAceptados.DataSource = listotalaceptado.ToList();


                    var resulttotalesR = (from li in ListaNote
                                          where li.Reject == true
                                          select new { key = 1 }).GroupBy(n => n)
                                    .Select(c => new { Cant = c.Count() });


                    DgTotalesRechazados.DataSource = resulttotalesR.ToList();


                    this.BtnExportar.Enabled = true;
                });
            }



            autoAction.ReceiveProccess("", type, name, message);

        }

        public void ExportarInformacion(string usuario)
        {
            Myfrm.Invoke((MethodInvoker)delegate
            {
                //var sb = new StringBuilder();

                //var headers = Dgconteo.Columns.Cast<DataGridViewColumn>();
                //sb.AppendLine(string.Join(",", headers.Select(column => "\"" + column.HeaderText + "\"").ToArray()));

                //foreach (DataGridViewRow row in Dgconteo.Rows)
                //{
                //    var cells = row.Cells.Cast<DataGridViewCell>();
                //    sb.AppendLine(string.Join(",", cells.Select(cell => "\"" + cell.Value + "\"").ToArray()));
                //}
                var pathtxt = Directory.GetCurrentDirectory() + "\\TcpDownloadFile\\" + usuario + DateTime.Now.ToString("yyyyMMddhhmmss") + ".csv";
                SaveDataGridViewToCSV(pathtxt);
                Limpiar();
                this.BtnExportar.Enabled = false;
            });

        }

        void SaveDataGridViewToCSV(string filename)
        {
            // Choose whether to write header. Use EnableWithoutHeaderText instead to omit header.
            Dgconteo.ClipboardCopyMode = DataGridViewClipboardCopyMode.EnableAlwaysIncludeHeaderText;
            // Select all the cells
            Dgconteo.SelectAll();
            // Copy selected cells to DataObject
            DataObject dataObject = Dgconteo.GetClipboardContent();
            // Get the text of the DataObject, and serialize it to a file
            File.WriteAllText(filename, dataObject.GetText(TextDataFormat.CommaSeparatedValue));
        }

        public void Limpiar()
        {
            Myfrm.Invoke((MethodInvoker)delegate
            {
                ListaNote = new List<Note>();
                ListaRoot = new List<Root>();
                rootTotal = new RootTotal();
               
                Dgconteo.DataSource = ListaNote.ToList();
                DgTotalesAceptados.DataSource = null;
                DgTotalesRechazados.DataSource = null;
                LblTotales.Text = "0";
            });
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

        //private void TcpLogUpdate(string log)
        //{
        //    //this.UpdateLog("TcpLog", log);
        //}

        #endregion

        #region 自動キープアライブ

        private bool shoudStop = false;
        private void KeepAliveRun()
        {
            int count = 60;

            while (shoudStop == false)
            {
                Thread.Sleep(1000);

                if (tcpClient == null)
                {
                    continue;
                }

                try
                {
                    count--;
                    if (count <= 0)
                    {
                        count = 60;
                        tcpClient.Send("");
                    }
                }
                catch (Exception ex)
                {
                    MessageLog = MessageLog + ex.ToString() + "\n\n";
                    //this.ScrollToEnd();
                }
            }
        }

        #endregion

    }
}
