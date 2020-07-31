using System;
using System.Collections.Generic;
using Glory.SorterInterface.Message;
using Glory.SorterInterface.MessageDetail;
using Glory.SorterInterface.Protocol.Mqtt;
using Glory.SorterInterface.Ftp;
using System.Security.Cryptography;
using System.Text;
using Glory.SorterInterface.SorterInterfaceClient.Client.Session;

namespace Glory.SorterInterface.SorterInterfaceClient.Client
{
    /// <summary>
    /// Sorter IoT Interface protocol for System
    /// </summary>
    internal class SorterIfProtocol : SorterIfProtocolBase, ISendLowMessage, ISorterIfProtocol
    {
        #region fixed parameter

        /// <summary>
        /// fixed client ID
        /// </summary>
        private const string FIXED_CLIENT_ID = "SorterInterface";

        #endregion

        #region property

        /// <summary>
        /// system name
        /// </summary>
        public string SystemName { get; private set; }

        /// <summary>
        /// system version
        /// </summary>
        public string SystemVersion { get; private set; }

        /// <summary>
        /// device name
        /// </summary>
        public string ConnectDeviceName { get; private set; }

        /// <summary>
        /// sessionID
        /// </summary>
        public int SessionID { get; set; }

        /// <summary>
        /// SorterIfClient
        /// </summary>
        public ISorterIfClient Client { get; private set; }

        #endregion

        #region parameter

        /// <summary>
        /// is connected device
        /// </summary>
        private bool isConnectedDevice = false;

        /// <summary>
        /// lock Object
        /// </summary>
        private Object lockObj = new Object();

        #endregion

        #region constructor

        /// <summary>
        /// constructor (plain text)
        /// </summary>
        /// <param name="client">client</param>
        /// <param name="deviceName">deviceName</param>
        /// <param name="systemName">system name</param>
        /// <param name="systemVersion">system version</param>
        /// <param name="hostName">host name</param>
        public SorterIfProtocol(SorterIfClient client, string deviceName, string systemName, string systemVersion, string hostName)
            : base(NodeType.SYSTEM, hostName)
        {
            this.Client = client;
            this.ConnectDeviceName = deviceName;
            this.SystemName = systemName;
            this.SystemVersion = systemVersion;
            this.SessionID = SessionManager.SESSION_ID_UNDEF;
        }

#if M2MQTT_V3
        // @@@ del USF-50S_TLSv1.2対応 2018.08.02 by Hemmi ↓
        ///// <summary>
        ///// constructor (TLS. Broker certificate)
        ///// </summary>
        ///// <param name="client">client</param>
        ///// <param name="deviceName">deviceName</param>
        ///// <param name="systemName">system name</param>
        ///// <param name="systemVersion">system version</param>
        ///// <param name="hostName">host name</param>
        ///// <param name="port">port Number</param>
        ///// <param name="secure">secure</param>
        ///// <param name="certificate">use server certivicate</param>
        //public SorterIfProtocol(SorterIfClient client, string deviceName, string systemName, string systemVersion, string hostName, int port, bool secure, bool certificate)
        //    : this(client, deviceName, systemName, systemVersion, hostName, port, secure, certificate, SslProtocols.TLSv1_0)
        //{
        //}
        // @@@ del USF-50S_TLSv1.2対応 2018.08.02 by Hemmi ↑

        /// <summary>
        /// constructor (SSL/TLS. Broker certificate)
        /// </summary>
        /// <param name="client">client</param>
        /// <param name="deviceName">deviceName</param>
        /// <param name="systemName">system name</param>
        /// <param name="systemVersion">system version</param>
        /// <param name="hostName">host name</param>
        /// <param name="port">port Number</param>
        /// <param name="secure">secure</param>
        /// <param name="certificate">use server certivicate</param>
        /// <param name="sslProtocol">SSL/TLS protocol</param>
        public SorterIfProtocol(    // @@@ chg USF-50S_TLSv1.2対応 2018.08.02 by Hemmi
            SorterIfClient client, 
            string deviceName, 
            string systemName, 
            string systemVersion, 
            string hostName, 
            int port, 
            bool secure, 
            bool certificate, 
            SslProtocols sslProtocol)
            : base(NodeType.SYSTEM, hostName, port, secure, certificate, sslProtocol)
        {
            this.Client = client;
            this.ConnectDeviceName = deviceName;
            this.SystemName = systemName;
            this.SystemVersion = systemVersion;
            this.SessionID = SessionManager.SESSION_ID_UNDEF;
        }
#endif

        #endregion

        #region IDisposable interface implement

        /// <summary>
        /// Dispose
        /// </summary>
        public new void Dispose()
        {
            base.Dispose();
        }

        #endregion

        #region get device info

        /// <summary>
        /// get device name
        /// </summary>
        /// <returns>device name</returns>
        public string GetConnectDeviceName()
        {
            return this.ConnectDeviceName;
        }

        /// <summary>
        /// is connected device
        /// </summary>
        /// <returns>true: connected / false: disconnected</returns>
        public bool IsConnectedDevice()
        {
            return this.isConnectedDevice;
        }

        /// <summary>
        /// set connected device state
        /// </summary>
        /// <param name="isConnected">is connected</param>
        public void SetConnectedDevice(bool isConnected)
        {
            if (isConnected == false)
            {
                this.SessionID = SessionManager.SESSION_ID_UNDEF;
            }
            this.isConnectedDevice = isConnected;
        }

        #endregion

        #region Connect Broker

        /// <summary>
        /// Connect to Broker
        /// </summary>
        /// <param name="username">user name, connect to broker</param>
        /// <param name="password">pass word, connect to broker</param>
        /// <param name="clientId">client id, connect to broker</param>
        public void ConnectBroker(
            string username,
            string password,
            string clientId)
        {
            IList<string> subscribeTopicList = new List<string>();
            //subscribeTopicList.Add(Topic.GetEventTopic("#"));
            //subscribeTopicList.Add(Topic.GetEventTopic(this.ConnectDeviceName, "#"));
            //subscribeTopicList.Add(Topic.GetResponseTopic(this.ConnectDeviceName));
            //subscribeTopicList.Add("/glory/sorter/" + this.ConnectDeviceName + "/+/response");

            // Connect to Broker
            base.ConnectBroker(clientId, username, password, false, string.Empty, string.Empty, subscribeTopicList);
        }

        #endregion

        #region receive process

        /// <summary>
        /// receive process
        /// </summary>
        /// <param name="topic">topic</param>
        /// <param name="jsonMessage">JSON message</param>
        public override void ReceiveProcess(string topic, string jsonMessage)
        {
            lock (this.lockObj)
            {
                // device name
                string deviceName = Topic.GetDeviceName(topic);
                int sessionId = Topic.GetSessionId(topic);

                // message name
                ReceiveMessage message = new ReceiveMessage(jsonMessage);
                MessageType messageType = message.Type;
                MessageName messageName = message.Name;

                // log
                base.messageLog.SetMessageLog(Log.MessageLog.LogType.Receive, topic, message.Type, message.Name, jsonMessage);
            }
        }

        #endregion
        
        #region Ftp Request command
        
        /// <summary>
        /// ftp resv Jsonmessage
        /// </summary>
        private string ftpJson = string.Empty;

        /// <summary>
        /// ftp downloadFilePath (RequestUpload***Command)
        /// </summary>
        private string ftpDownload = string.Empty;

        /// <summary>
        /// ftp uploadFilePath (RequestDownload***Command)
        /// </summary>
        private string ftpUpload = string.Empty;
        
        /// <summary>
        /// FTP returnProcessResult
        /// </summary>
        /// <param name="ftpPattern">FTPPattern</param>
        /// <param name="result">RESULT_PROCESS</param>
        /// <param name="msg">process message</param>
        private void returnResult(FTPPattern ftpPattern, FtpAccess.RESULT_PROCESS result, string msg)
        {
            // @@@ chg USF-200_機能改善 2018.08.02 by Hemmi ↓
            if (msg == string.Empty)
            {
                base.messageLog.SetMessageLog(Log.MessageLog.LogType.FtpResponse, ftpPattern.ComandPattern + " response " + result);
            }
            else
            {
                base.messageLog.SetMessageLog(Log.MessageLog.LogType.FtpResponse, ftpPattern.ComandPattern + " response " + result + ", msg " + msg);
            }
            // @@@ chg USF-200_機能改善 2018.08.02 by Hemmi ↑
        }


        /// <summary>
        /// FTP configSetting
        /// </summary>
        /// <param name="hostName"></param>
        /// <param name="portNo"></param>
        /// <param name="userName"></param>
        /// <param name="password"></param>
        /// <param name="useSsl"></param>
        /// <param name="cirtificate"></param>
        /// <param name="sslMode">EXPLICIT / IMPLICIT</param>  
        public void settingFTP(string hostName,
                               int portNo,
                               string userName,
                               string password,
                               bool useSsl,
                               bool cirtificate,
                               string sslMode)
        {
            base.SettingFTP(hostName,
                            portNo,
                            userName,
                            password,
                            useSsl,
                            cirtificate,
                            sslMode);
        }


        /// <summary>
        /// FTP returnConnectResult
        /// </summary>
        /// <param name="result">RESULT_PROCESS</param>
        /// <param name="msg">process message</param>
        private void returnConnectResult(FtpAccess.RESULT_PROCESS result, string msg)
        {
            if (result != FtpAccess.RESULT_PROCESS.SUCCESS && result != FtpAccess.RESULT_PROCESS.NONE)
            {
                // log
                base.messageLog.SetMessageLog(Log.MessageLog.LogType.FtpResponse, "FtpConnectResultError:" + result + ":" + msg);
            }
        }
        
        #endregion


        #region FTP UploadFiles

        /// <summary>
        /// UploadFilesFTP
        /// </summary>
        /// <param name="id"></param>
        /// <param name="uploadUri"></param>
        /// <param name="dirPath"></param>
        public override bool UploadFilesFTP(string id, string uploadUri, string dirPath)
        {
            base.ConnectFTP(returnConnectResult, returnResult);
            return base.UploadFilesFTP(id, uploadUri, dirPath);
        }

        #endregion

        #region FTP DownloadFiles

        /// <summary>
        /// DownloadFilesFTP
        /// </summary>
        /// <param name="id"></param>
        /// <param name="downloadUri"></param>
        /// <param name="dirPath"></param>
        public override bool DownloadFilesFTP(string id, string downloadUri, string dirPath)
        {
            base.ConnectFTP(returnConnectResult, returnResult);
            return base.DownloadFilesFTP(id, downloadUri, dirPath);
        }

        #endregion

        #region FTP RemoveDirectory

        /// <summary>
        /// RemoveDirFTP
        /// </summary>
        /// <param name="id"></param>
        /// <param name="Uri"></param>
        public override bool RemoveDirFTP(string id, string Uri)
        {
            base.ConnectFTP(returnConnectResult, returnResult);
            return base.RemoveDirFTP(id, Uri);
        }

        #endregion

        #region FTP CreateDirectory

        /// <summary>
        /// CreateDirFTP
        /// </summary>
        /// <param name="id"></param>
        /// <param name="Uri"></param>
        public override bool CreateDirFTP(string id, string Uri)
        {
            base.ConnectFTP(returnConnectResult, returnResult);
            return base.CreateDirFTP(id, Uri);
        }

        #endregion

    }
}
