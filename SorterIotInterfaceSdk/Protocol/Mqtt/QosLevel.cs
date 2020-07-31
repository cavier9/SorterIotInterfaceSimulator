using System;
using uPLibrary.Networking.M2Mqtt.Messages;

namespace Glory.SorterInterface.Protocol.Mqtt
{
    /// <summary>
    /// QoS(Quality of Service) level
    /// </summary>
    internal enum QosLevel : byte
    {
        /// <summary>
        /// 0: At most once
        /// </summary>
        AT_MOST_ONCE = MqttMsgBase.QOS_LEVEL_AT_MOST_ONCE,

        /// <summary>
        /// 1: At least once
        /// </summary>
        AT_LEAST_ONCE = MqttMsgBase.QOS_LEVEL_AT_LEAST_ONCE,

        /// <summary>
        /// 2: Exactly once
        /// </summary>
        EXACTLY_ONCE = MqttMsgBase.QOS_LEVEL_EXACTLY_ONCE,

        ///// <summary>
        ///// 3: Reserved
        ///// </summary>
        //RESERVED_3,
    }
}
