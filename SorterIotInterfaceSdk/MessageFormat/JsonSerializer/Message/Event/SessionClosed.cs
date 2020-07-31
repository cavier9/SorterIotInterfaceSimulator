using Glory.SorterInterface.Message;
using Glory.SorterInterface.MessageFormat.Message;
using System;
using System.Runtime.Serialization;

namespace Glory.SorterInterface.MessageFormat.JsonSerializer.Message.Event
{
    /// <summary>
    /// SessionClosed Event クラス
    /// </summary>
    [DataContract]
    public class SessionClosed : SorterIfMessageFormat
    {
        /// <summary>
        /// コンストラクタ
        /// </summary>
        public SessionClosed()
            : base(MessageType.Event, MessageName.SessionClosed)
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
            /// SessionID
            /// </summary>
            [DataMember]
            public string SessionID { get; set; }
        }
    }
}
