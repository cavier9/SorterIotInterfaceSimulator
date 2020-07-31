using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Text.RegularExpressions;

namespace Glory.SorterInterface.Ftp
{
    public class FtpAccess : IDisposable
    {
        #region Constructor
        /// <summary>
        /// Constructor
        /// </summary>
        public FtpAccess(FTPConfigulation config, ReturnConnectResult rtnConnectResult, ReturnResultProcess rtnResultProcess)
        {
            try
            {
                string message = string.Empty;
                isEndThread = true;

                // set delegete
                returnConnectResult = rtnConnectResult;
                returnResultProcess = rtnResultProcess;

                // check config parameters
                if (config == null || this.isParameters(config, out message) == false)
                {
                    // error
                    if (returnConnectResult != null) returnConnectResult(FtpAccess.RESULT_PROCESS.FTP_CONNECTION_FAILED, message);
                    return;
                }
                this.config = config;
                isEndThread = false;

                // Connect and Send Command Thread
                ftpStart = new Thread(new ThreadStart(ftpClientConnectProcess));
                ftpStart.Start();

                Thread.Sleep(500);
            }
            catch (Exception ex)
            {
                isEndThread = true;
                if (returnConnectResult != null) returnConnectResult(FtpAccess.RESULT_PROCESS.FTP_CONNECTION_FAILED, ex.Message);
            }
        }
        #endregion


        #region parameter

        /// <summary>
        /// FTP STATUS
        /// </summary>
        public enum RESULT_PROCESS
        {
            /// <summary>
            /// Initial
            /// </summary>
            NONE,

            /// <summary>
            /// Success
            /// </summary>
            SUCCESS,

            /// <summary>
            /// Error
            /// </summary>
            ERROR,

            /// <summary>
            /// Login Failed
            /// </summary>
            FTP_LOGIN_FAILED,

            /// <summary>
            /// Connection Failed
            /// </summary>
            FTP_CONNECTION_FAILED,

            /// <summary>
            /// FTP Error
            /// </summary>
            FTP_ERROR,

            /// <summary>
            /// SSL Error
            /// </summary>
            SSL_ERROR,

            /// <summary>
            /// SEND PED Error
            /// </summary>
            PWD_ERROR,

            /// <summary>
            /// EXCEPTION Error
            /// </summary>
            EXCEPTION_ERROR,

            /// <summary>
            /// REMOVE DIRECTORY
            /// </summary>
            REMOVE_DIR,

            /// <summary>
            /// CREATE DIRECTORY
            /// </summary>
            CREATE_DIR,
        }

        /// <summary>
        /// FTP DOWNOAD and UPLOAD to dummy name
        /// </summary>
        private const string TMP_NAME = ".tmp";

        /// <summary>
        /// ErrorMessage : Login Failed
        /// </summary>
        private const string FTP_RESPONSE_LOGIN_FAILED = "530 End";

        /// <summary>
        /// ErrorMessage : Login Failed
        /// </summary>
        private const string FTP_RESPONSE_LOGIN_FAILED_CODE = "530";

        /// <summary>
        /// ErrorMessage : Cannot connect
        /// </summary>
        private const string FTP_RESPONSE_CONNECTION_FAILED = "Cannot connect to remote server";

        /// <summary>
        /// ErrorMessage : Ssl policy error
        /// </summary>
        private const string FTP_RESPONSE_SSL_ERROR = "Ssl policy error";

        /// <summary>
        /// FTP Configulation
        /// </summary>
        private FTPConfigulation config = null;

        /// <summary>
        /// FTP Client
        /// </summary>
        private volatile IFtpClient ftpClient = null;

        /// <summary>
        /// ExitThread
        /// </summary>
        private volatile bool isExitThread = false;

        /// <summary>
        /// FTP Status Message
        /// </summary>        
        private string ftpStatusMsg = "";

        /// <summary>
        /// FTP Status
        /// </summary>        
        private RESULT_PROCESS ftpStatus = RESULT_PROCESS.NONE;

        /// <summary>
        /// FTP Command Pattern List
        /// </summary>        
        private volatile List<FTPPattern> ftpCommandPatterList = new List<FTPPattern>();

        /// <summary>
        /// List Limit
        /// </summary>        
        private const int ftpCommandPatterLimit = 1000;

        /// <summary>
        /// FTP status
        /// </summary>
        private volatile bool isFtpConnect = false;

        /// <summary>
        /// Delagate for ResultProcess
        /// </summary>
        private ReturnResultProcess returnResultProcess;

        /// <summary>
        /// Delagate for ConnectProcessResult
        /// </summary>
        private ReturnConnectResult returnConnectResult;

        /// <summary>
        /// Thread
        /// </summary>
        private Thread ftpStart = null;

        /// <summary>
        /// check is FTP Process Thread END
        /// </summary>
        private volatile bool isEndThread = true;

        #endregion

        #region propaty

        /// <summary>
        /// FTP Conneted (true : connect)
        /// </summary>   
        public bool IsFtpConnect
        {
            get { return isFtpConnect; }
            set { value = isFtpConnect; }
        }

        /// <summary>
        /// FTP Command Count
        /// </summary>   
        public int IsFtpCommandCount
        {
            get { return ftpCommandPatterList.Count; }
        }

        /// <summary>
        /// check ftp Thread
        /// </summary>   
        public bool IsEndThread
        {
            get { return isEndThread; }
            set { value = isEndThread; }
        }

        /// <summary>
        /// Delagate for ResultProcess
        /// </summary>
        /// <param name="FTPPattern">ftpPattern parameter (allow to null)</param>
        /// <param name="RESULT_PROCESS">result</param>
        /// <param name="message">message</param> 
        public delegate void ReturnResultProcess(FTPPattern ftpPattern, FtpAccess.RESULT_PROCESS result, string message);

        /// <summary>
        /// Delagate for ConnectResult
        /// </summary>
        /// <param name="RESULT_PROCESS">result</param>
        public delegate void ReturnConnectResult(FtpAccess.RESULT_PROCESS result, string message);

        #endregion

        #region "Public Subs and Functions"

        /// <summary>
        /// FTP Command Set(upload / download)
        /// </summary>
        /// <param name="ftpPattern">ftpPattern parameter</param>
        /// <param name="errorMessage">error message</param>
        /// <returns>true:success</returns>
        public bool FtpCommandPatternSet(FTPPattern ftpPattern, out string errorMessage)
        {
            bool isRet = false;
            errorMessage = "";

            if (ftpStart == null)
            {
                errorMessage = "is not start Thread";
                return false;
            }
            if (isExitThread)
            {
                errorMessage = "Dispose in action";
                return false;
            }
            try
            {
                if (ftpPattern != null)
                {
                    //Regex reg = new Regex(@"(ftp)(:\/\/[-_.!~*\'()a-zA-Z0-9;\/?:\@&=+\$,%#]+)");

                    bool isError = true;
                    switch (ftpPattern.ComandPattern)
                    {
                        case FTPPattern.COMMAND_PATTERN.UPLOAD:
                        case FTPPattern.COMMAND_PATTERN.DOWNLOAD:
                        case FTPPattern.COMMAND_PATTERN.UPLOAD_FILES:
                        case FTPPattern.COMMAND_PATTERN.DOWNLOAD_FILES:
                            if (ftpPattern.Url == string.Empty ||
                                ftpPattern.Url == "/")
                            {
                                errorMessage = "Url is Empty";
                            }
                            //else if (reg.Match(ftpPattern.Url).Success == false)
                            //{
                            //    errorMessage = "Url error " + ftpPattern.Url;
                            //}
                            else if (ftpPattern.FileDirectory == string.Empty)
                            {
                                errorMessage = "FileDirectory is Empty";
                            }
                            else if (ftpCommandPatterList.Count > ftpCommandPatterLimit)
                            {
                                errorMessage = "List over count";
                            }
                            else
                            {
                                isError = false;
                            }
                            break;
                        case FTPPattern.COMMAND_PATTERN.REMOVE_DIR:
                        case FTPPattern.COMMAND_PATTERN.CREATE_DIR:
                        case FTPPattern.COMMAND_PATTERN.CONNECT_STATUS:
                            isError = false;
                            break;
                        default:
                            errorMessage = "ComandPattern is nothing";
                            break;
                    }
                    if (isError == false)
                    {
                        ftpCommandPatterList.Add(ftpPattern);
                        isRet = true;
                    }
                }
                else
                {
                    errorMessage = FTP_RESPONSE_CONNECTION_FAILED;
                }
            }
            catch (Exception ex)
            {
                errorMessage = ex.Message;
            }
            return isRet;
        }

        /// <summary>
        /// Dispose
        /// </summary>
        public void Dispose()
        {
            isExitThread = true;
        }


        #endregion

        #region "Private Subs and Functions"

        private bool isParameters(FTPConfigulation config, out string message)
        {
            bool isRet = false;
            message = string.Empty;

            if (config.FtpServer == string.Empty)
            {
                message = "FtpServer is Empty";
            }
            if (config.FtpUser == string.Empty)
            {
                message = "FtpUser is Empty";
            }
            else if (config.FtpPassword == string.Empty)
            {
                message = "FtpPassword is Empty";
            }
            else
            {
                isRet = true;
            }
            return isRet;
        }

        /// <summary>
        /// FTP Connect Process
        /// </summary>
        private void ftpClientConnectProcess()
        {
            ftpStatus = RESULT_PROCESS.NONE;
            ftpStatusMsg = string.Empty;

            //retry connect process delete
            //int connectErrorCount = 0;      // ftp connect error count
            int commandErrorCount = 0;      // command(upload/download) Error count


            try
            {

                while (true)
                {
                    // true : FTP END
                    if (isExitThread)
                    {
                        if (returnConnectResult != null) returnConnectResult(FtpAccess.RESULT_PROCESS.NONE, "Dispose");
                        if (returnResultProcess != null)
                        {
                            foreach (FTPPattern pattern in ftpCommandPatterList)
                            {
                                returnResultProcess(pattern, RESULT_PROCESS.NONE, "Dispose");
                            }
                            ftpCommandPatterList.Clear();
                        }
                        return;
                    }

                    try
                    {

                        // FTP Connect
                        if (ftpClient == null || ftpClient.IsLogin == false)
                        {
                            isFtpConnect = false;

                            ftpStatus = RESULT_PROCESS.NONE;
                            ftpClient = CreateConnection(out ftpStatus, out ftpStatusMsg);
                            if (ftpStatus == RESULT_PROCESS.SUCCESS)
                            {
                                isFtpConnect = true; // true : connected
                                if (returnConnectResult != null) returnConnectResult(ftpStatus, ftpStatusMsg); // connected
                                //connectErrorCount = 0;
                            }
                            else
                            {
                                if (returnConnectResult != null) returnConnectResult(ftpStatus, ftpStatusMsg); // error
                                CloseConnection(ftpClient);
                            }

                        }
                    }
                    catch (Exception ex)
                    {
                        CloseConnection(ftpClient);
                        if (returnConnectResult != null) returnConnectResult(RESULT_PROCESS.EXCEPTION_ERROR, ex.Message);
                    }

                    // connect error check (delete:retry loop process)
                    if (ftpClient == null || ftpClient.IsLogin == false) //&& config.FtpConnectRunCount != 0)
                    {
                        //connectErrorCount++;
                        //if (connectErrorCount >= config.FtpConnectRunCount)
                        //{
                        //    connectErrorCount = 0;
                        if (returnResultProcess != null)
                        {
                            foreach (FTPPattern pattern in ftpCommandPatterList)
                            {
                                returnResultProcess(pattern, RESULT_PROCESS.FTP_CONNECTION_FAILED, "run count over");
                            }
                        }
                        ftpCommandPatterList.Clear();
                        return;
                        //}
                    }

                    FTPPattern targetPattern = null;

                    try
                    {
                        // Upload / Download process

                        // interval timer
                        for (int timerCount = 0; timerCount < config.FtpInterval; timerCount++)
                        {
                            // true : FTP END
                            if (isExitThread)
                            {
                                if (returnConnectResult != null) returnConnectResult(FtpAccess.RESULT_PROCESS.NONE, "Dispose");
                                if (returnResultProcess != null)
                                {
                                    foreach (FTPPattern pattern in ftpCommandPatterList)
                                    {
                                        returnResultProcess(pattern, RESULT_PROCESS.NONE, "Dispose");
                                    }
                                }
                                ftpCommandPatterList.Clear();
                                return;
                            }

                            // connect status
                            if (ftpCommandPatterList.Count > 0)
                            {
                                List<FTPPattern> ptns = new List<FTPPattern>();
                                foreach (FTPPattern ftpPtn in ftpCommandPatterList)
                                {
                                    if (ftpPtn.ComandPattern == FTPPattern.COMMAND_PATTERN.CONNECT_STATUS) ptns.Add(ftpPtn);
                                }
                                foreach (FTPPattern resPtn in ptns)
                                {
                                    // result connect Status
                                    ftpCommandPatterList.Remove(resPtn);
                                    if (returnResultProcess != null) returnResultProcess(resPtn, ftpStatus, ftpStatusMsg);
                                    Thread.Sleep(1000);
                                }
                            }

                            // upload / download / remove
                            if (ftpCommandPatterList.Count > 0 && (ftpClient != null && ftpClient.IsLogin))
                            {
                                string message = string.Empty;
                                timerCount = 0; // count reset
                                ftpStatus = RESULT_PROCESS.NONE;
                                targetPattern = ftpCommandPatterList[0];

                                switch (ftpCommandPatterList[0].ComandPattern)
                                {
                                    case FTPPattern.COMMAND_PATTERN.UPLOAD:
                                        ftpStatus = this.FtpUpload(@ftpCommandPatterList[0].Url,
                                                                    ftpCommandPatterList[0].FileDirectory,
                                                                    ftpCommandPatterList[0].IsRename,
                                                                    ref message);
                                        break;
                                    case FTPPattern.COMMAND_PATTERN.DOWNLOAD:
                                        ftpStatus = this.FtpDownload(@ftpCommandPatterList[0].Url,
                                                                      ftpCommandPatterList[0].FileDirectory,
                                                                      ftpCommandPatterList[0].IsRename,
                                                                      ref message);
                                        break;
                                    case FTPPattern.COMMAND_PATTERN.UPLOAD_FILES:
                                        ftpStatus = this.FtpUploads(@ftpCommandPatterList[0].Url,
                                                                     ftpCommandPatterList[0].FileDirectory,
                                                                     ftpCommandPatterList[0].IsRename,
                                                                     ref message);
                                        break;
                                    case FTPPattern.COMMAND_PATTERN.DOWNLOAD_FILES:
                                        ftpStatus = this.FtpDownloads(@ftpCommandPatterList[0].Url,
                                                                       ftpCommandPatterList[0].FileDirectory,
                                                                       ftpCommandPatterList[0].IsRename,
                                                                       ref message);
                                        break;
                                    case FTPPattern.COMMAND_PATTERN.REMOVE_DIR:
                                        this.FtpRemoveDir(ftpCommandPatterList[0].Url, ref message);
                                        ftpStatus = RESULT_PROCESS.SUCCESS;
                                        break;
                                    case FTPPattern.COMMAND_PATTERN.CREATE_DIR:
                                        if (this.FtpCreateDir(ftpCommandPatterList[0].Url, ref message))
                                        {
                                            ftpStatus = RESULT_PROCESS.SUCCESS;
                                        }
                                        else
                                        {
                                            ftpStatus = RESULT_PROCESS.FTP_ERROR;
                                        }
                                        // ftpStatus = RESULT_PROCESS.SUCCESS;
                                        break;
                                }

                                if (ftpStatus == RESULT_PROCESS.SUCCESS)
                                {
                                    // delete commandPattern
                                    ftpCommandPatterList.RemoveAt(0);
                                    commandErrorCount = 0;

                                    if (returnResultProcess != null) returnResultProcess(targetPattern, ftpStatus, message);
                                }
                                else
                                {   // Error process

                                    // create dir
                                    if (ftpCommandPatterList[0].ComandPattern == FTPPattern.COMMAND_PATTERN.CREATE_DIR)
                                    {
                                        commandErrorCount = 0;
                                        if (returnResultProcess != null) returnResultProcess(targetPattern, ftpStatus, message);

                                        // delete commandPattern
                                        ftpCommandPatterList.RemoveAt(0);
                                    }
                                    else
                                    {
                                        if (config.FtpCommandRunCount > 0)
                                        {
                                            commandErrorCount++;
                                            if (commandErrorCount >= config.FtpCommandRunCount)
                                            {
                                                // RunCount over
                                                commandErrorCount = 0;
                                                if (returnResultProcess != null) returnResultProcess(targetPattern, ftpStatus, message);

                                                // delete commandPattern
                                                ftpCommandPatterList.RemoveAt(0);
                                            }
                                        }
                                    }
                                    CloseConnection(ftpClient);
                                    // go to reconnect
                                    break;
                                }
                            }
                            Thread.Sleep(1000); // 1sec * FtpInterval
                        }
                        targetPattern = null;

                        // Send "PWD" Command
                        if (ftpClient != null && ftpClient.IsLogin)
                        {
                            if (ftpClient.GetDirectory() == false)
                            {
                                // error
                                CloseConnection(ftpClient);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        if (returnResultProcess != null) returnResultProcess(targetPattern, RESULT_PROCESS.EXCEPTION_ERROR, ex.Message);
                        CloseConnection(ftpClient);
                    }

                }
            }
            catch
            {
            }
            finally
            {
                CloseConnection(ftpClient);
                isEndThread = true;
            }
        }


        /// <summary>
        /// Upload Request
        /// </summary>
        /// <param name="url">url</param>
        /// <param name="filePath">upload file path</param>
        /// <param name="isRename">true: use rename</param>
        /// <param name="message">message</param>
        /// <returns>RESULT_PROCESS</returns>
        private RESULT_PROCESS FtpUpload(string url, string filePath, bool isRename, ref string message)
        {
            RESULT_PROCESS ftpResult = RESULT_PROCESS.NONE;

            // check file
            if (filePath == string.Empty || File.Exists(@filePath) == false)
            {
                message = "file error " + filePath;
                return RESULT_PROCESS.ERROR;
            }

            try
            {
                ftpClient.MessageString = string.Empty;

                // Move Directory
                if (MoveCurrentDirectory(ftpClient, Path.GetDirectoryName(@url)) == false)
                {
                    ftpResult = RESULT_PROCESS.FTP_ERROR;
                }
                else
                {
                    ftpResult = RESULT_PROCESS.SUCCESS;

                    // upload
                    string uploadFileName = isRename == true ? Path.GetFileName(filePath) + TMP_NAME : Path.GetFileName(filePath);
                    if (ftpClient.UploadFile(filePath, uploadFileName) == false)
                    {
                        // upload error
                        ftpResult = RESULT_PROCESS.FTP_ERROR;
                    }
                    else
                    {
                        // upload file check
                        string[] fileArgs;

                        // get filelist
                        bool isRet = ftpClient.GetFileDirList("", out fileArgs);
                        if (isRet == false)
                        {
                            // get filelist error
                            return RESULT_PROCESS.FTP_ERROR;
                        }

                        if (fileArgs == null || fileArgs.Length == 0)
                        {
                            // get filelist error
                            ftpClient.MessageString = "uploadfile was not found.";
                            return RESULT_PROCESS.FTP_ERROR;
                        }

                        isRet = false;

                        // file check
                        foreach (string file in fileArgs)
                        {
                            if (uploadFileName.Equals(file))
                            {
                                isRet = true;
                                break;
                            }
                        }
                        if (isRet == false)
                        {
                            ftpClient.MessageString = "The file: " + uploadFileName + " was not found." + " Cannot upload the file.";
                            return RESULT_PROCESS.FTP_ERROR;
                        }
                        else
                        {
                            // rename
                            if (isRename)
                            {
                                if (ftpClient.RenameFile(uploadFileName, Path.GetFileName(filePath)) == false)
                                {
                                    return RESULT_PROCESS.FTP_ERROR;
                                }
                            }
                        }

                    }
                }
            }
            catch (Exception ex)
            {
                if (ftpClient != null) ftpClient.MessageString = ex.Message;
                ftpResult = RESULT_PROCESS.FTP_ERROR;
            }
            finally
            {
                if (ftpClient != null) message = ftpClient.MessageString;
            }
            return ftpResult;
        }


        /// <summary>
        /// Upload Request
        /// </summary>
        /// <param name="url">url</param>
        /// <param name="fileDir">upload directory</param>
        /// <param name="isRename">true: use rename</param>
        /// <param name="message">message</param>
        /// <returns>RESULT_PROCESS</returns>
        private RESULT_PROCESS FtpUploads(string url, string fileDir, bool isRename, ref string message)
        {
            RESULT_PROCESS ftpResult = RESULT_PROCESS.NONE;

            // check file
            if (fileDir == string.Empty)
            {
                message = "directory error " + fileDir;
                return RESULT_PROCESS.ERROR;
            }

            try
            {
                ftpClient.MessageString = string.Empty;

                // get File names
                string[] getFiles = Directory.GetFiles(fileDir, "*");
                if (getFiles.Length == 0)
                {
                    ftpClient.MessageString = "uploadfile was not found." + fileDir;
                    return RESULT_PROCESS.FTP_ERROR;
                }

                // Move Directory
                if (MoveCurrentDirectory(ftpClient, @url) == false)
                {
                    ftpResult = RESULT_PROCESS.FTP_ERROR;
                }
                else
                {
                    ftpResult = RESULT_PROCESS.SUCCESS;
                    foreach (string uploadFile in getFiles)
                    {
                        // Dirctory is not target
                        if (Path.GetExtension(Path.GetFileName(uploadFile)) == "") continue;

                        // upload
                        string uploadFileName = isRename == true ? Path.GetFileName(uploadFile) + TMP_NAME : Path.GetFileName(uploadFile);
                        if (ftpClient.UploadFile(uploadFile, uploadFileName) == false)
                        {
                            ftpResult = RESULT_PROCESS.FTP_ERROR;
                            break;
                        }
                    }
                }

                if (ftpResult == RESULT_PROCESS.SUCCESS)
                {
                    string[] fileArgs;

                    // get filelist
                    bool isRet = ftpClient.GetFileDirList("", out fileArgs);
                    if (isRet == false)
                    {
                        // get filelist error
                        return RESULT_PROCESS.FTP_ERROR;
                    }

                    if (fileArgs == null || fileArgs.Length == 0)
                    {
                        // get filelist error
                        ftpClient.MessageString = "uploadfile was not found.";
                        return RESULT_PROCESS.FTP_ERROR;
                    }

                    isRet = false;

                    // file check
                    foreach (string uploadFile in getFiles)
                    {
                        // Dirctory is not target
                        if (Path.GetExtension(Path.GetFileName(uploadFile)) == "") continue;

                        string uploadFileName = isRename == true ? Path.GetFileName(uploadFile) + TMP_NAME : Path.GetFileName(uploadFile);
                        foreach (string file in fileArgs)
                        {
                            if (uploadFileName.Equals(file))
                            {
                                isRet = true;
                                break;
                            }
                        }
                        if (isRet == false)
                        {
                            ftpClient.MessageString = "The file: " + uploadFileName + " was not found." + " Cannot upload the file.";
                            return RESULT_PROCESS.FTP_ERROR;
                        }

                        // rename
                        if (isRename)
                        {
                            if (ftpClient.RenameFile(uploadFileName, Path.GetFileName(uploadFile)) == false)
                            {
                                return RESULT_PROCESS.FTP_ERROR;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                if (ftpClient != null) ftpClient.MessageString = ex.Message;
                ftpResult = RESULT_PROCESS.FTP_ERROR;
            }
            finally
            {
                if (ftpClient != null) message = ftpClient.MessageString;
            }

            return ftpResult;

        }


        /// <summary>
        /// Download Request
        /// </summary>
        /// <param name="fileUrl">fileUrl</param>
        /// <param name="dir">download path</param>
        /// <param name="isRename">true: use rename</param>
        /// <param name="message">process message</param>
        /// <returns>RESULT_PROCESS</returns>
        private RESULT_PROCESS FtpDownload(string fileUrl, string dir, bool isRename, ref string message)
        {
            RESULT_PROCESS ftpResult = RESULT_PROCESS.NONE;

            // check file
            if (dir == string.Empty)
            {
                message = "directory error " + dir;
                return RESULT_PROCESS.ERROR;
            }

            try
            {
                ftpClient.MessageString = string.Empty;

                // file download
                //string[] path = (fileUrl.Replace("ftp://", "")).Split('/');
                string[] fileArgs;

                // Move Directory
                if (MoveCurrentDirectory(ftpClient, Path.GetDirectoryName(@fileUrl)) == false)
                {
                    return RESULT_PROCESS.FTP_ERROR;
                }

                // get filelist
                bool isRet = ftpClient.GetFileDirList("", out fileArgs);
                if (isRet == false)
                {
                    // get filelist error
                    return RESULT_PROCESS.FTP_ERROR;
                }

                if (fileArgs == null || fileArgs.Length == 0)
                {
                    // get filelist error
                    ftpClient.MessageString = "downloadfile was not found.";
                    return RESULT_PROCESS.FTP_ERROR;
                }

                isRet = false;

                // download file check
                foreach (string file in fileArgs)
                {
                    if (Path.GetFileName(fileUrl).Equals(file))
                    {
                        isRet = true;
                        break;
                    }
                }
                if (isRet == false)
                {
                    ftpClient.MessageString = "The file: " + Path.GetFileName(fileUrl) + " was not found." + " Cannot download the file.";
                    return RESULT_PROCESS.FTP_ERROR;
                }

                // download destination file name.
                string filePath = Path.Combine(dir, Path.GetFileName(fileUrl));
                string downloadFilePath = isRename == true ? filePath + TMP_NAME : filePath;

                // create dir
                string downloadPath = GetParent(filePath.TrimEnd(Path.DirectorySeparatorChar)).ToString();
                if (Directory.Exists(downloadPath) == false)
                {
                    Directory.CreateDirectory(downloadPath);
                }

                // download process
                ftpResult = RESULT_PROCESS.SUCCESS;
                if (ftpClient.DownloadFile(Path.GetFileName(fileUrl), downloadFilePath) == false)
                {
                    ftpResult = RESULT_PROCESS.FTP_ERROR;
                }
                else
                {
                    // check file
                    if (File.Exists(@downloadFilePath))
                    {
                        if (isRename)
                        {
                            // rename download file
                            File.Copy(@downloadFilePath, @filePath, true);

                            //delete ".tmp"
                            try { File.Delete(@downloadFilePath); }
                            catch { }
                        }
                    }
                    else
                    {   // error
                        ftpClient.MessageString = "The file: " + downloadFilePath + " was not found." + " Cannot download the file.";
                        return RESULT_PROCESS.FTP_ERROR;
                    }
                }


            }
            catch (Exception ex)
            {
                if (ftpClient != null) ftpClient.MessageString = ex.Message;
                ftpResult = RESULT_PROCESS.FTP_ERROR;
            }
            finally
            {
                if (ftpClient != null) message = ftpClient.MessageString;
            }

            return ftpResult;
        }


        /// <summary>
        /// Download Request
        /// </summary>
        /// <param name="dirUrl">directory Url</param>
        /// <param name="dir">download path</param>
        /// <param name="isRename">true: use rename</param>
        /// <param name="message">process message</param>
        /// <returns>RESULT_PROCESS</returns>
        private RESULT_PROCESS FtpDownloads(string dirUrl, string dir, bool isRename, ref string message)
        {
            RESULT_PROCESS ftpResult = RESULT_PROCESS.NONE;

            // check file
            if (dir == string.Empty)
            {
                message = "directory error " + dir;
                return RESULT_PROCESS.ERROR;
            }

            try
            {
                ftpClient.MessageString = string.Empty;

                // file download
                //string[] path = (dirUrl.Replace("ftp://", "")).Split('/');
                string[] fileArgs;

                // Move Directory
                if (MoveCurrentDirectory(ftpClient, @dirUrl) == false)
                {
                    return RESULT_PROCESS.FTP_ERROR;
                }

                // get filelist
                bool isRet = ftpClient.GetFileDirList("", out fileArgs);
                if (isRet == false)
                {
                    // get filelist error
                    return RESULT_PROCESS.FTP_ERROR;
                }

                if (fileArgs == null || fileArgs.Length == 0)
                {
                    // get filelist error
                    ftpClient.MessageString = "downloadfile was not found.";
                    return RESULT_PROCESS.FTP_ERROR;
                }

                isRet = false;

                // download file check
                foreach (string file in fileArgs)
                {
                    // Dirctory is not target
                    if (Path.GetExtension(Path.GetFileName(file)) == "") continue;

                    // download destination file name.
                    string filePath = Path.Combine(dir, Path.GetFileName(file));
                    string downloadFilePath = isRename == true ? filePath + TMP_NAME : filePath;

                    // create dir
                    string downloadPath = GetParent(filePath.TrimEnd(Path.DirectorySeparatorChar)).ToString();
                    if (Directory.Exists(downloadPath) == false)
                    {
                        Directory.CreateDirectory(downloadPath);
                    }

                    // download process
                    ftpResult = RESULT_PROCESS.SUCCESS;
                    if (ftpClient.DownloadFile(Path.GetFileName(file), downloadFilePath) == false)
                    {
                        ftpResult = RESULT_PROCESS.FTP_ERROR;
                        break;
                    }
                }

                if (ftpResult == RESULT_PROCESS.SUCCESS)
                {
                    // get File names
                    string[] getFiles = Directory.GetFiles(dir, "*");
                    string upfile = string.Empty;

                    // file check
                    foreach (string file in fileArgs)
                    {
                        // Dirctory is not target
                        if (Path.GetExtension(Path.GetFileName(file)) == "") continue;

                        string filename = isRename == true ? file + TMP_NAME : file;

                        foreach (string downloadFile in getFiles)
                        {
                            if (Path.GetFileName(downloadFile).Equals(filename))
                            {
                                isRet = true;

                                if (isRename)
                                {
                                    // rename download file
                                    string renamePAth = Path.Combine(dir, Path.GetFileName(file));
                                    File.Copy(@downloadFile, @renamePAth, true);

                                    //delete ".tmp"
                                    try { File.Delete(@downloadFile); }
                                    catch { }
                                }
                                break;
                            }
                        }
                        if (isRet == false)
                        {
                            ftpClient.MessageString = "The file: " + Path.GetFileName(file) + " was not found." + " Cannot download the file.";
                            return RESULT_PROCESS.FTP_ERROR;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                if (ftpClient != null) ftpClient.MessageString = ex.Message;
                ftpResult = RESULT_PROCESS.FTP_ERROR;
            }
            finally
            {
                if (ftpClient != null) message = ftpClient.MessageString;
            }

            return ftpResult;
        }

        /// <summary>
        /// Remove Directory Request
        /// </summary>
        /// <param name="dirUrl">directory Url</param>
        private void FtpRemoveDir(string dirUrl, ref string message)
        {
            // directory delete
            try
            {
                ftpClient.RemoveDirectory(@dirUrl);
            }
            catch (Exception ex)
            {
                message = ex.Message;
            }
        }

        /// <summary>
        /// Current Director Request
        /// </summary>
        /// <param name="dirUrl">directory Url</param>
        private bool FtpCreateDir(string dirUrl, ref string message)
        {
            // @@@ chg USF-200_機能改善 2018.08.02 by Hemmi ↓
            if (MoveCurrentDirectory(ftpClient, @dirUrl))
            {// already exist
                message = "Already exist. " + dirUrl;
                return true;
            }
            // @@@ chg USF-200_機能改善 2018.08.02 by Hemmi ↑

            bool isSucess = this.CreateAndMoveCurrentDirectory(ftpClient, @dirUrl);
            message = ftpClient.MessageString;
            return isSucess;
        }

        /// <summary>
        /// MoveCurrentDirector
        /// (folder nothing is create)
        /// </summary>
        /// <param name="ftp">FtpClient</param>
        /// <param name="url">url</param>
        /// <returns>true :success</returns>
        private bool CreateAndMoveCurrentDirectory(IFtpClient ftp, string url)
        {

            if (ftp == null || ftp.IsLogin == false)
            {
                return false;
            }

            try
            {
                string[] path = (url.Replace("ftp://", "")).Split('/');

                // CurrentDirector
                string dirName = string.Empty;
                for (int i = 0; i < path.Length; i++)   // @@@ chg USF-200_機能改善 2018.01.23 by Hemmi
                {
                    dirName += path[i] + "/";

                    if (ftp.ExistsDirectory(dirName) == false)
                    {
                        // @@@ chg USF-200_機能改善 2018.08.02 by Hemmi ↓
                        ftp.MessageString = string.Empty;
                        // @@@ chg USF-200_機能改善 2018.08.02 by Hemmi ↑
                        if (ftp.CreateDirectory(dirName) == false)
                        {
                            return false;
                        }
                    }
                }

                // Move CurrentDirector 
                return ftp.ChangeDirectory(dirName);
            }
            catch (Exception ex)
            {
                ftp.MessageString = "CreateAndMoveCurrentDirectory FILED. " + ex.Message;
                return false;
            }

        }

        /// <summary>
        /// MoveCurrentDirectory
        /// (folder nothing is return)
        /// </summary>
        /// <param name="ftp">FtpClientト</param>
        /// <param name="url">url</param>
        /// <returns>true :success</returns>
        private bool MoveCurrentDirectory(IFtpClient ftp, string url)
        {
            if (ftp == null || ftp.IsLogin == false)
            {
                return false;
            }

            try
            {
                string[] path = (url.Replace("ftp://", "")).Split('/');
                // @@@ chg USF-200_機能改善 2018.08.02 by Hemmi ↓
                if (path[0] == string.Empty)
                {
                    // Move RootDirector
                    ftpClient.ChangeDirectory("/");
                }
                ftp.MessageString = string.Empty;
                // @@@ chg USF-200_機能改善 2018.08.02 by Hemmi ↑

                string dirName = string.Empty;
                for (int i = 0; i < path.Length; i++)   // @@@ chg USF-200_機能改善 2018.01.23 by Hemmi
                {
                    dirName += path[i] + "/";

                    if (ftp.ExistsDirectory(dirName) == false)
                    {
                        return false;
                    }
                }

                // Mov CurrentDirectory
                return ftp.ChangeDirectory(dirName);
            }
            catch (Exception ex)
            {
                ftp.MessageString = "MoveCurrentDirectory FILED. " + ex.Message;
                return false;
            }
        }

        /// <summary>
        /// GetCurrentDirectory
        /// (Directory.GetParent not support in CompactFramework)
        /// </summary>
        /// <param name="path"></param>
        /// <returns>true :success</returns>
        private string GetParent(string path)
        {
            string[] arg = path.Split(Path.DirectorySeparatorChar);
            string ret = string.Empty;

            for (int i = 0; i < arg.Length - 1; i++)
            {
                if (i != 0)
                {
                    ret += "\\";
                }
                ret += arg[i];
            }

            return ret;
        }


        /// <summary>
        /// ftp server connect
        /// </summary>
        private IFtpClient CreateConnection(out RESULT_PROCESS result, out string message)
        {
            result = RESULT_PROCESS.NONE;
            message = string.Empty;

            string[] ftpServerName = (this.config.FtpServer.Replace("ftp://", "")).Split('/');
            IFtpClient ftp = new FtpClient(ftpServerName[0]
                                          , config.FtpPort
                                          , config.IsUseSSL
                                          , config.SSLmode
                                          , config.ValidateCertificate
                                          , config.FtpUser
                                          , config.FtpPassword
                                          , config.FtpTimeout
                                          , @"/");

            if (ftp != null)
            {
                try
                {
                    if (ftp.Login() == true)    // FTP Login
                    {
                        result = RESULT_PROCESS.SUCCESS;
                        return (ftp);
                    }
                    else
                    {
                        switch (ftp.MessageString)
                        {
                            case FTP_RESPONSE_CONNECTION_FAILED:
                                result = RESULT_PROCESS.FTP_CONNECTION_FAILED;
                                break;
                            case FTP_RESPONSE_LOGIN_FAILED:
                            case FTP_RESPONSE_LOGIN_FAILED_CODE:
                                result = RESULT_PROCESS.FTP_LOGIN_FAILED;
                                break;
                            case FTP_RESPONSE_SSL_ERROR:
                                result = RESULT_PROCESS.SSL_ERROR;
                                break;
                            default:
                                result = RESULT_PROCESS.FTP_ERROR;
                                break;
                        }
                        message = ftp.MessageString;
                    }
                }
                catch (Exception ex)
                {
                    result = RESULT_PROCESS.ERROR;
                    message = ex.Message;
                    ftp.Dispose();
                }
            }
            return (null);
        }

        /// <summary>
        /// CloseConnection
        /// </summary>
        /// <param name="ftp">close FtpClient</param>
        private void CloseConnection(IFtpClient ftp)
        {
            isFtpConnect = false;
            if (ftp != null)
            {
                ftp.Logout();
                ftp.Dispose();
                ftp = null;
            }
        }


        #endregion
    }
}
