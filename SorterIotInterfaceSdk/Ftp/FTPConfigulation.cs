using System;
using System.Collections.Generic;
using System.Text;

namespace Glory.SorterInterface.Ftp
{
    public class FTPConfigulation
    {
        #region Constructor
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="ftpServer">FTP Server(Name/IP)</param>
        /// <param name="ftpPort">FTP Port</param>
        /// <param name="ftpTimeOut">FTP TimeOut(1:1sec)</param>
        /// <param name="ftpInterval">FTP ftpInterval(1:1sec)</param>
        /// <param name="ftpConnectRunCount">Connect RunCount</param>
        /// <param name="ftpCommandRunCount">Command RunCount</param>
        /// <param name="ftpUser">FTP UserName</param>
        /// <param name="ftpPassword">FTP Password</param>
        /// <param name="isUseSSL">true:USE SSL</param>
        /// <param name="sslMode">true:USE SSL</param>
        /// <param name="isUseSslValidate">true:validate certificate</param>
        public FTPConfigulation(string ftpServer, int ftpPort, int ftpTimeOut, int ftpInterval, int ftpConnectRunCount, int ftpCommandRunCount, string ftpUser, string ftpPassword, bool isUseSSL, SSL_MODE sslMode, bool isUseSslValidate)
        {
            FtpServer = (ftpServer == null) ? string.Empty : ftpServer.Replace("ftps:", "ftp:");

            // FtpPort : 0 to 65535 (default=21)
            FtpPort = (ftpPort < 0 || 65535 < ftpPort) ? 21 : ftpPort;

            // timeout : 1 to 419 (default=10)
            FtpTimeout = (ftpTimeOut < 1 || 419 < ftpTimeOut) ? 10 : ftpTimeOut;

            // interval : 5 to 120 (default=60)
            FtpInterval = (ftpInterval < 5 || 120 < ftpInterval) ? 60 : ftpInterval;

            // RunCount : 1 to 100 (default=10) 0:non use
            FtpConnectRunCount = (ftpConnectRunCount < 0 || 100 < ftpConnectRunCount) ? 10 : ftpConnectRunCount;

            // RunCount : 1 to 100 (default=10) 0:non use
            FtpCommandRunCount = (ftpCommandRunCount < 0 || 100 < ftpCommandRunCount) ? 10 : ftpCommandRunCount;

            FtpUser = (ftpUser == null) ? string.Empty : ftpUser;
            FtpPassword = (ftpPassword == null) ? string.Empty : ftpPassword;
            IsUseSSL = isUseSSL;
            SSLmode = sslMode;
            ValidateCertificate = isUseSslValidate;
        }
        #endregion

        #region parameter
        /// <summary>
        /// SSL MODE
        /// </summary>
        public enum SSL_MODE
        {
            /// <summary>
            /// DEFAULT
            /// </summary>
            EXPLICIT,

            /// <summary>
            /// IMPLICIT
            /// </summary>
            IMPLICIT
        }

        /// <summary>
        /// ftp server
        /// </summary>
        private string ftp_server = string.Empty;

        /// <summary>
        /// FTP port
        /// </summary>
        private int ftp_port = 21;

        /// <summary>
        /// FTP user
        /// </summary>
        private string ftp_user = string.Empty;

        /// <summary>
        /// FTP password
        /// </summary>
        private string ftp_password = string.Empty;

        /// <summary>
        /// use ssl
        /// </summary>
        private bool is_use_ssl = false;

        /// <summary>
        /// SSL Mode
        /// </summary>
        private SSL_MODE ssl_mode = SSL_MODE.EXPLICIT;

        /// <summary>
        /// SSL validate certificate
        /// </summary>
        private bool validate_certificate = false;

        /// <summary>
        /// Ftp Timeout (1:1sec) min:1, max:419
        /// </summary>
        private int ftp_timeout = 10;

        /// <summary>
        /// FTP Interval (1:1sec) min:5, max:120
        /// </summary>
        private int ftp_interval = 60;

        /// <summary>
        /// FTP RunCount min:1, max:100 (0:non use)
        /// </summary>
        private int ftp_ConnectRunCount = 10;

        /// <summary>
        /// FTP RunCommandCount min:1, max:100 (0:non use)
        /// </summary>
        private int ftp_CommandRunCount = 10;

        /// <summary>
        /// Delagate that defines event handler for process error
        /// </summary>
        /// <param name="FTPPattern">ftpPattern parameter (allow to null)</param>
        /// <param name="RESULT_PROCESS">result</param>
        /// <param name="message">message</param> 
        public delegate void ResultProcess(FTPPattern ftpPattern, FtpAccess.RESULT_PROCESS result, string message);

        /// <summary>
        /// Delagate that defines event handler for process error
        /// </summary>
        /// <param name="FTPPattern">ftpPattern parameter (allow to null)</param>
        /// <param name="RESULT_PROCESS">result</param>
        /// <param name="message">message</param> 
        public delegate void ConnectProcess(FtpAccess.RESULT_PROCESS result);

        #endregion

        #region property
        /// <summary>
        /// Ftp Server
        /// </summary>
        public string FtpServer
        {
            get
            {
                return (ftp_server);
            }
            set
            {
                ftp_server = value;
            }
        }

        /// <summary>
        /// Ftp Port
        /// </summary>
        public int FtpPort
        {
            get
            {
                return (ftp_port);
            }
            set
            {
                ftp_port = value;
            }
        }

        /// <summary>
        /// Ftp User
        /// </summary>
        public string FtpUser
        {
            get
            {
                return (ftp_user);
            }
            set
            {
                ftp_user = value;
            }
        }

        /// <summary>
        /// Ftp Password
        /// </summary>
        public string FtpPassword
        {
            get
            {
                return (ftp_password);
            }
            set
            {
                ftp_password = value;
            }
        }

        /// <summary>
        /// use SSL
        /// </summary>
        public bool IsUseSSL
        {
            get
            {
                return (is_use_ssl);
            }
            set
            {
                is_use_ssl = value;
            }
        }

        /// <summary>
        /// SSL MODE
        /// </summary>
        public SSL_MODE SSLmode
        {
            get
            {
                return (ssl_mode);
            }
            set
            {
                ssl_mode = value;
            }
        }

        /// <summary>
        /// use ValidateCertificate
        /// </summary>
        public bool ValidateCertificate
        {
            get
            {
                return (validate_certificate);
            }
            set
            {
                validate_certificate = value;
            }
        }

        /// <summary>
        /// Ftp Timout (1:1sec)
        /// </summary>
        public int FtpTimeout
        {
            get
            {
                return (ftp_timeout);
            }
            set
            {
                ftp_timeout = value;
            }
        }

        /// <summary>
        /// Ftp Interval
        /// </summary>
        public int FtpInterval
        {
            get
            {
                return (ftp_interval);
            }
            set
            {
                ftp_interval = value;
            }
        }

        /// <summary>
        /// Ftp ConnectRunCount
        /// </summary>
        public int FtpConnectRunCount
        {
            get
            {
                return (ftp_ConnectRunCount);
            }
            set
            {
                ftp_ConnectRunCount = value;
            }
        }

        /// <summary>
        /// Ftp CommandRunCount
        /// </summary>
        public int FtpCommandRunCount
        {
            get
            {
                return (ftp_CommandRunCount);
            }
            set
            {
                ftp_CommandRunCount = value;
            }
        }
        #endregion
    }
}
