using System;

namespace Glory.SorterInterface.Protocol.Mqtt
{
    /// <summary>
    /// ReceiveProcessDelegate
    /// </summary>
    internal class ReceiveProcessDelegate
    {
        /// <summary>
        /// Delagate that defines event handler for message received
        /// </summary>
        public delegate void ReceiveProcess(string topic, string message);
    }
}
