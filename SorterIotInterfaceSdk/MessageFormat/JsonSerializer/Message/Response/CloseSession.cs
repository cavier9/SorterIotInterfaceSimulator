using Glory.SorterInterface.Message;
using Glory.SorterInterface.MessageFormat.Message;
using System;
using System.Runtime.Serialization;

namespace Glory.SorterInterface.MessageFormat.JsonSerializer.Message.Response
{
    /// <summary>
    /// CloseSession Response クラス
    /// </summary>
    [DataContract]
    public class CloseSession : SorterIfMessageFormat
    {
        /// <summary>
        /// コンストラクタ
        /// </summary>
        public CloseSession()
            : base(MessageType.Response, MessageName.CloseSession)
        {
        }

        /// <summary>
        /// Detail
        /// </summary>
        [DataMember]
        public detail Detail { get; set; }

        /// <summary>
        /// Detail
        /// </summary>
        public class detail
        {
            /// <summary>
            /// Result
            /// </summary>
            [DataMember]
            public string Result { get; set; }

            /// <summary>
            /// SessionID
            /// </summary>
            [DataMember]
            public string SessionID { get; set; }
        }
    }
}
