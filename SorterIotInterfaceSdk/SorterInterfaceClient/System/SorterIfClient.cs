using System;
using Glory.SorterInterface.SorterInterfaceClient;
using Glory.SorterInterface.SorterInterfaceClient.Log;

namespace Glory.SorterInterface.SorterInterfaceClient.Client
{
    /// <summary>
    /// Sorter IoT Interface client for System
    /// </summary>
    public class SorterIfClient : ISorterIfClient
    {
        #region fixed parameter

        /// <summary>
        /// Topic 最小長
        /// </summary>
        private const int TOPIC_LENGTH_MIN = 1;

        /// <summary>
        /// Topic 最大長
        /// </summary>
        private const int TOPIC_LENGTH_MAX = 32767;

        /// <summary>
        /// 機器名称 文字列長
        /// </summary>
        private const int DEVICE_NAME_LENGTH = 9;

        #endregion

        #region property

        /// <summary>
        /// Connection status between client and broker
        /// </summary>
        public bool IsConnectedBroker
        {
            get { return this.protocol.IsConnectedBroker(); }
        }

        /// <summary>
        /// message log
        /// </summary>
        public IMessageLog MessageLog
        {
            get { return this.protocol.MessageLog; }
        }

        #endregion

        #region parameter

        /// <summary>
        /// Sorter Interface Protocol
        /// </summary>
        private ISorterIfProtocol protocol;

        #endregion

        #region constructor

        /// <summary>
        /// constructor for Unit Test
        /// </summary>
        /// <param name="protocol">protocol</param>
        internal SorterIfClient(ISorterIfProtocol protocol)
        {
            this.protocol = protocol;
        }

        /// <summary>
        /// constructor (plain text)
        /// </summary>
        /// <param name="deviceName">deviceName</param>
        /// <param name="systemName">system name</param>
        /// <param name="systemVersion">system version</param>
        /// <param name="hostName">host name</param>
        public SorterIfClient(string deviceName, string systemName, string systemVersion, string hostName)
        {
            if ((deviceName == null) || (deviceName.Length != SorterIfClient.DEVICE_NAME_LENGTH) || 
                (systemName == null) || (systemName.Length < 1) || (64 < systemName.Length) ||
                (systemVersion == null) || (systemVersion.Length < 1) || (32 < systemVersion.Length) ||
                (hostName == null) || (hostName.Length < 1) || (64 < hostName.Length))
            {
                throw new ArgumentException();
            }
            this.protocol = new SorterIfProtocol(this, deviceName, systemName, systemVersion, hostName);
        }

#if M2MQTT_V3
        // @@@ chg USF-50S_TLSv1.2対応 2018.08.02 by Hemmi ↓
        /// <summary>
        /// constructor (TLS. Broker certificate)
        /// </summary>
        /// <param name="deviceName">deviceName</param>
        /// <param name="systemName">system name</param>
        /// <param name="systemVersion">system version</param>
        /// <param name="hostName">host name</param>
        /// <param name="port">port Number</param>
        /// <param name="secure">Using secure connection</param>
        /// <param name="certificate">Using server certificate</param>
        /// <param name="tlsVersion">Using TLS Version</param>
        public SorterIfClient(string deviceName, string systemName, string systemVersion, string hostName, int port, bool secure, bool certificate, SslProtocols tlsVersion)
        {
            if ((deviceName == null) || (deviceName.Length != SorterIfClient.DEVICE_NAME_LENGTH) || 
                (systemName == null) || (systemName.Length < 1) || (64 < systemName.Length) ||
                (systemVersion == null) || (systemVersion.Length < 1) || (32 < systemVersion.Length) ||
                (hostName == null) || (hostName.Length < 1) || (64 < hostName.Length) ||
                (port < 0) || (65535 < port))
            {
                throw new ArgumentException();
            }
            this.protocol = new SorterIfProtocol(this, deviceName, systemName, systemVersion, hostName, port, secure, certificate, tlsVersion);
        }
        // @@@ chg USF-50S_TLSv1.2対応 2018.08.02 by Hemmi ↑
#else
#endif
        #endregion

        #region IDisposable interface implement

        /// <summary>
        /// Dispose
        /// </summary>
        public void Dispose()
        {
            this.protocol.Dispose();
            this.protocol = null;
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
            if ((this.protocol == null) || (this.protocol.IsConnectedBroker() == true))
            {
                throw new InvalidOperationException();
            }
            if ((username == null) || (64 < username.Length) ||
                (password == null) || (64 < password.Length) ||
                (clientId == null) || (64 < clientId.Length))
            {
                throw new ArgumentException();
            }
            this.protocol.ConnectBroker(username, password, clientId);
        }

        #endregion

        #region Disconnect Broker

        /// <summary>
        /// disconnect from Broker
        /// </summary>
        public void DisconnectBroker()
        {
            if (this.protocol != null)
            {
                this.protocol.DisconnectBroker();
            }
        }

        #endregion
        
        #region send PUBLISH

        /// <summary>
        /// send publish
        /// </summary>
        /// <param name="topic">topic</param>
        /// <param name="jsonMessage">message</param>
        /// <param name="retain">Retain flag</param>
        public void SendPublish(string topic, string jsonMessage, bool retain)
        {
            this.CheckIsConnectedBroker();
            if ((this.CheckTopic(topic) == false) ||
                (jsonMessage == null) || (jsonMessage.Length < 1))
            {
                throw new ArgumentException();
            }
            this.protocol.SendPublish(topic, jsonMessage, retain);
        }

        #endregion

        #region send SUBSCRIBE

        /// <summary>
        /// send subscribe
        /// </summary>
        /// <param name="topic">topic</param>
        public void SendSubscribe(string topic)
        {
            this.CheckIsConnectedBroker();
            if (this.CheckTopic(topic) == false)
            {
                throw new ArgumentException();
            }
            this.protocol.SendSubscribe(topic);
        }

        #endregion

        #region send UNSUBSCRIBE

        /// <summary>
        /// send unsubscribe
        /// </summary>
        /// <param name="topic">topic</param>
        public void SendUnSubscribe(string topic)
        {
            this.CheckIsConnectedBroker();
            if (this.CheckTopic(topic) == false)
            {
                throw new ArgumentException();
            }
            this.protocol.SendUnSubscribe(topic);
        }

        #endregion

        #region check state, check parameter

        /// <summary>
        /// check is connected broker
        /// </summary>
        private void CheckIsConnectedBroker()
        {
            if ((this.protocol == null) || (this.protocol.IsConnectedBroker() == false))
            {
                throw new InvalidOperationException("disconnect MQTT broker");
            }
        }

        /// <summary>
        /// check is connected device
        /// </summary>
        private void CheckIsConnectedDevice()
        {
            if ((this.protocol == null) || (this.protocol.IsConnectedDevice() == false))
            {
                // nop
                // SDK send CreateSession command, if unconnected.
            }
        }

        /// <summary>
        /// check parameter to send command
        /// </summary>
        /// <param name="receive">receive</param>
        private void CheckParamSendCommand(object receive)
        {
            if (receive == null)
            {
                throw new ArgumentException("receive function is null");
            }
        }
        /// <summary>
        /// check parameter to send command
        /// </summary>
        /// <param name="receive">receive</param>
        /// <param name="detail">detail</param>
        private void CheckParamSendCommand(object receive, object detail)
        {
            if (receive == null)
            {
                throw new ArgumentException("receive function is null");
            }
            if (detail == null)
            {
                throw new ArgumentException("detail object is null");
            }
        }

        /// <summary>
        /// check Topic
        /// </summary>
        /// <param name="topic">Topic</param>
        /// <returns>true:OK / false:NG</returns>
        private bool CheckTopic(string topic)
        {
            return ((topic != null) &&
                (SorterIfClient.TOPIC_LENGTH_MIN <= topic.Length) &&
                (topic.Length <= SorterIfClient.TOPIC_LENGTH_MAX));
        }

        #endregion

        #region FTP ConfigSetting

        /// <summary>
        /// FTP access SSL mode(Explicit/Implicit)
        /// </summary>
        public enum FTP_SSL_MODE
        {
            /// <summary>
            /// Explicit
            /// </summary>
            EXPLICIT,

            /// <summary>
            /// Implicit
            /// </summary>
            IMPLICIT
        }

        /// <summary>
        /// FTP ConfigSetting
        /// </summary>
        /// <param name="hostName"></param>
        /// <param name="portNo"></param>
        /// <param name="userName"></param>
        /// <param name="password"></param>
        /// <param name="useSsl"></param>
        /// <param name="cirtificate"></param>
        /// <param name="sslMode">EXPLICIT / IMPLICIT</param>      
        public void FtpConfigSetting(string hostName,
                                    int portNo,
                                    string userName,
                                    string password,
                                    bool useSsl,
                                    bool cirtificate,
                                    FTP_SSL_MODE sslMode)
        {
            // check argument
            if ((hostName == null) ||
                (portNo < 0) || (65535 < portNo) ||
                (userName == null) ||
                (password == null))
            {
                throw new ArgumentException();
            }

            // FTP configSetting
            this.protocol.settingFTP(hostName,
                                    portNo,
                                    userName,
                                    password,
                                    useSsl,
                                    cirtificate,
                                    sslMode.ToString());
        }

        #endregion

        #region FTP UploadFiles

        /// <summary>
        /// UploadFilesFTP
        /// </summary>
        /// <param name="id"></param>
        /// <param name="uploadUri"></param>
        /// <param name="dirPath"></param>
        public bool UploadFilesFTP(string id, string uploadUri, string dirPath)
        {
            return this.protocol.UploadFilesFTP(id, uploadUri, dirPath);
        }

        #endregion

        #region FTP DownloadFiles

        /// <summary>
        /// DownloadFilesFTP
        /// </summary>
        /// <param name="id"></param>
        /// <param name="downloadUri"></param>
        /// <param name="dirPath"></param>
        public bool DownloadFilesFTP(string id, string downloadUri, string dirPath)
        {
            return this.protocol.DownloadFilesFTP(id, downloadUri, dirPath);
        }

        #endregion

        #region FTP RemoveDirectory

        /// <summary>
        /// RemoveDirFTP
        /// </summary>
        /// <param name="id"></param>
        /// <param name="Uri"></param>
        public bool RemoveDirFTP(string id, string Uri)
        {
            return this.protocol.RemoveDirFTP(id, Uri);
        }

        #endregion

        #region FTP CreateDirectory

        /// <summary>
        /// CreateDirFTP
        /// </summary>
        /// <param name="id"></param>
        /// <param name="Uri"></param>
        public bool CreateDirFTP(string id, string Uri)
        {
            return this.protocol.CreateDirFTP(id, Uri);
        }

        #endregion



    }
}
