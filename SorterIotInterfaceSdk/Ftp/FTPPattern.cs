using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Glory.SorterInterface.Ftp
{
    public class FTPPattern
    {
        #region Constructor


        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="FtpAccess.COMMAND_PATTERN">Ftp command (UPLOAD, UPLOAD_FILES, DOWNLOAD, DOWNLOAD_FILES)</param>
        /// <param name="url">FTP URL / DOWNLOAD:ftp url(FileName), other: ftp url(Directory)</param>
        /// <param name="fileDirectory">FILE PATH, DIRECTORY PATH / UPLOAD:FilePath , other: Directory</param>
        /// <param name="isRename">true : use rename</param>
        /// <param name="index">index</param>
        public FTPPattern(COMMAND_PATTERN command, string url, string fileDirectory, bool isRename, string index)
        {
            this.commandPattern = command;
            this.url = (url == null) ? string.Empty : url.Replace("ftps:", "ftp:"); ;
            this.fileDirectory = (fileDirectory == null) ? string.Empty : fileDirectory;
            this.isRename = isRename;
            this.index = (index == null) ? string.Empty : index;
        }

        #endregion

        #region parameter
        /// <summary>
        /// FTP PATTERN
        /// </summary>
        public enum COMMAND_PATTERN
        {
            /// <summary>
            /// upload
            /// </summary>
            UPLOAD,

            /// <summary>
            /// upload files
            /// </summary>
            UPLOAD_FILES,

            /// <summary>
            /// download
            /// </summary>
            DOWNLOAD,

            /// <summary>
            /// download files
            /// </summary>
            DOWNLOAD_FILES,

            /// <summary>
            /// Remove Directory
            /// </summary>
            REMOVE_DIR,
            
            /// <summary>
            /// Create Directory
            /// </summary>
            CREATE_DIR,

            /// <summary>
            /// FTP connect status
            /// </summary>
            CONNECT_STATUS
        }

        /// <summary>
        /// FTP COMMAND PATTERN
        /// </summary>      
        private COMMAND_PATTERN commandPattern;

        /// <summary>
        /// FTP URL (DOWNLOAD is url+FileName)
        /// </summary> 
        private string url;

        /// <summary>
        /// FILE AND DIRECTORY PATH (UPLOAD is FilePath)
        /// </summary>   
        private string fileDirectory;

        /// <summary>
        /// true : file name add ".tmp" 
        ///        (delete ".tmp" is after processing success)
        /// </summary>    
        private bool isRename;

        /// <summary>
        /// INDEX
        /// </summary>    
        private string index;

        #endregion

        #region propaty

        /// <summary>
        /// FTP COMMAND PATTERN (UPLOAD, UPLOAD_FILES, DOWNLOAD, DOWNLOAD_FILES)
        /// </summary>        
        public COMMAND_PATTERN ComandPattern
        {
            get { return commandPattern; }
            set { commandPattern = value; }
        }

        /// <summary>
        /// FTP URL / DOWNLOAD:ftp url(FileName), other: ftp url(Directory)
        /// </summary>        
        public string Url
        {
            get { return url; }
            set { url = value; }
        }

        /// <summary>
        /// FILE PATH, DIRECTORY PATH / UPLOAD:FilePath , other: Directory
        /// </summary>        
        public string FileDirectory
        {
            get { return fileDirectory; }
            set { fileDirectory = value; }
        }

        /// <summary>
        /// Rename setting (tre:use)
        /// </summary>        
        public bool IsRename
        {
            get { return isRename; }
            set { isRename = value; }
        }

        /// <summary>
        /// INDEX
        /// </summary>        
        public string INDEX
        {
            get { return index; }
            set { index = value; }
        }

        #endregion

    }
}
