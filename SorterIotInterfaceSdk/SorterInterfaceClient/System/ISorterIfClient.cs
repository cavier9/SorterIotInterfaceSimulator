using System;

namespace Glory.SorterInterface.SorterInterfaceClient.Client
{
    /// <summary>
    /// Soreter IoT interface Client interface
    /// </summary>
    public interface ISorterIfClient : ISorterIfClientBase
    {
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
        
        #region send PUBLISH

        /// <summary>
        /// send publish
        /// </summary>
        /// <param name="topic">topic</param>
        /// <param name="jsonMessage">message</param>
        /// <param name="retain">Retain flag</param>
        void SendPublish(string topic, string jsonMessage, bool retain);

        #endregion

        #region send SUBSCRIBE

        /// <summary>
        /// send subscribe
        /// </summary>
        /// <param name="topic">topic</param>
        void SendSubscribe(string topic);

        #endregion

        #region send UNSUBSCRIBE

        /// <summary>
        /// send unsubscribe
        /// </summary>
        /// <param name="topic">topic</param>
        void SendUnSubscribe(string topic);

        #endregion

        #region FTP ConfigSetting

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
        void FtpConfigSetting(string hostName,
                              int portNo,
                              string userName,
                              string password,
                              bool useSsl,
                              bool cirtificate,
                              SorterIfClient.FTP_SSL_MODE sslMode);
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
