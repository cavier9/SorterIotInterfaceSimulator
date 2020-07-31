using Glory.SorterInterface.Message;
using Glory.SorterInterface.MessageFormat.Message;
using System;
using System.Runtime.Serialization;

namespace Glory.SorterInterface.MessageFormat.JsonSerializer.Message.Response
{
    /// <summary>
    /// SetOperator Response クラス
    /// </summary>
    [DataContract]
    public class SetOperator : SorterIfMessageFormat
    {
        /// <summary>
        /// コンストラクタ
        /// </summary>
        public SetOperator()
            : base(MessageType.Response, MessageName.SetOperator)
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
        }
    }
}
