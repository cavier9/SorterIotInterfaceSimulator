using Glory.SorterInterface.Message;
using Glory.SorterInterface.MessageFormat.Message;
using System;
using System.Runtime.Serialization;

namespace Glory.SorterInterface.MessageFormat.JsonSerializer.Message.Response
{
    /// <summary>
    /// CreateSession Response クラス
    /// </summary>
    [DataContract]
    public class CreateSession : SorterIfMessageFormat
    {
        /// <summary>
        /// コンストラクタ
        /// </summary>
        public CreateSession()
            : base(MessageType.Response, MessageName.CreateSession)
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

            /// <summary>
            /// CommandTopic
            /// </summary>
            [DataMember]
            public string CommandTopic { get; set; }
        }
    }
}
