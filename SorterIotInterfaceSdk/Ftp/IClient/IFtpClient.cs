using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Glory.SorterInterface.Ftp
{
    /// <summary>
    /// IFtpClient interface
    /// </summary>
    public interface IFtpClient : IDisposable
    {
        /// <summary>
        /// ServerLogin
        /// </summary>
        /// <returns>result（true:success、false:error）</returns>
        bool Login();

        /// <summary>
        /// Logout
        /// </summary>
        void Logout();

        /// <summary>
        /// ExistsDirectory
        /// </summary>
        /// <param name="directoryName">directoryName</param>
        /// <returns>false：nothing</returns>
        bool ExistsDirectory(string directoryName);

        /// <summary>
        /// ReturnCurrentDirectory
        /// </summary>
        /// <returns>true:success</returns>
        bool ReturnCurrentDirectory();

        /// <summary>
        /// ChangecurrentDirectory
        /// </summary>
        /// <param name="directoryName">directoryName</param>
        /// <returns>true:success</returns>
        bool ChangeDirectory(string directoryName);

        /// <summary>
        /// GetCurrentDirectory
        /// </summary>
        /// <returns>true:success</returns>
        bool GetDirectory();

        /// <summary>
        /// GetFileDirList (NLST command)
        /// </summary>
        /// <param name="mask">Masking</param>
        /// <param name="fileList">fileList</param>
        /// <returns>true:success</returns>
        bool GetFileDirList(string mask, out string[] fileList);

        /// <summary>
        /// GetFileDirList (LIST command)
        /// </summary>
        /// <param name="mask">Masking</param>
        /// <param name="fileList">fileList</param>
        /// <returns>true:success</returns>
        bool GetFileList(string mask, out string[] fileList);

        /// <summary>
        /// GetFileSize
        /// </summary>
        /// <param name="fileName">fileName</param>
        /// <param name="fileSize">fileSize</param>
        /// <returns>true:success</returns>
        bool GetFileSize(string fileName, out long fileSize);

        /// <summary>
        /// DownloadFile
        /// </summary>
        /// <param name="serverFileName">serverFileName</param>
        /// <param name="localFileName">localFileName</param>
        /// <returns>true:success</returns>
        bool DownloadFile(string serverFileName, string localFileName);

        /// <summary>
        /// UploadFile
        /// </summary>
        /// <param name="localFileName">localFileName</param>
        /// <param name="serverFileName">serverFileName</param>
        /// <returns>true:success</returns>
        bool UploadFile(string localFileName, string serverFileName);

        /// <summary>
        /// DeleteFile
        /// </summary>
        /// <param name="serverFileName">serverFileName</param>
        /// <returns>true:success</returns>
        bool DeleteFile(string serverFileName);

        /// <summary>
        /// RenameFile
        /// </summary>
        /// <param name="oldFileName">oldFileName</param>
        /// <param name="newFileName">newFileName</param>
        /// <returns>true:success</returns>
        bool RenameFile(string oldFileName, string newFileName);

        /// <summary>
        /// CreateDirectory
        /// </summary>
        /// <param name="directoryName">directoryName</param>
        /// <returns>true:success</returns>
        bool CreateDirectory(string directoryName);

        /// <summary>
        /// RemoveDirectory
        /// </summary>
        /// <param name="directoryName">directoryName</param>
        /// <returns>true:success</returns>
        bool RemoveDirectory(string directoryName);

        /// <summary>
        /// IsLogin
        /// </summary>
        bool IsLogin { get; }

        /// <summary>
        /// ftp process message
        /// </summary>
        string MessageString { get; set; }
    }
}
