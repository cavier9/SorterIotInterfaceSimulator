using Glory.SorterInterface.Message;
using Glory.SorterInterface.MessageFormat.Message;
using System;
using System.Runtime.Serialization;

namespace Glory.SorterInterface.MessageFormat.JsonSerializer.Message.Event
{
    /// <summary>
    /// CounterfeitDetected Event クラス
    /// </summary>
    [DataContract]
    public class CounterfeitDetected : SorterIfMessageFormat
    {
        /// <summary>
        /// コンストラクタ
        /// </summary>
        public CounterfeitDetected()
            : base(MessageType.Event, MessageName.CounterfeitDetected)
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
            /// NoteInformation
            /// </summary>
            [DataMember]
            public noteInformation NoteInformation { get; set; }

            /// <summary>
            /// NoteInformation
            /// </summary>
            public class noteInformation
            {
                /// <summary>
                /// Key
                /// </summary>
                [DataMember]
                public string Key { get; set; }
            }
        }
    }
}
