using System;

namespace Glory.SorterInterface.SorterInterfaceClient.Client
{
    /// <summary>
    /// Sorter Interface protocol interface
    /// </summary>
    internal interface ISorterIfProtocol : ISorterIfProtocolBase
    {
        #region property

        /// <summary>
        /// system name
        /// </summary>
        string SystemName { get; }

        /// <summary>
        /// system version
        /// </summary>
        string SystemVersion { get; }

        /// <summary>
        /// device name
        /// </summary>
        string ConnectDeviceName { get; }

        /// <summary>
        /// sessionID
        /// </summary>
        int SessionID { get; set; }

        /// <summary>
        /// SorterIfClient
        /// </summary>
        ISorterIfClient Client { get; }

        #endregion

        #region get device info

        /// <summary>
        /// get device name
        /// </summary>
        /// <returns>device name</returns>
        string GetConnectDeviceName();

        /// <summary>
        /// is connected device
        /// </summary>
        /// <returns>true: connected / false: disconnected</returns>
        bool IsConnectedDevice();

        /// <summary>
        /// set connected device state
        /// </summary>
        /// <param name="isConnected">is connected</param>
        void SetConnectedDevice(bool isConnected);

        #endregion

        #region Connect Broker

        /// <summary>
        /// Connect to Broker
        /// </summary>
        /// <param name="username">user name, connect to broker</param>
        /// <param name="password">pass word, connect to broker</param>
        /// <param name="clientId">client id, connect to broker</param>
        void ConnectBroker(
            string username,
            string password,
            string clientId);

        #endregion

        #region receive process

        /// <summary>
        /// receive process
        /// </summary>
        /// <param name="topic">topic</param>
        /// <param name="jsonMessage">JSON message</param>
        void ReceiveProcess(string topic, string jsonMessage);

        #endregion
        
        #region FTP configSetting

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
        void settingFTP(string hostName,
                        int portNo,
                        string userName,
                        string password,
                        bool useSsl,
                        bool cirtificate,
                        string sslMode);

        #endregion

        #region FTP UploadFiles

        /// <summary>
        /// UploadFilesFTP
        /// </summary>
        /// <param name="id"></param>
        /// <param name="uploadUri"></param>
        /// <param name="dirPath"></param>
        bool UploadFilesFTP(string id, string uploadUri, string dirPath);

        #endregion

        #region FTP DownloadFiles

        /// <summary>
        /// DownloadFilesFTP
        /// </summary>
        /// <param name="id"></param>
        /// <param name="downloadUri"></param>
        /// <param name="dirPath"></param>
        bool DownloadFilesFTP(string id, string downloadUri, string dirPath);

        #endregion

        #region FTP RemoveDirectory

        /// <summary>
        /// RemoveDirFTP
        /// </summary>
        /// <param name="id"></param>
        /// <param name="Uri"></param>
        bool RemoveDirFTP(string id, string Uri);

        #endregion

        #region FTP CreateDirectory

        /// <summary>
        /// CreateDirFTP
        /// </summary>
        /// <param name="id"></param>
        /// <param name="Uri"></param>
        bool CreateDirFTP(string id, string Uri);

        #endregion

    }
}
