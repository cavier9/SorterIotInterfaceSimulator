using Glory.SorterInterface.Message;
using Glory.SorterInterface.SorterInterfaceClient;
using Glory.SorterInterface.SorterInterfaceClient.Client;
using Glory.SorterInterface.SorterInterfaceClient.Log;
using Microsoft.Win32;
using Newtonsoft.Json;
using SorterIotInterfaceSimulator.Common.Logic;
using SorterIotInterfaceSimulator.CustomView;
using System;
using System.ComponentModel;
using System.IO;
using System.Text;
using System.Windows.Input;
using System.Xml;

namespace SorterIotInterfaceSimulator.ViewModel
{
    /// <summary>
    /// メインウィンドウビューモデル
    /// </summary>
    public class MainWindowViewModel : INotifyPropertyChanged, IDisposable
    {
        /// <summary>
        /// Sorter IoT Interface Client
        /// </summary>
        ISorterIfClient client = null;

        /// <summary>
        /// Validate
        /// </summary>
        Validate validate = new Validate();

        CustomTextBox logTextBox;

        /// <summary>
        /// 自動実行機能
        /// </summary>
        AutoAction autoAction;

        #region コンストラクタ

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
                        case "HostName":
                            this.HostName = xmlReader.ReadString();
                            break;
                        case "UserName":
                            this.UserName = xmlReader.ReadString();
                            break;
                        case "Password":
                            this.Password = xmlReader.ReadString();
                            break;
                        case "MqttClientName":
                            this.SystemName = xmlReader.ReadString();
                            break;
                        //case "SystemVersion":
                        //    this.SystemVersion = xmlReader.ReadString();
                        //    break;
                        case "DeviceName":
                            this.DeviceName = xmlReader.ReadString();
                            break;
                        case "PortNo":
                            this.PortNo = xmlReader.ReadString();
                            break;
                        // @@@ add USF-50S_TLSv1.2対応 2018.08.02 by Hemmi ↓
                        case "TLSVersion":
                            SetTLSVersion(xmlReader.ReadString());
                            break;
                        // @@@ add USF-50S_TLSv1.2対応 2018.08.02 by Hemmi ↑
                        case "Cirtificate":
                            this.Cirtificate = xmlReader.ReadString();
                            break;

                        case "LeadFileName":
                            this.LeadFileName = xmlReader.ReadString();
                            break;
                        case "DownloadFolderName":
                            this.DownloadFolderName = xmlReader.ReadString();
                            break;
                        case "UploadFolderName":
                            this.UploadFolderName = xmlReader.ReadString();
                            break;
                        case "Topic":
                            this.Topic = xmlReader.ReadString();
                            break;

                        case "FtpHostName":
                            this.FtpHostName = xmlReader.ReadString();
                            break;
                        case "FtpPortNo":
                            this.FtpPortNo = xmlReader.ReadString();
                            break;
                        case "FtpUserName":
                            this.FtpUserName = xmlReader.ReadString();
                            break;
                        case "FtpPassword":
                            this.FtpPassword = xmlReader.ReadString();
                            break;
                        case "FtpTls":
                            this.FtpTls = xmlReader.ReadString();
                            break;
                        case "FtpCirtificate":
                            this.FtpCirtificate = xmlReader.ReadString();
                            break;
                        case "FtpSslMode":
                            this.FtpSslMode = xmlReader.ReadString();
                            break;
                        case "FtpFolderName":
                            this.FtpFolderName = xmlReader.ReadString();
                            break;

                        default:
                            break;
                    }
                }
            }

            this.SystemVersion = "0.0";
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

            if (this.client != null)
            {
                this.client.DisconnectBroker();
                this.client.Dispose();
                this.client = null;
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

        #region IoTサーバー接続

        /// <summary>
        /// ユーザ名
        /// </summary>
        public String UserName { get; set; }
        /// <summary>
        /// パスワード
        /// </summary>
        public String Password { get; set; }
        /// <summary>
        /// ホスト名
        /// </summary>
        public String HostName { get; set; }

        /// <summary>
        /// システム名
        /// </summary>
        public String SystemName { get; set; }
        /// <summary>
        /// システムバージョン
        /// </summary>
        public String SystemVersion { get; set; }
        /// <summary>
        /// デバイス名
        /// </summary>
        public String DeviceName { get; set; }

        /// <summary>
        /// TLS ポート番号
        /// </summary>
        public String PortNo { get; set; }
        // @@@ add USF-50S_TLSv1.2対応 2018.08.02 by Hemmi ↓
        /// <summary>
        /// TLSバージョン
        /// </summary>
        private SslProtocols tlsVersion = SslProtocols.TLSv1_0;
        public SslProtocols TLSVersion
        {
            get
            {
                return this.tlsVersion;
            }
        }
        private void SetTLSVersion(string val)
        {
            if ("1.1" == val)
            {
                this.tlsVersion = SslProtocols.TLSv1_1;
            }
            else if ("1.2" == val)
            {
                this.tlsVersion = SslProtocols.TLSv1_2;
            }
            else
            {
                this.tlsVersion = SslProtocols.TLSv1_0;
            }
        }
        // @@@ add USF-50S_TLSv1.2対応 2018.08.02 by Hemmi ↑
        /// <summary>
        /// TLS 認証
        /// </summary>
        public String Cirtificate { get; set; }

        /// <summary>
        /// サーバー接続(平文 Port 1883)コマンド送信
        /// </summary>
        public ICommand SendConnect { get; set; }
        /// <summary>
        /// サーバー接続(平文 Port 1883)処理
        /// </summary>
        public void SendConnectFunc()
        {
            if (this.client != null)
            {
                // メッセージログの末尾に追加
                this.MessageLog = this.MessageLog + "<< Please Disconnect Broker. >>\n";
                this.ScrollToEnd();
                return;
            }

            try
            {
                int portNo = 1883;
                Int32.TryParse(this.PortNo, out portNo);

                // SorterIotInterface オブジェクト生成
                ISorterIfClient sorterIotInterfaceClient = new SorterIfClient(this.DeviceName, this.SystemName, this.SystemVersion, this.HostName);

                // サーバー接続処理共通処理
                this.SendConnectFuncCommon(sorterIotInterfaceClient);
            }
            catch (Exception ex)
            {
                this.MessageLog = this.MessageLog + ex.ToString() + "\n\n";
                this.ScrollToEnd();
            }
        }

        /// <summary>
        /// サーバー接続(TLS)コマンド送信
        /// </summary>
        public ICommand SendConnectTls { get; set; }
        /// <summary>
        /// サーバー接続(TLS)処理
        /// </summary>
        public void SendConnectTlsFunc()
        {
            if (this.client != null)
            {
                // メッセージログの末尾に追加
                this.MessageLog = this.MessageLog + "<< Please Disconnect Broker. >>\n";
                this.ScrollToEnd();
                return;
            }

            try
            {
                bool cirtificate = (this.Cirtificate.ToLower() == "true");
                int portNo = 8883;
                Int32.TryParse(this.PortNo, out portNo);

                // @@@ chg USF-50S_TLSv1.2対応 2018.08.02 by Hemmi ↓
                // SorterIotInterface オブジェクト生成
                ISorterIfClient sorterIotInterfaceClient =
                    new SorterIfClient(this.DeviceName, this.SystemName, this.SystemVersion, this.HostName, portNo, true, cirtificate, this.TLSVersion);
                // @@@ chg USF-50S_TLSv1.2対応 2018.08.02 by Hemmi ↑

                // サーバー接続処理共通処理
                this.SendConnectFuncCommon(sorterIotInterfaceClient);
            }
            catch (Exception ex)
            {
                this.MessageLog = this.MessageLog + ex.ToString() + "\n\n";
                this.ScrollToEnd();
            }
        }

        /// <summary>
        /// サーバー接続処理共通処理
        /// </summary>
        /// <param name="sorterIotInterfaceClient"></param>
        private void SendConnectFuncCommon(ISorterIfClient sorterIotInterfaceClient)
        {
            // SorterIotInterface オブジェクト登録
            this.client = sorterIotInterfaceClient;

            // ログ受信処理を登録
            this.client.MessageLog.MessageLogEventHandler += this.LogReceive;

            // ログ出力
            string logText = String.Format("UserName={0}, Password={1}", this.UserName, this.Password);
            this.UpdateLog("Connect to Broker", logText);

            // サーバ接続
            this.client.ConnectBroker(this.UserName, this.Password, this.SystemName);

            // サブスクライブ
            this.client.SendSubscribe("/glory/sorter/event/#");
            this.client.SendSubscribe(string.Format("/glory/sorter/{0}/event/#", this.DeviceName));
            this.client.SendSubscribe(string.Format("/glory/sorter/{0}/response", this.DeviceName));
            this.client.SendSubscribe(string.Format("/glory/sorter/{0}/+/response", this.DeviceName));

            // サブスクライブ(IBM IoT Platform用)
            this.client.SendSubscribe(string.Format("iot-2/evt/+/fmt/json"));

            // FTP接続
            this.FtpSettingFunc();

            // 自動実行機能
            this.autoAction = new AutoAction(this.client, this.DeviceName);
        }

        #endregion

        #region IoTサーバー切断

        /// <summary>
        /// サーバー切断コマンド送信
        /// </summary>
        public ICommand SendDisconnect { get; set; }
        /// <summary>
        /// サーバー切断コマンド送信
        /// </summary>
        public void SendDisconnectFunc()
        {
            try
            {
                if (this.client == null)
                {
                    return;
                }

                this.UpdateLog("Disconnect from Broker", string.Empty);

                // サーバー切断
                this.client.DisconnectBroker();

                // SorterIotInterface オブジェクト削除
                this.client.Dispose();
                this.client = null;
            }
            catch (Exception ex)
            {
                this.MessageLog = this.MessageLog + ex.ToString() + "\n\n";
                this.ScrollToEnd();
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

                if (dialog.ShowDialog() == true)
                {
                    this.LeadFileName = dialog.FileName.Replace(startupPath, ".");

                    //LeadMessageFunc();
                }
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

        #region メッセージ送信(JSON生)

        /// <summary>
        /// Topic
        /// </summary>
        public String Topic { get; set; }

        /// <summary>
        /// メッセージ送信(JSON生)処理コマンド
        /// </summary>
        public ICommand SendPublishJson { get; set; }
        /// <summary>
        /// メッセージ送信(JSON生)処理
        /// </summary>
        public void SendPublishJsonFunc()
        {
            if (this.client == null)
            {
                return;
            }

            try
            {
                //this.UpdateLog("Simulator send JSON message(MQTT)", this.sendMessage);

                string message = this.sendMessage;
                message = message.Replace("\r\n", "");
                message = message.Replace("\n", "");
                message = message.Replace("\t", "");
                this.client.SendPublish(this.Topic, message, false);
            }
            catch (Exception ex)
            {
                this.MessageLog = this.MessageLog + ex.ToString() + "\n\n";
                this.ScrollToEnd();
            }
        }

        #endregion

        #region Subscribe送信

        /// <summary>
        /// Subscribe送信処理コマンド
        /// </summary>
        public ICommand SendSubscribe { get; set; }
        /// <summary>
        /// Subscribe送信処理
        /// </summary>
        public void SendSubscribeFunc()
        {
            if (this.client == null)
            {
                return;
            }

            try
            {
                //this.UpdateLog("Simulator send Subscribe", this.Topic);
                this.client.SendSubscribe(this.Topic);
            }
            catch (Exception ex)
            {
                this.MessageLog = this.MessageLog + ex.ToString() + "\n\n";
                this.ScrollToEnd();
            }
        }

        #endregion

        #region UnSubscribe送信

        /// <summary>
        /// UnSubscribe送信処理コマンド
        /// </summary>
        public ICommand SendUnSubscribe { get; set; }
        /// <summary>
        /// UnSubscribe送信処理
        /// </summary>
        public void SendUnSubscribeFunc()
        {
            if (this.client == null)
            {
                return;
            }

            try
            {
                //this.UpdateLog("Simulator send UnSubscribe", this.Topic);
                this.client.SendUnSubscribe(this.Topic);
            }
            catch (Exception ex)
            {
                this.MessageLog = this.MessageLog + ex.ToString() + "\n\n";
                this.ScrollToEnd();
            }
        }

        #endregion

        #region ダウンロードフォルダ指定

        /// <summary>
        /// ダウンロードフォルダ
        /// </summary>
        private String downloadFolderName = String.Empty;
        /// <summary>
        /// ダウンロードフォルダ
        /// </summary>
        public String DownloadFolderName
        {
            get { return this.downloadFolderName; }
            set
            {
                this.downloadFolderName = value;
                NotifyPropertyChanged("DownloadFolderName");
            }
        }

        /// <summary>
        /// ダウンロードファイル選択処理コマンド
        /// </summary>
        public ICommand SelectDownloadFolder { get; set; }
        /// <summary>
        /// ダウンロードファイル選択処理
        /// </summary>
        public void SelectDownloadFolderFunc()
        {
            try
            {
                // アプリケーション実行ファイルパス
                string exePath = Environment.GetCommandLineArgs()[0];
                string exeFullPath = System.IO.Path.GetFullPath(exePath);
                string startupPath = System.IO.Path.GetDirectoryName(exeFullPath);

                System.Windows.Forms.FolderBrowserDialog fbd = new System.Windows.Forms.FolderBrowserDialog();
                fbd.Description = "Download Folder Slect";
                fbd.RootFolder = Environment.SpecialFolder.Desktop;
                fbd.SelectedPath = startupPath + "\\DownloadFolder";
                fbd.ShowNewFolderButton = true;

                if (fbd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    this.DownloadFolderName = fbd.SelectedPath.Replace(startupPath, ".");
                }
            }
            catch
            {
                return;
            }
        }
        #endregion

        #region アップロードフォルダ指定

        /// <summary>
        /// ダアップロードフォルダ
        /// </summary>
        private String uploadFolderName = String.Empty;
        /// <summary>
        /// アップロードフォルダ
        /// </summary>
        public String UploadFolderName
        {
            get { return this.uploadFolderName; }
            set
            {
                this.uploadFolderName = value;
                NotifyPropertyChanged("UploadFolderName");
            }
        }

        /// <summary>
        /// アップロードフォルダ選択処理コマンド
        /// </summary>
        public ICommand SelectUploadFolder { get; set; }
        /// <summary>
        /// アップロードフォルダ選択処理
        /// </summary>
        public void SelectUploadFolderFunc()
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
                    this.UploadFolderName = fbd.SelectedPath.Replace(startupPath, ".");
                }
            }
            catch
            {
                return;
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
                {
                    dynamic logConvert = JsonConvert.DeserializeObject(message);
                    string logMessage = JsonConvert.SerializeObject(logConvert, Newtonsoft.Json.Formatting.Indented);
                    sb.Append(logMessage);
                }
            }

            this.UpdateLog(logType.ToString(), sb.ToString());

            this.autoAction.ReceiveProccess(topic, messageType, messageName, message);
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
            lock (updateLogLock)
            {
                this.MessageLog = this.GetLogText(direction, message);
                this.ScrollToEnd();
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

        #region FTPサーバー接続

        /// <summary>
        /// ホスト名
        /// </summary>
        public String FtpHostName { get; set; }
        /// <summary>
        /// ポート番号
        /// </summary>
        public String FtpPortNo { get; set; }
        /// <summary>
        /// ユーザ名
        /// </summary>
        public String FtpUserName { get; set; }
        /// <summary>
        /// パスワード
        /// </summary>
        public String FtpPassword { get; set; }

        /// <summary>
        /// FTP TLS使用･未使用
        /// </summary>
        public String FtpTls { get; set; }
        /// <summary>
        /// FTP サーバ認証
        /// </summary>
        public String FtpCirtificate { get; set; }
        /// <summary>
        /// FTP SSLﾓｰﾄﾞ(Explicit / Implicit)
        /// </summary>
        public String FtpSslMode { get; set; }

        /// <summary>
        /// FTP フォルダ
        /// </summary>
        private String ftpFolderPath = String.Empty;
        /// <summary>
        /// FTP フォルダ
        /// </summary>
        public String FtpFolderPath
        {
            get { return this.ftpFolderPath; }
            set
            {
                this.ftpFolderPath = value;
                NotifyPropertyChanged("FtpFolderPath");
            }
        }

        /// <summary>
        /// FTP 設定更新
        /// </summary>
        public ICommand FtpSetting { get; set; }
        /// <summary>
        /// FTP 設定更新処理
        /// </summary>
        public void FtpSettingFunc()
        {
            if (this.client == null)
            {
                // メッセージログの末尾に追加
                this.MessageLog = this.MessageLog + "<< Please Connect MQTT Broker. >>\n";
                this.ScrollToEnd();
                return;
            }

            try
            {
                bool ftpTls = (this.FtpTls.ToLower() == "true");
                bool ftpCirtificate = (this.FtpCirtificate.ToLower() == "true");
                SorterIfClient.FTP_SSL_MODE ftpSslMode = SorterIfClient.FTP_SSL_MODE.EXPLICIT;
                if (this.FtpSslMode.ToLower() == "implicit")
                {
                    ftpSslMode = SorterIfClient.FTP_SSL_MODE.IMPLICIT;
                }
                // 設定更新
                this.client.FtpConfigSetting(
                                            this.FtpHostName,
                                            Convert.ToInt32(this.FtpPortNo),
                                            this.FtpUserName,
                                            this.FtpPassword,
                                            ftpTls,
                                            ftpCirtificate,
                                            ftpSslMode);
            }
            catch (Exception ex)
            {
                this.MessageLog = this.MessageLog + ex.ToString() + "\n\n";
                this.ScrollToEnd();
            }
        }

        /// <summary>
        /// FTP フォルダ選択処理コマンド
        /// </summary>
        public ICommand SelectFtpFolder { get; set; }
        /// <summary>
        /// FTP フォルダ選択処理
        /// </summary>
        public void SelectFtpFolderFunc()
        {
            try
            {
                // アプリケーション実行ファイルパス
                string exePath = Environment.GetCommandLineArgs()[0];
                string exeFullPath = System.IO.Path.GetFullPath(exePath);
                string startupPath = System.IO.Path.GetDirectoryName(exeFullPath);

                System.Windows.Forms.FolderBrowserDialog fbd = new System.Windows.Forms.FolderBrowserDialog();
                fbd.Description = "FTP Folder Slect";
                fbd.RootFolder = Environment.SpecialFolder.Desktop;
                //fbd.SelectedPath = startupPath + "\\FtpFolder";
                fbd.ShowNewFolderButton = true;

                //ダイアログを表示する
                if (fbd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    // 選択されたフォルダで更新
                    this.FtpFolderPath = fbd.SelectedPath.Replace(startupPath, ".");
                }
            }
            catch
            {
                return;
            }
        }

        #endregion

        #region FTP操作

        /// <summary>
        /// FTP上のフォルダ
        /// </summary>
        private String ftpFolderName = String.Empty;
        /// <summary>
        /// FTP上のフォルダ
        /// </summary>
        public String FtpFolderName
        {
            get { return this.ftpFolderName; }
            set
            {
                this.ftpFolderName = value;
                NotifyPropertyChanged("FtpFolderName");
            }
        }

        /// <summary>
        /// FTP フォルダ作成処理コマンド
        /// </summary>
        public ICommand CreateDirFTP { get; set; }
        public void CreateDirFTPFunc()
        {
            if (this.client == null)
            {
                // メッセージログの末尾に追加
                this.MessageLog = this.MessageLog + "<< Please Connect MQTT Broker. >>\n";
                this.ScrollToEnd();
                return;
            }

            try
            {
                this.client.CreateDirFTP("", this.ftpFolderName);
            }
            catch (Exception ex)
            {
                this.MessageLog = this.MessageLog + ex.ToString() + "\n\n";
                this.ScrollToEnd();
            }
        }

        /// <summary>
        /// FTP フォルダ作成処理コマンド
        /// </summary>
        public ICommand RemoveDirFTP { get; set; }
        public void RemoveDirFTPFunc()
        {
            if (this.client == null)
            {
                // メッセージログの末尾に追加
                this.MessageLog = this.MessageLog + "<< Please Connect MQTT Broker. >>\n";
                this.ScrollToEnd();
                return;
            }

            try
            {
                this.client.RemoveDirFTP("", this.ftpFolderName);
            }
            catch (Exception ex)
            {
                this.MessageLog = this.MessageLog + ex.ToString() + "\n\n";
                this.ScrollToEnd();
            }
        }
        /// <summary>
        /// FTP アップロード処理コマンド
        /// </summary>
        public ICommand UploadFilesFTP { get; set; }
        public void UploadFilesFTPFunc()
        {
            if (this.client == null)
            {
                // メッセージログの末尾に追加
                this.MessageLog = this.MessageLog + "<< Please Connect MQTT Broker. >>\n";
                this.ScrollToEnd();
                return;
            }

            try
            {
                this.client.UploadFilesFTP("", this.ftpFolderName, this.UploadFolderName);
            }
            catch (Exception ex)
            {
                this.MessageLog = this.MessageLog + ex.ToString() + "\n\n";
                this.ScrollToEnd();
            }
        }

        /// <summary>
        /// FTP ダウンロード処理コマンド
        /// </summary>
        public ICommand DownloadFilesFTP { get; set; }
        public void DownloadFilesFTPFunc()
        {
            if (this.client == null)
            {
                // メッセージログの末尾に追加
                this.MessageLog = this.MessageLog + "<< Please Connect MQTT Broker. >>\n";
                this.ScrollToEnd();
                return;
            }

            try
            {
                this.client.DownloadFilesFTP("", this.ftpFolderName, this.DownloadFolderName);
            }
            catch (Exception ex)
            {
                this.MessageLog = this.MessageLog + ex.ToString() + "\n\n";
                this.ScrollToEnd();
            }
        }

        #endregion
    }
}