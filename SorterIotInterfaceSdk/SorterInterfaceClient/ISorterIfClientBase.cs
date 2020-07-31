using System;
using Glory.SorterInterface.SorterInterfaceClient.Log;

namespace Glory.SorterInterface.SorterInterfaceClient
{
    /// <summary>
    /// Sorter IoT Interface client interface
    /// </summary>
    public interface ISorterIfClientBase : IDisposable
    {
        /// <summary>
        /// Connection status between client and broker
        /// </summary>
        bool IsConnectedBroker { get; }

        /// <summary>
        /// message log
        /// </summary>
        IMessageLog MessageLog { get; }

        #region Disconnect Broker

        /// <summary>
        /// disconnect from Broker
        /// </summary>
        void DisconnectBroker();

        #endregion
    }
}
