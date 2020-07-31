using System;
using Glory.SorterInterface.SorterInterfaceClient.Log;

namespace Glory.SorterInterface.SorterInterfaceClient
{
    /// <summary>
    /// Sorter Interface protocol base interface
    /// </summary>
    internal interface ISorterIfProtocolBase : IDisposable
    {
        #region property

        /// <summary>
        /// message log
        /// </summary>
        IMessageLog MessageLog { get; }

        #endregion
        
        #region Disconnect Broker

        /// <summary>
        /// disconnect from Broker
        /// </summary>
        void DisconnectBroker();
            
        #endregion

        #region IsConnected Broker

        /// <summary>
        /// IsConnected Broker
        /// </summary>
        /// <returns>true: conencted / false disconencted</returns>
        bool IsConnectedBroker();

        #endregion

        #region send PUBLISH

        /// <summary>
        /// send publish
        /// </summary>
        /// <param name="topic">topic</param>
        /// <param name="jsonMessage">message</param>
        /// <param name="retain">Retain flag</param>
        /// <returns>true: send message success / false: send message fail</returns>
        bool SendPublish(string topic, string jsonMessage, bool retain);

        #endregion

        #region send SUBSCRIBE

        /// <summary>
        /// send subscribe
        /// </summary>
        /// <param name="topic">topic</param>
        /// <returns>true: send message success / false: send message fail</returns>
        bool SendSubscribe(string topic);

        #endregion

        #region send UNSUBSCRIBE

        /// <summary>
        /// send unsubscribe
        /// </summary>
        /// <param name="topic">topic</param>
        /// <returns>true: send message success / false: send message fail</returns>
        bool SendUnSubscribe(string topic);

        #endregion
    }
}
