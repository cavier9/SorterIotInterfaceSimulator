using System;
using Glory.SorterInterface.SorterInterfaceClient.Log;
using Glory.SorterInterface.SorterInterfaceClient.Client;

namespace Glory.SorterInterface.SorterInterfaceClient
{
    /// <summary>
    /// Sorter IoT Interface direct send message interface
    /// </summary>
    internal interface ISendLowMessage : IDisposable
    {
        #region property

        /// <summary>
        /// SorterIfClient
        /// </summary>
        ISorterIfClient Client { get; }

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
