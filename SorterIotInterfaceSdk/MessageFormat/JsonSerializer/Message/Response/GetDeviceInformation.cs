using Glory.SorterInterface.Message;
using Glory.SorterInterface.MessageFormat.Message;
using System;
using System.Runtime.Serialization;

namespace Glory.SorterInterface.MessageFormat.JsonSerializer.Message.Response
{
    /// <summary>
    /// GetDeviceInformation Response クラス
    /// </summary>
    [DataContract]
    public class GetDeviceInformation : SorterIfMessageFormat
    {
        /// <summary>
        /// コンストラクタ
        /// </summary>
        public GetDeviceInformation()
            : base(MessageType.Response, MessageName.GetDeviceInformation)
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
