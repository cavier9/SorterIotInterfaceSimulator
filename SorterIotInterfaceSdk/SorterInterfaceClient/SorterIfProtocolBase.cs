using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using uPLibrary.Networking.M2Mqtt.Messages;
using uPLibrary.Networking.M2Mqtt.Exceptions;
using Glory.SorterInterface.Exceptions;
using Glory.SorterInterface.Message;
using Glory.SorterInterface.Protocol.Mqtt;
using Glory.SorterInterface.SorterInterfaceClient.Log;
using Glory.SorterInterface.SorterInterfaceClient.ReConnect;
using Glory.SorterInterface.Ftp;
using System.IO;

namespace Glory.SorterInterface.SorterInterfaceClient
{
    /// <summary>
    /// Sorter IoT Interface protocol base
    /// </summary>
    internal abstract class SorterIfProtocolBase
    {
        #region fixed parameter

        /// <summary>
        /// fixed QoS level
        /// </summary>
        private const QosLevel FIXED_QOS_LEVEL = QosLevel.AT_MOST_ONCE;

        #endregion

        #region property

        /// <summary>
        /// message log
        /// </summary>
        public IMessageLog MessageLog
        {
            get { return this.messageLog; }
        }

        #endregion

        #region parameter

        /// <summary>
        /// lock object
        /// </summary>
        private object lockObj = new Object();

        /// <summary>
        /// node type
        /// </summary>
        protected readonly NodeType nodeType;

        /// <summary>
        /// MQTT protocol
        /// </summary>
        private IMqttProtocol mqttProtocol = null;

        /// <summary>
        /// reconnect thread
        /// </summary>
        protected IReconnectThread reconnectThread = null;

        /// <summary>
        /// message log
        /// </summary>
        protected MessageLog messageLog = new MessageLog();

        /// <summary>
        /// FtpAccess
        /// </summary>
        private FtpAccess ftpAccess = null;

        /// <summary>
        /// FTPConfigulation
        /// </summary>
        private FTPConfigulation ftpConf = null;

        #endregion
        
        #region constructor

        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="nodeType">node type</param>
        /// <param name="mqttProtocol">MQTT protocol</param>
        internal SorterIfProtocolBase(NodeType nodeType, IMqttProtocol mqttProtocol)
        {
            this.nodeType = nodeType;
            this.mqttProtocol = mqttProtocol;
            this.mqttProtocol.SetReceiveProcess(this.ReceiveProcess);
            this.reconnectThread = new ReconnectThread(mqttProtocol, this);
        }
        
        /// <summary>
        /// constructor (plain text)
        /// </summary>
        /// <param name="nodeType">node type</param>
        /// <param name="hostName">MQTT broker host name</param>
        protected SorterIfProtocolBase(NodeType nodeType, string hostName)
            : this(nodeType, new MqttProtocol(hostName, SorterIfProtocolBase.FIXED_QOS_LEVEL))
        {
        }
        
#if M2MQTT_V3
        /// <summary>
        /// constructor (TLS. Broker certificate)
        /// </summary>
        /// <param name="nodeType">node type</param>
        /// <param name="hostName">host name</param>
        /// <param name="port">port Number</param>
        /// <param name="secure">Using secure connection</param>
        /// <param name="certificate">Using server certificate</param>
        /// <param name="sslProtocol">SSL/TLS protocol versions</param>
        /// <param name="qosLevel">QoS level</param>
        protected SorterIfProtocolBase(NodeType nodeType, string hostName, int port, bool secure, bool certificate, SslProtocols sslProtocol)
            : this(nodeType, new MqttProtocol(hostName, port, secure, certificate, sslProtocol, SorterIfProtocolBase.FIXED_QOS_LEVEL))
        {
        }
#else
        /// <summary>
        /// constructor (Secure. Broker certificate)
        /// </summary>
        /// <param name="nodeType">node type</param>
        /// <param name="hostName">host name</param>
        /// <param name="port">port Number</param>
        /// <param name="secure">Using secure connection</param>
        /// <param name="certificate">Using Broker certificate</param>
        internal SorterIfProtocolBase(NodeType nodeType, string hostName, int port, bool secure, bool certificate)
            : this(nodeType, new MqttProtocol(hostName, port, secure, certificate, SorterIfProtocolBase.FIXED_QOS_LEVEL))
        {
        }
#endif

        #endregion

        #region IDisposable interface implement

        /// <summary>
        /// Dispose        
        /// </summary>
        public virtual void Dispose()
        {
            // reconnect thread
            if (this.reconnectThread != null)
            {
                this.reconnectThread.RequestStop();
                this.reconnectThread.Dispose();
                this.reconnectThread = null;
            }

            // MqttProtocol
            if (this.mqttProtocol != null)
            {
                this.mqttProtocol.Disconnect();
                this.mqttProtocol.Dispose();
                this.mqttProtocol = null;
            }
            //ftp
            if (this.ftpAccess != null)
            {
                this.ftpAccess.Dispose();
                this.ftpAccess = null;
            }
            this.ftpConf = null;
        }

        #endregion

        #region Connect Broker

        /// <summary>
        /// Connect to Broker
        /// </summary>
        /// <param name="clientId">Client identifier</param>
        /// <param name="username">Username</param>
        /// <param name="password">Password</param>
        /// <param name="willFlag">Will flag</param>
        /// <param name="willTopic">Will topic</param>
        /// <param name="willMessage">Will message</param>
        /// <param name="subscribeTopicList">subscribe topic when success</param>
        protected void ConnectBroker(
            string clientId,
            string username,
            string password,
            bool willFlag,
            string willTopic,
            string willMessage,
            IList<string> subscribeTopicList)
        {
            lock (lockObj)
            {
                if (this.mqttProtocol == null)
                {
                    return;
                }

                if (this.mqttProtocol.IsConnected() == true)
                {
                    return;
                }

                try
                {
                    // Connect to broker
                    bool ret = this.mqttProtocol.Connect(clientId, username, password, willFlag, willTopic, willMessage);
                    if (ret == true)
                    {
                        // subscribe
                        foreach (string topic in subscribeTopicList)
                        {
                            this.SendSubscribe(topic);
                        }
                    }
                }
                finally
                {
                    // start ReConnect thread
                    this.reconnectThread.RequestStart(subscribeTopicList);
                }

                return;
            }
        }

        #endregion

        #region Disconnect Broker

        /// <summary>
        /// disconnect from Broker
        /// </summary>
        public void DisconnectBroker()
        {
            lock (lockObj)
            {
                if (this.mqttProtocol == null)
                {
                    return;
                }

                if (this.mqttProtocol.IsConnected() == false)
                {
                    return;
                }

                try
                {
                    // stop ReConnect thread
                    this.reconnectThread.RequestStop();

                    // disconnect from broker
                    this.mqttProtocol.Disconnect();
                }
                catch
                {
                    return;
                }
            }
        }

        #endregion
        
        #region IsConnected Broker

        /// <summary>
        /// IsConnected Broker
        /// </summary>
        /// <returns>true: conencted / false disconencted</returns>
        public bool IsConnectedBroker()
        {
            bool ret = (this.mqttProtocol != null) && (this.mqttProtocol.IsConnected());
            return ret;
        }

        #endregion

        #region receive process

        /// <summary>
        /// receive process
        /// </summary>
        /// <param name="topic">topic</param>
        /// <param name="jsonMessage">JSON message</param>
        public abstract void ReceiveProcess(string topic, string jsonMessage);

        #endregion

        #region send PUBLISH

        /// <summary>
        /// send publish
        /// </summary>
        /// <param name="topic">topic</param>
        /// <param name="jsonMessage">message</param>
        /// <param name="retain">Retain flag</param>
        /// <returns>true: send message success / false: send message fail</returns>
        public virtual bool SendPublish(string topic, string jsonMessage, bool retain)
        {
            if (this.mqttProtocol == null)
            {
                return false;
            }
            if (this.mqttProtocol.IsConnected() == false)
            {
                return false;
            }

            try
            {
                // log
                ReceiveMessage message = new ReceiveMessage(jsonMessage);
                this.messageLog.SetMessageLog(Log.MessageLog.LogType.Publish, topic, message.Type, message.Name, jsonMessage);
                // publish
                bool ret = this.mqttProtocol.SendPublish(topic, jsonMessage, retain);

                return ret;
            }
            catch
            {
                return false;
            }
        }

        #endregion

        #region send SUBSCRIBE

        /// <summary>
        /// send subscribe
        /// </summary>
        /// <param name="topic">topic</param>
        /// <returns>true: send message success / false: send message fail</returns>
        public bool SendSubscribe(string topic)
        {
            if (this.mqttProtocol == null)
            {
                return false;
            }
            if (this.mqttProtocol.IsConnected() == false)
            {
                return false;
            }

            try
            {
                // subscribe
                bool ret = this.mqttProtocol.SendSubscribe(topic);
                // log
                this.messageLog.SetMessageLog(Log.MessageLog.LogType.Subscribe, topic);

                return ret;
            }
            catch
            {
                return false;
            }
        }

        #endregion

        #region send UNSUBSCRIBE

        /// <summary>
        /// send unsubscribe
        /// </summary>
        /// <param name="topic">topic</param>
        /// <returns>true: send message success / false: send message fail</returns>
        public bool SendUnSubscribe(string topic)
        {
            if (this.mqttProtocol == null)
            {
                return false;
            }
            if (this.mqttProtocol.IsConnected() == false)
            {
                return false;
            }

            try
            {
                // unsubscribe
                bool ret = this.mqttProtocol.SendUnSubscribe(topic);
                // log
                this.messageLog.SetMessageLog(Log.MessageLog.LogType.UnSubscribe, topic);

                return ret;
            }
            catch
            {
                return false;
            }
        }

        #endregion

        #region Connect FTP

        /// <summary>
        /// Connect to FTP
        /// </summary>
        /// <param name="returnConnectResult">FtpAccess.ReturnConnectResult</param>
        /// <param name="returnResult">FtpAccess.ReturnResultProcess</param>
        protected void ConnectFTP(FtpAccess.ReturnConnectResult returnConnectResult,
                                  FtpAccess.ReturnResultProcess returnResult)
        {
            if (ftpAccess != null && ftpAccess.IsEndThread == false) return;

            // ftp connect start
            ftpAccess = new FtpAccess(this.ftpConf, new FtpAccess.ReturnConnectResult(returnConnectResult),
                                                    new FtpAccess.ReturnResultProcess(returnResult));
        }

        #endregion

        #region Setting FTP config

        /// <summary>
        /// Setting to FTP
        /// </summary>
        /// <param name="ftpServer"></param>
        /// <param name="ftpPort"></param>
        /// <param name="ftpUser"></param>
        /// <param name="ftpPassword"></param>
        /// <param name="ftpIsUseSSL"></param>
        /// <param name="ftpValidateCertificate"></param>
        /// <param name="ftpModeEXPLICIT"></param>
        protected void SettingFTP(string ftpServer, int ftpPort, string ftpUser, string ftpPassword,
                                  bool ftpIsUseSSL, bool ftpValidateCertificate, string ftpSslMode)
        {
            FTPConfigulation.SSL_MODE mode = FTPConfigulation.SSL_MODE.EXPLICIT;
            if (ftpSslMode.ToLower() == FTPConfigulation.SSL_MODE.IMPLICIT.ToString().ToLower())
            {
                mode = FTPConfigulation.SSL_MODE.IMPLICIT;
            }

            this.ftpConf = new FTPConfigulation(ftpServer, ftpPort,
                                                20, 10, 10, 10,
                                                @ftpUser, @ftpPassword,
                                                ftpIsUseSSL, mode,
                                                ftpValidateCertificate);
        }

        #endregion

        #region Dispose FTP

        /// <summary>
        /// Dispose to FTP
        /// </summary>
        protected void DisposeFTP()
        {
            if (ftpAccess != null)
            {
            ftpAccess.Dispose();
                ftpAccess = null;
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
        public virtual bool UploadFilesFTP(string id, string uploadUri, string dirPath)
        {
            string msg = "";
            if (uploadUri.EndsWith("/") == false) uploadUri += "/";

            bool res = ftpAccess.FtpCommandPatternSet(new FTPPattern(FTPPattern.COMMAND_PATTERN.UPLOAD_FILES,
                                                      uploadUri, dirPath, false, id), out msg);
            if (res == false)
                this.messageLog.SetMessageLog(Log.MessageLog.LogType.FtpResponse, "UploadFilesFTP Error:" + msg);

            return res;
        }

        #endregion

        #region FTP DownloadFiles

        /// <summary>
        /// DownloadFilesFTP
        /// </summary>
        /// <param name="id"></param>
        /// <param name="downloadUri"></param>
        /// <param name="dirPath"></param>
        public virtual bool DownloadFilesFTP(string id, string downloadUri, string dirPath)
        {
            string msg = "";

            if (downloadUri.EndsWith("/") == false) downloadUri += "/";
            bool res = ftpAccess.FtpCommandPatternSet(new FTPPattern(FTPPattern.COMMAND_PATTERN.DOWNLOAD_FILES,
                                                      downloadUri, dirPath, false, id), out msg);
            if (res == false)
                this.messageLog.SetMessageLog(Log.MessageLog.LogType.FtpResponse, "DownloadFilesFTP Error:" + msg);

            return res;
        }

        #endregion

        #region FTP RemoveDirectory

        /// <summary>
        /// RemoveDirFTP
        /// </summary>
        /// <param name="id"></param>
        /// <param name="Uri"></param>
        public virtual bool RemoveDirFTP(string id, string Uri)
        {
            string msg = "";

            if (Uri.EndsWith("/") == false) Uri += "/";
            bool res = ftpAccess.FtpCommandPatternSet(new FTPPattern(FTPPattern.COMMAND_PATTERN.REMOVE_DIR,
                                                      Uri, "", false, id), out msg);
            return res;
        }

        #endregion

        #region FTP CreateDirectory

        /// <summary>
        /// CreateDirFTP
        /// </summary>
        /// <param name="id"></param>
        /// <param name="Uri"></param>
        public virtual bool CreateDirFTP(string id, string Uri)
        {
            string msg = "";

            if (Uri.EndsWith("/") == false) Uri += "/";
            bool res = ftpAccess.FtpCommandPatternSet(new FTPPattern(FTPPattern.COMMAND_PATTERN.CREATE_DIR,
                                                      Uri, "", false, id), out msg);
            if (res == false)
                this.messageLog.SetMessageLog(Log.MessageLog.LogType.FtpResponse, "CreateDirFTP Error:" + msg);
            return res;
        }

        #endregion

        #region FTP GetConnetStatus

        /// <summary>
        /// ConnetStatusFTP
        /// </summary>
        /// <param name="id"></param>
        protected virtual bool ConnetStatusFTP(string id, string Uri)
        {
            string msg = "";

            if (Uri.EndsWith("/") == false) Uri += "/";
            bool res = ftpAccess.FtpCommandPatternSet(new FTPPattern(FTPPattern.COMMAND_PATTERN.CONNECT_STATUS,
                                                      Uri, "", false, id), out msg);
            if (res == false)
                this.messageLog.SetMessageLog(Log.MessageLog.LogType.FtpResponse, "ConnetStatusFTP Error:" + msg);
            return res;
        }

        #endregion
    }
}
