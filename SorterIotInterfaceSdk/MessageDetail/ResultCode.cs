using System;

namespace Glory.SorterInterface.MessageDetail
{
    /// <summary>
    /// result code
    /// </summary>
    public class ResultCode
    {
        #region Informational
        // 1xx (Informational)

        #endregion

        #region Success
        // 2xx (Success)

        /// <summary>
        /// 200 Success
        /// </summary>
        public const string SUCCESS = "200 Success";

        /// <summary>
        /// 204 No Content
        /// </summary>
        public const string NO_CONTENT = "204 No Content";

        #endregion

        #region Redirection
        // 3xx Redirection

        #endregion

        #region ClientError
        // 4xx ClientError

        /// <summary>
        /// "400 Bad Request"
        /// </summary>
        public const string BAD_REQUEST = "400 Bad Request";

        /// <summary>
        /// 401 Unauthorized
        /// </summary>
        public const string UNAUTHORIZED = "401 Unauthorized";

        /// <summary>
        /// 402 Invalid Command
        /// </summary>
        public const string INVALID_COMMAND = "402 Invalid Command";

        #endregion

        #region DeviceError
        // 5xx DeviceError

        /// <summary>
        /// 500 Internal Device Error
        /// </summary>
        public const string INTERNAL_DEVICE_ERROR = "500 Internal Device Error";

        /// <summary>
        /// 501 Locked by other system
        /// </summary>
        public const string LOCKED_BY_OTHER_SYSTEM = "501 Locked by other system";

        /// <summary>
        /// 502 Session upper limits
        /// </summary>
        public const string SESSION_UPPER_LIMIT = "502 Session upper limits";

        /// <summary>
        /// 503 Locked by device
        /// </summary>
        public const string LOCKED_BY_DEVICE = "503 Locked by device";

        /// <summary>
        /// 504 FTP Error
        /// </summary>
        public const string FTP_ERROR = "504 FTP Error";

        #endregion
    }
}
