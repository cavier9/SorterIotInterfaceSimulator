using System;
using System.Collections.Generic;

namespace Glory.SorterInterface.SorterInterfaceClient.ReConnect
{
    /// <summary>
    /// reconnect to broker interface
    /// </summary>
    internal interface IReconnectThread : IDisposable
    {
        /// <summary>
        /// is running
        /// </summary>
        bool IsRunning { get; }

        /// <summary>
        /// request start
        /// </summary>
        void RequestStart(IList<string> subscribeTopicList);

        /// <summary>
        /// request stop
        /// </summary>
        void RequestStop();

        /// <summary>
        /// reconnected process
        /// </summary>
        event ReconnectThread.ReconnectedProcessDelegate ReconnectedProcessEvent;
    }
}
