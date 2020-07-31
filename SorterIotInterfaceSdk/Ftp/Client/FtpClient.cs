using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Collections;

namespace Glory.SorterInterface.Ftp
{
    /// <summary>
    /// Ftp Client
    /// </summary>
    public class FtpClient : IFtpClient
    {
        #region Constructor
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="serverName">server name</param>
        /// <param name="portNumber">port number</param>
        /// <param name="useSSL">true : use SSL</param>
        /// <param name="sslMode">sslMode (Explicit / Implicit)</param>
        /// <param name="validate">true: use validate</param>
        /// <param name="userName">user name</param>
        /// <param name="password">password</param>
        /// <param name="timeout">timeout (1=1sec)</param>
        /// <param name="currentDirectory">currentDirectory</param>
        public FtpClient(string serverName, int portNumber, bool useSSL, FTPConfigulation.SSL_MODE sslMode, bool validate,
                         string userName, string password, int timeout, string currentDirectory)
        {
            this.serverName = serverName;
            this.portNumber = portNumber;
            this.useSSL = useSSL;
            this.sslMode = sslMode;
            if (this.useSSL == false)
                this.sslMode = FTPConfigulation.SSL_MODE.EXPLICIT;
            this.useValidate = validate;
            this.userName = userName;
            this.password = password;
            this.timeout = timeout;
            this.currentDirectory = currentDirectory;
            this.messageString = "";
            this.isLogin = false;

        }

        #endregion

        #region property
        /// <summary>
        /// controlSocket
        /// </summary>
        private Socket controlSocket = null;
        private Socket dataSocket = null;

        /// <summary>
        /// sslControl
        /// </summary>
        private ISslHelper sslControl = null;
        private ISslHelper sslData = null;

        /// <summary>
        /// serverName
        /// </summary>
        private string serverName;

        /// <summary>
        /// portNumber
        /// </summary>
        private int portNumber;

        /// <summary>
        /// use SSL
        /// </summary>
        private bool useSSL;

        /// <summary>
        /// SSL mode (Implicit / Explicit)
        /// </summary>
        private FTPConfigulation.SSL_MODE sslMode;

        /// <summary>
        /// use Validate
        /// </summary>
        private bool useValidate;

        /// <summary>
        /// userName
        /// </summary>
        private string userName;

        /// <summary>
        /// password
        /// </summary>
        private string password;

        /// <summary>
        /// timeout(1:1sec)
        /// </summary>
        private int timeout;

        /// <summary>
        /// currentDirectory
        /// </summary>
        private string currentDirectory;

        /// <summary>
        /// isLogin (true:login)
        /// </summary>
        private bool isLogin;

        /// <summary> 
        /// resv string encoding
        /// </summary>
        private Encoding ASCII = Encoding.ASCII;

        /// <summary>
        /// binary
        /// </summary>
        private byte[] buffer = new byte[4096];

        /// <summary>
        /// response string
        /// </summary>
        private string responseString;

        /// <summary>
        /// response Code
        /// </summary>
        private int responseCode;

        /// <summary>
        /// message
        /// </summary>
        private string messageString;
        #endregion

        #region parameter
        /// <summary>
        /// isLogin
        /// </summary>
        public bool IsLogin
        {
            get
            {
                return this.isLogin;
            }
        }

        /// <summary>
        /// message
        /// </summary>
        public string MessageString
        {
            get
            {
                return this.messageString;
            }
            set
            {
                this.messageString = value;
            }
        }
        #endregion

        #region "Public Subs and Functions"

        /// <summary>
        /// ServerLogin
        /// </summary>
        /// <returns>result（true:success、false:error）</returns>
        public bool Login()
        {
            // Control Socket Create.
            this.controlSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.IP);

            this.sslControl = new SslHelperFW(this.controlSocket, this.serverName);

            IPAddress address = null;
            // IPv4 FTP Connect.
            try
            {
                address = IPAddress.Parse(this.serverName);
                if (address.AddressFamily != AddressFamily.InterNetwork) // Check IPv4.
                {
                    throw new FormatException();    // DNS Search.
                }
            }
            catch (FormatException)
            {
                try
                {
                    IPHostEntry host = Dns.GetHostEntry(this.serverName);

                    foreach (IPAddress a in host.AddressList)
                    {
                        if (a.AddressFamily == AddressFamily.InterNetwork)
                        {
                            address = a;
                            break;
                        }
                    }
                    if (address == null)
                    {
                        this.messageString = "Cannot find remote server";
                        return false;
                    }
                }
                catch
                {
                    this.messageString = "Cannot find remote server";
                    return false;
                }
            }
            IPEndPoint ep = new IPEndPoint(address, this.portNumber);

            // Control Socket Connect.
            try
            {
                this.controlSocket.Connect(ep);
            }
            catch
            {
                this.messageString = "Cannot connect to remote server";
                return false;
            }

            if (this.sslMode == FTPConfigulation.SSL_MODE.IMPLICIT)
            {
                //-implicit- Only UseSSL

                //SSL Start
                try
                {
                    sslControl.StartSSL(this.useValidate); // SSL Start.
                }
                catch (Exception ex)
                {
                    this.Cleanup();
                    this.messageString = "Ssl policy error." + ex.Message;
                    return false;
                }
                this.ReadReply();

                // Protect Data.
                this.SendCommand("PBSZ 0");
                if (!(this.responseCode == 200))
                {
                    this.Cleanup();
                    this.messageString = "PBSZ responseCode: " + this.responseString;
                    return false;
                }
                this.SendCommand("PROT P");
                if (!(this.responseCode == 200))
                {
                    this.Cleanup();
                    this.messageString = "PROT responseCode: " + this.responseString;
                    return false;
                }

                // Send UserName
                this.SendCommand("USER " + this.userName);
                if (!(this.responseCode == 230 || this.responseCode == 331))
                {
                    this.Cleanup();
                    this.messageString = "USER " + this.userName + " responseCode: " + this.responseString;
                    return false;
                }

                // Server Request = Password
                if (this.responseCode == 331)
                {
                    // Send Password
                    this.SendCommand("PASS " + this.password);
                    if (!(this.responseCode == 230 || this.responseCode == 202))
                    {
                        this.Cleanup();
                        this.messageString = "PASS " + this.password + " responseCode: " + this.responseString;
                        return false;
                    }
                }

                this.isLogin = true; // Login OK
            }
            else
            {

                this.ReadReply();
                if (this.responseCode != 220)
                {
                    this.Cleanup();
                    this.messageString = this.responseString;
                    return false;
                }

                if (this.useSSL == true)
                {
                    this.SendCommand("AUTH SSL");
                    try
                    {
                        sslControl.StartSSL(this.useValidate); // SSL Start.
                    }
                    catch
                    {
                        this.Cleanup();
                        this.messageString = "Ssl policy error";
                        return false;
                    }
                }

                // send username
                this.SendCommand("USER " + this.userName);
                if (!(this.responseCode == 230 || this.responseCode == 331))
                {
                    this.Cleanup();
                    this.messageString = "USER " + this.userName + " responseCode: " + this.responseString;
                    return false;
                }

                if (this.responseCode == 331)
                {
                    // send password
                    this.SendCommand("PASS " + this.password);
                    if (!(this.responseCode == 230 || this.responseCode == 202))
                    {
                        this.Cleanup();
                        this.messageString = "PASS " + this.password + " responseCode: " + this.responseString;
                        return false;
                    }
                }

                this.isLogin = true;

                // sslDataport is protect 
                if (this.useSSL == true)
                {
                    this.SendCommand("PBSZ 0");
                    if (!(this.responseCode == 200))
                    {
                        this.Cleanup();
                        this.messageString = "PBSZ responseCode: " + this.responseString;
                        return false;
                    }
                    this.SendCommand("PROT P");
                    if (!(this.responseCode == 200))
                    {
                        this.Cleanup();
                        this.messageString = "PROT responseCode: " + this.responseString;
                        return false;
                    }
                }
            }

            // Change currentDirectory
            if (this.ChangeDirectory(this.currentDirectory))
            {
                this.messageString = "Connected FTP";
                return true;
            }
            else
            {
                this.Logout();
                return false;
            }
        }

        /// <summary>
        /// Logout
        /// </summary>
        public void Logout()
        {
            if (this.controlSocket != null)
            {
                try
                {
                    // Send an FTP command to end an FTP server system.
                    this.SendCommand("QUIT");
                }
                catch
                {
                    // no check
                }
            }

            this.Cleanup();
        }

        /// <summary>
        /// ExistsDirectory
        /// </summary>
        /// <param name="directoryName">directoryName</param>
        /// <returns>false：nothing</returns>
        public bool ExistsDirectory(string directoryName)
        {
            string tmp = this.currentDirectory;
            if (this.ChangeDirectory(directoryName))
            {
                this.ChangeDirectory(tmp);
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// ReturnCurrentDirectory
        /// </summary>
        /// <returns>true:success</returns>
        public bool ReturnCurrentDirectory()
        {
            if (!this.isLogin)
            {
                return false;
            }

            this.SendCommand("CDUP");
            if (this.responseCode != 250)
            {
                this.messageString = "CDUP responseCode: " + this.responseString;
                return false;
            }

            this.SendCommand("PWD");
            if (this.responseCode != 257)
            {
                this.messageString = "PWD responseCode: " + this.responseString;
                return false;
            }

            this.currentDirectory = this.responseString.Substring(5, this.responseString.IndexOf('"', 5) - 5);

            return true;
        }

        /// <summary>
        /// ChangecurrentDirectory
        /// </summary>
        /// <param name="directoryName">directoryName</param>
        /// <returns>true:success</returns>
        public bool ChangeDirectory(string directoryName)
        {
            if (!this.isLogin)
            {
                return false;
            }

            this.SendCommand("CWD " + directoryName);
            if (this.responseCode != 250)
            {
                this.messageString = "CWD responseCode: " + this.responseString;
                return false;
            }

            this.SendCommand("PWD");
            if (this.responseCode != 257)
            {
                this.messageString = "PWD responseCode: " + this.responseString;
                return false;
            }

            this.currentDirectory = this.responseString.Substring(5, this.responseString.IndexOf('"', 5) - 5);

            return true;
        }

        /// <summary>
        /// GetCurrentDirectory
        /// </summary>
        /// <returns>true:success</returns>
        public bool GetDirectory()
        {
            if (!this.isLogin)
            {
                return false;
            }

            this.SendCommand("PWD");
            if (this.responseCode != 257)
            {
                this.messageString = "PWD responseCode: " + responseString;
                return false;
            }

            this.currentDirectory = this.responseString.Substring(5, this.responseString.IndexOf('"', 5) - 5);

            return true;
        }

        /// <summary>
        /// GetFileDirList (NLST Command)
        /// </summary>
        /// <param name="mask">Masking</param>
        /// <param name="fileList">fileList</param>
        /// <returns>true:success</returns>
        public bool GetFileDirList(string mask, out string[] fileList)
        {
            fileList = null;

            if (!this.isLogin)
            {
                return false;
            }

            ConnectDataSocket();

            if (this.sslData != null)
            {
                this.SendCommand("NLST " + mask);
                if (!(this.responseCode == 150 || this.responseCode == 125 || this.responseCode == 226))
                {
                    this.messageString = "NLST responseCode: " + this.responseString;
                    return false;
                }

                if (this.useSSL == true)
                {
                    try
                    {
                        this.sslData.StartSSL(this.useValidate); // SSL Start.
                    }
                    catch
                    {
                        this.messageString = "Ssl policy error";
                        return false;
                    }
                }

                string temp = "";
                do
                {
                    Array.Clear(this.buffer, 0, this.buffer.Length);
                    int bytes = this.sslData.Recv(this.buffer, this.timeout);
                    if (bytes > 0)
                    {
                        temp += this.ASCII.GetString(this.buffer, 0, bytes);
                    }
                    else
                    {
                        break;
                    }
                } while (this.sslData.Available);

                if (this.responseCode != 226)
                {
                    DisconnectDataSocket();
                    this.ReadReply();
                    if (this.responseCode != 226)
                    {
                        this.messageString = this.responseString;
                        return false;
                    }
                }

                temp = temp.Replace("\r", "").TrimEnd('\n');
                if (string.IsNullOrEmpty(temp))
                {
                    fileList = new string[0];
                }
                else
                {
                    fileList = temp.Split('\n');
                }
            }
            return true;
        }

        /// <summary>
        /// GetFileList (LIST command)
        /// </summary>
        /// <param name="mask">Masking</param>
        /// <param name="fileList">fileList</param>
        /// <returns>true:success</returns>
        public bool GetFileList(string mask, out string[] fileList)
        {
            string command = "LIST ";
            fileList = null;

            if (!this.isLogin)
            {
                return false;
            }

            ConnectDataSocket();

            if (this.sslData != null)
            {
                this.SendCommand(command + mask);
                if (!(this.responseCode == 150 || this.responseCode == 125 || this.responseCode == 226))
                {
                    this.messageString = command + " responseCode: " + this.responseString;
                    return false;
                }

                if (this.useSSL == true)
                {
                    try
                    {
                        this.sslData.StartSSL(this.useValidate); // SSL Start.
                    }
                    catch
                    {
                        this.messageString = "Ssl policy error";
                        return false;
                    }
                }

                string temp = "";
                do
                {
                    Array.Clear(this.buffer, 0, this.buffer.Length);
                    int bytes = this.sslData.Recv(this.buffer, this.timeout);
                    if (bytes > 0)
                    {
                        temp += this.ASCII.GetString(this.buffer, 0, bytes);
                    }
                    else
                    {
                        break;
                    }
                } while (this.sslData.Available);

                if (this.responseCode != 226)
                {
                    DisconnectDataSocket();
                    this.ReadReply();
                    if (this.responseCode != 226)
                    {
                        this.messageString = command + this.responseString;
                        return false;
                    }
                }

                temp = temp.Replace("\r", "").TrimEnd('\n');
                if (string.IsNullOrEmpty(temp))
                {
                    fileList = new string[0];
                }
                else
                {
                    //fileList = temp.Split('\n');
                    ArrayList data = new ArrayList();
                    foreach (string fileline in temp.Split('\n'))
                    {
                        if (fileline.Contains("<DIR>") == false)
                        {
                            string[] files = fileline.Split(' ');
                            if (files.Length > 0) data.Add(files[files.Length - 1]);
                        }

                    }
                    fileList = (string[])data.ToArray(typeof(string));
                }

            }
            return true;
        }


        /// <summary>
        /// GetFileSize
        /// </summary>
        /// <param name="fileName">fileName</param>
        /// <param name="fileSize">fileSize</param>
        /// <returns>true:success</returns>
        public bool GetFileSize(string fileName, out long fileSize)
        {
            fileSize = 0;
            if (!this.isLogin)
            {
                return false;
            }

            this.SendCommand("SIZE " + fileName);
            if (this.responseCode == 213)
            {
                fileSize = Int64.Parse(this.responseString.Substring(4));
                return true;
            }
            else
            {
                this.messageString = "SIZE " + fileName + " responseCode: " + responseString;
                return false;
            }
        }


        /// <summary>
        /// DownloadFile
        /// </summary>
        /// <param name="serverFileName">serverFileName</param>
        /// <param name="localFileName">localFileName</param>
        /// <returns>true:success</returns>
        public bool DownloadFile(string serverFileName, string localFileName)
        {
            if (!this.isLogin)
            {
                return false;
            }

            if (!this.SetTransferMode(true))
            {
                return false;
            }

            if (string.IsNullOrEmpty(localFileName))
            {
                localFileName = serverFileName;
            }

            try { if (File.Exists(localFileName)) File.Delete(localFileName); }
            catch { }

            using (FileStream output = new FileStream(localFileName, FileMode.OpenOrCreate, FileAccess.ReadWrite))
            {
                ConnectDataSocket();

                if (this.sslData != null)
                {
                    bool isRecieve226 = false;

                    //long offset = 0;

                    // get filedata
                    this.SendCommand("RETR " + serverFileName);
                    if (!(this.responseCode == 150 || this.responseCode == 125))
                    {
                        this.messageString = "RETR responseCode: " + responseString;
                        if (this.responseCode == 226)
                        {
                            // 226. Closing data connection.
                            isRecieve226 = true;
                        }
                        else
                        {
                            return false;
                        }
                    }
                    
                    if (this.useSSL == true)
                    {
                        try
                        {
                            this.sslData.StartSSL(this.useValidate); // SSL Start.
                        }
                        catch
                        {
                            this.messageString = "Ssl policy error";
                            return false;
                        }
                    }

                    long downloadSize = 0;
                    long loopCount = 0;

                    do
                    {
                        Array.Clear(this.buffer, 0, this.buffer.Length);
                        int count = this.sslData.Recv(this.buffer, this.timeout);
                        if (count > 0)
                        {
                            // ダウンロードしたデータをファイルに書き込み
                            output.Write(this.buffer, 0, count);

                            downloadSize += count;
                        }
                        else
                        {
                            break;
                        }
                        loopCount++;

                    } while (this.sslData.Available);

                    if (isRecieve226 == false)
                    {
                        DisconnectDataSocket();
                        this.ReadReply();
                        if (!(this.responseCode == 226 || this.responseCode == 250))
                        {
                            this.messageString = responseString + " -- RECV: downlaodSize=" + downloadSize.ToString() + ", loopCount=" + loopCount.ToString();
                            return false;
                        }
                    }
                    else
                    {
                        DisconnectDataSocket();
                    }

                    this.messageString = responseString + " -- RECV: downlaodSize=" + downloadSize.ToString() + ", loopCount=" + loopCount.ToString();
                }
            }
            
            return true;
        }

        /// <summary>
        /// UploadFile
        /// </summary>
        /// <param name="localFileName">localFileName</param>
        /// <param name="serverFileName">serverFileName</param>
        /// <returns>true:success</returns>
        public bool UploadFile(string localFileName, string serverFileName)
        {
            if (!File.Exists(localFileName))
            {
                this.messageString = "The file: " + localFileName + " was not found." + " Cannot upload the file to the FTP site.";
                return false;
            }

            if (!this.isLogin)
            {
                return false;
            }

            if (!this.SetTransferMode(true))
            {
                return false;
            }

            ConnectDataSocket();

            if (this.sslData != null)
            {
                long offset = 0;

                if (offset > 0)
                {
                    this.SendCommand("REST " + offset);
                    if (this.responseCode != 350)
                    {
                        // The remote server may not support resuming.
                        offset = 0;
                    }
                }

                this.SendCommand("STOR " + serverFileName);
                if (!(this.responseCode == 125 || this.responseCode == 150))
                {
                    this.messageString = "STOR responseCode: " + responseString;
                    return false;
                }
                bool isZeroSize = true;
                if (this.useSSL == true)
                {
                    try
                    {
                        this.sslData.StartSSL(this.useValidate); // SSL Start.
                    }
                    catch
                    {
                        this.messageString = "Ssl policy error";
                        return false;
                    }
                }

                using (FileStream input = new FileStream(localFileName, FileMode.Open, FileAccess.Read))
                {
                    if (offset != 0)
                    {
                        input.Seek(offset, SeekOrigin.Begin);
                    }

                    int count = input.Read(this.buffer, 0, this.buffer.Length);
                    while (count > 0)
                    {
                        isZeroSize = false;
                        this.sslData.Send(this.buffer, count);
                        count = input.Read(this.buffer, 0, this.buffer.Length);
                    }
                }

                DisconnectDataSocket();
                this.ReadReply();
                if (!(this.responseCode == 226 || this.responseCode == 250 || (this.responseCode == 552 && isZeroSize)))
                {
                    this.messageString = responseString;
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// DeleteFile
        /// </summary>
        /// <param name="serverFileName">serverFileName</param>
        /// <returns>true:success</returns>
        public bool DeleteFile(string serverFileName)
        {
            if (!this.isLogin)
            {
                return false;
            }

            this.SendCommand("DELE " + serverFileName);
            if (this.responseCode != 250)
            {
                this.messageString = "DELE " + serverFileName + " responseCode: " + responseString;
                return false;
            }
            else
            {
                return true;
            }
        }

        /// <summary>
        /// RenameFile
        /// </summary>
        /// <param name="oldFileName">oldFileName</param>
        /// <param name="newFileName">newFileName</param>
        /// <returns>true:success</returns>
        public bool RenameFile(string oldFileName, string newFileName)
        {
            if (!this.isLogin)
            {
                return false;
            }

            this.SendCommand("RNFR " + oldFileName);
            if (this.responseCode != 350)
            {
                this.messageString = "RNFR " + oldFileName + " responseCode: " + responseString;
                return false;
            }

            this.SendCommand("RNTO " + newFileName);
            if (this.responseCode != 250)
            {
                this.messageString = "RNTO " + newFileName + " responseCode: " + responseString;
                return false;
            }

            return true;
        }

        /// <summary>
        /// CreateDirectory
        /// </summary>
        /// <param name="directoryName">directoryName</param>
        /// <returns>true:success</returns>
        public bool CreateDirectory(string directoryName)
        {
            if (!this.isLogin)
            {
                return false;
            }

            this.SendCommand("MKD " + directoryName);
            if (this.responseCode != 257)
            {
                this.messageString = "MKD " + directoryName + " responseCode: " + responseString;
                return false;
            }

            return true;
        }

        /// <summary>
        /// RemoveDirectory
        /// </summary>
        /// <param name="directoryName">directoryName</param>
        /// <returns>true:success</returns>
        public bool RemoveDirectory(string directoryName)
        {
            if (!this.isLogin)
            {
                return false;
            }
            string current_bak = this.currentDirectory;

            if (directoryName.StartsWith("ftp://"))
            {
                string[] path = directoryName.Replace("ftp://", "").Split('/');
                if (path.Length > 1)
                {
                    string dir = "";
                    for (int idx = 1; idx < path.Length ; idx++)
                    {
                        dir = dir + "/" + path[idx];
                    }
                    directoryName = dir;
                }
            }

            if (this.ChangeDirectory(directoryName))
            {
                string[] list = new string[0];
                GetFileDirList(null, out list);
                foreach (string f in list)
                {
                    // file delete
                    if (this.DeleteFile(f) == false)
                    {
                        if (RemoveDirectory(f) == false)
                            return false;
                    }
                }
            }

            this.ChangeDirectory(current_bak);

            this.SendCommand("RMD " + directoryName);
            if (this.responseCode != 250)
            {
                this.messageString = "RMD " + directoryName + " responseCode: " + responseString;
                return false;
            }

            return true;
        }

        /// <summary>
        /// Dispose
        /// </summary>
        public void Dispose()
        {
            this.Cleanup();
        }


        #endregion

        #region "Private Subs and Functions"
        /// <summary>
        /// Cleanup process
        /// </summary>
        private void Cleanup()
        {
            if (this.sslControl != null)
            {
                this.sslControl.Dispose();
                this.sslControl = null;
            }
            if (this.sslData != null)
            {
                this.sslData.Dispose();
                this.sslData = null;
            }
            if (this.controlSocket != null)
            {
                this.controlSocket.Close();
                this.controlSocket = null;
            }

            this.isLogin = false;
        }

        /// <summary>
        /// SetTransferMode
        /// </summary>
        /// <param name="isBinary">true:binary, false:ascii</param>
        /// <returns>true:success</returns>
        private bool SetTransferMode(bool isBinary)
        {
            if (isBinary)
            {
                this.SendCommand("TYPE I");
            }
            else
            {
                this.SendCommand("TYPE A");
            }

            if (this.responseCode == 200)
            {
                return true;
            }
            else
            {
                if (isBinary)
                {
                    this.messageString = "TYPE I responseCode: " + responseString;
                }
                else
                {
                    this.messageString = "TYPE A responseCode: " + responseString;
                }
                return false;
            }
        }

        /// <summary>
        /// GetResvData
        /// </summary>
        private void ReadReply()
        {
            int count = this.sslControl.Recv(this.buffer, this.timeout);
            string message = this.ASCII.GetString(this.buffer, 0, count);

            string[] messageList = message.Replace("\r", "").Split('\n');
            if (messageList.Length > 2)
            {
                message = messageList[messageList.Length - 2];
            }
            else
            {
                message = messageList[0];
            }

            if (!message.Substring(3, 1).Equals(" "))
            {
                this.ReadReply();
            }
            else
            {
                //Log.Log.Write(Log.Log.LogType.InternalInformation, Log.Log.LogLevel.Debug, string.Format("FTP Recv : {0}", message));
                this.responseString = message;
                this.responseCode = int.Parse(responseString.Substring(0, 3));
            }
        }

        /// <summary>
        /// SendCmmand
        /// </summary>
        /// <param name="command">command</param>
        private void SendCommand(string command)
        {
            command = command + "\r\n";
            byte[] cmdbytes = this.ASCII.GetBytes(command);

            //SSL_TEST.Program.GetForm().AddMessage(command);
            //Log.Log.Write(Log.Log.LogType.InternalInformation, Log.Log.LogLevel.Debug, string.Format("FTP Send : {0}", command));

            this.sslControl.Send(cmdbytes, cmdbytes.Length);
            this.ReadReply();
        }

        /// <summary>
        /// GetDataSocket
        /// </summary>
        /// <returns>DataSocket</returns>
        private void ConnectDataSocket()
        {
            DisconnectDataSocket();

            this.SendCommand("PASV");
            if (this.responseCode != 227)
            {
                this.messageString = "PASV responseCode: " + responseString;
                return;
            }

            int index1 = responseString.IndexOf("(");
            int index2 = responseString.IndexOf(")");
            string ipData = responseString.Substring(index1 + 1, index2 - index1 - 1);

            int len = ipData.Length;
            int partCount = 0;
            string buf = "";

            int[] parts = new int[6];
            for (int i = 0; i <= (len - 1) && partCount <= 6; i++)
            {
                char ch = ipData.Substring(i, 1)[0];
                if (Char.IsDigit(ch))
                {
                    buf += ch;
                }
                else if (ch != ',')
                {
                    this.messageString = "Malformed PASV reply: " + responseString;
                    return;
                }

                if ((ch == ',') || (i + 1 == len))
                {
                    try
                    {
                        parts[partCount] = int.Parse(buf);
                        partCount += 1;
                        buf = "";
                    }
                    catch
                    {
                        this.messageString = "Malformed PASV reply: " + responseString;
                        return;
                    }
                }
            }

            string ipAddress = string.Format("{0}.{1}.{2}.{3}", parts[0], parts[1], parts[2], parts[3]);

            int port = parts[4] << 8;
            port = port + parts[5];

            this.dataSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.IP);
            this.sslData = new SslHelperFW(dataSocket, this.serverName);

            byte[] adr = new byte[4];
            for (int i = 0; i < 4; i++)
                adr[i] = (byte)parts[i];

            IPEndPoint ep = new IPEndPoint(new IPAddress(adr), port);

            try
            {
                this.dataSocket.Connect(ep);
            }
            catch
            {
                this.messageString = "Cannot connect to remote server (" + ipAddress + " " + port.ToString() + ").";
                return;
            }
        }

        private void DisconnectDataSocket()
        {
            if (this.sslData != null)
            {
                this.sslData.Dispose();
                this.sslData = null;
            }
            if (this.dataSocket != null)
            {
                this.dataSocket.Close();
                this.dataSocket = null;
            }
        }
        #endregion
    }
}
