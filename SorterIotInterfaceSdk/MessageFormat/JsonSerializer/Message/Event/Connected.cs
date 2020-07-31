using Glory.SorterInterface.Message;
using Glory.SorterInterface.MessageFormat.Message;
using System;
using System.Runtime.Serialization;

namespace Glory.SorterInterface.MessageFormat.JsonSerializer.Message.Event
{
    /// <summary>
    /// Connected Event クラス
    /// </summary>
    [DataContract]
    public class Connected : SorterIfMessageFormat
    {
        /// <summary>
        /// コンストラクタ
        /// </summary>
        public Connected()
            : base(MessageType.Event, MessageName.Connected)
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
