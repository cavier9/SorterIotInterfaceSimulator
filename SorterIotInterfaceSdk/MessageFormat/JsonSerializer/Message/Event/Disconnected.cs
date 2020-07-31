using Glory.SorterInterface.Message;
using Glory.SorterInterface.MessageFormat.Message;
using System;
using System.Runtime.Serialization;

namespace Glory.SorterInterface.MessageFormat.JsonSerializer.Message.Event
{
    /// <summary>
    /// Disconnected Event クラス
    /// </summary>
    [DataContract]
    public class Disconnected : SorterIfMessageFormat
    {
        /// <summary>
        /// コンストラクタ
        /// </summary>
        public Disconnected()
            : base(MessageType.Event, MessageName.Disconnected)
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
            /// DeviceName
            /// </summary>
            [DataMember]
            public string DeviceName { get; set; }
        }
    }
}
