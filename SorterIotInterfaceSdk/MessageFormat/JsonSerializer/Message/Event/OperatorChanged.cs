using Glory.SorterInterface.Message;
using Glory.SorterInterface.MessageFormat.Message;
using System;
using System.Runtime.Serialization;

namespace Glory.SorterInterface.MessageFormat.JsonSerializer.Message.Event
{
    /// <summary>
    /// OperatorChanged Event クラス
    /// </summary>
    [DataContract]
    public class OperatorChanged : SorterIfMessageFormat
    {
        /// <summary>
        /// コンストラクタ
        /// </summary>
        public OperatorChanged()
            : base(MessageType.Event, MessageName.OperatorChanged)
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
            /// ID
            /// </summary>
            [DataMember]
            public string ID { get; set; }
        }
    }
}
