using System;

namespace Glory.SorterInterface.Protocol.Mqtt
{
    internal interface IMqttProtocol : IDisposable
    {
        /// <summary>
        /// Connection status between client and broker
        /// </summary>
        bool IsConnected();

        /// <summary>
        /// set receive process
        /// </summary>
        /// <param name=" function">receive process</param>
        void SetReceiveProcess(ReceiveProcessDelegate.ReceiveProcess receiveProcess);
        
        /// <summary>
        /// connect to MQTT broker
        /// </summary>
        /// <param name="clientId">Client identifier</param>
        /// <param name="username">Username</param>
        /// <param name="password">Password</param>
        /// <param name="willFlag">Will flag</param>
        /// <param name="willTopic">Will topic</param>
        /// <param name="willMessage">Will message</param>
        /// <returns>is connect success</returns>
        bool Connect(string clientId, string username, string password, bool willFlag, string willTopic, string willMessage);

        /// <summary>
        /// Reconnect to broker
        /// </summary>
        /// <returns>is connect success</returns>
        bool Reconnect();

        /// <summary>
        /// Disconnect from broker
        /// </summary>
        void Disconnect();
        
        /// <summary>
        /// send SUBSCRIBE
        /// </summary>
        /// <param name="topic"></param>
        /// <returns></returns>
        bool SendSubscribe(string topic);
        
        /// <summary>
        /// send PUBLISH
        /// </summary>
        /// <param name="topic">Message topic</param>
        /// <param name="message">Message data </param>
        /// <param name="retain">Retain flag</param>
        /// <returns>Message Id related to PUBLISH message</returns>
        bool SendPublish(string topic, string message, bool retain);
        
        /// <summary>
        /// send UNSUBSCRIBE
        /// </summary>
        /// <param name="topic"></param>
        /// <returns></returns>
        bool SendUnSubscribe(string topic);

        /// <summary>
        /// send UNSUBSCRIBE
        /// </summary>
        /// <param name="topics"></param>
        /// <returns></returns>
        bool SendUnSubscribe(string[] topics);
    }
}
