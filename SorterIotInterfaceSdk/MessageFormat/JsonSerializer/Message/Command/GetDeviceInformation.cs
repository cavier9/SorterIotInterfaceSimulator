using Glory.SorterInterface.Message;
using Glory.SorterInterface.MessageFormat.Message;
using System;
using System.Runtime.Serialization;

namespace Glory.SorterInterface.MessageFormat.JsonSerializer.Message.Command
{
    /// <summary>
    /// GetDeviceInformation Command クラス
    /// </summary>
    [DataContract]
    public class GetDeviceInformation : SorterIfMessageFormat
    {
        /// <summary>
        /// コンストラクタ
        /// </summary>
        public GetDeviceInformation()
            : base(MessageType.Command, MessageName.GetDeviceInformation)
        {
        }
    }
}
