using System;
using System.Text;
using uPLibrary.Networking.M2Mqtt;
using uPLibrary.Networking.M2Mqtt.Exceptions;
using uPLibrary.Networking.M2Mqtt.Messages;
using Glory.SorterInterface.Exceptions;
using Glory.SorterInterface.SorterInterfaceClient;

namespace Glory.SorterInterface.Protocol.Mqtt
{
    /// <summary>
    /// MQTT protocol
    /// </summary>
    internal class MqttProtocol : IMqttProtocol
    {
        #region fixed parameter

        /// <summary>
        /// Ping to MQTT Broker interval time 
        /// </summary>
        private const int PING_INTERVAL_TIME = 60;

        #endregion

        #region property

        /// <summary>
        /// Connection status between client and broker
        /// </summary>
        public bool IsConnected()
        {
            return this.client.IsConnected;
        }

        #endregion

        #region parameter

        /// <summary>
        /// lock object
        /// </summary>
        private object lockObj = new Object();

        /// <summary>
        /// MQTT client
        /// </summary>
        private IMqttProtocolClient client = null;

        /// <summary>
        /// receive message process
        /// </summary>
        private ReceiveProcessDelegate.ReceiveProcess receiveProcess;

        /// <summary>
        /// QoS level
        /// </summary>
        private QosLevel qosLevel = QosLevel.AT_MOST_ONCE;

        /// <summary>
        /// Client identifier
        /// </summary>
        private string clientId = string.Empty;

        /// <summary>
        /// username
        /// </summary>
        private string username = string.Empty;

        /// <summary>
        /// password
        /// </summary>
        private string password = string.Empty;

        /// <summary>
        /// Will retain flag
        /// </summary>
        private bool willRetainFlag = false;

        /// <summary>
        /// Will QOS level
        /// </summary>
        private QosLevel willQosLevel = QosLevel.AT_MOST_ONCE;

        /// <summary>
        /// Will flag
        /// </summary>
        private bool willFlag = false;

        /// <summary>
        /// Will topic
        /// </summary>
        private string willTopic = string.Empty;

        /// <summary>
        /// Will message
        /// </summary>
        private string willMessage = string.Empty;

        /// <summary>
        /// Clean sessione flag
        /// </summary>
        private bool cleanSessionFlag = true;

        /// <summary>
        /// Keep alive period
        /// </summary>
        private ushort keepAlivePeriod = MqttProtocol.PING_INTERVAL_TIME;
        
        #endregion

        #region constructor

        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="mqttProtocolClient">MQTT client</param>
        /// <param name="qosLevel">QoS level</param>
        private MqttProtocol(IMqttProtocolClient mqttProtocolClient, QosLevel qosLevel)
        {
            this.client = mqttProtocolClient;
            this.qosLevel = qosLevel;
            this.willQosLevel = qosLevel;

            // event handler message received
            this.client.PublishReceived += new MqttClient.MqttMsgPublishEventHandler(ReceiveMqttMessage);
        }

        /// <summary>
        /// constructor (plain text)
        /// </summary>
        /// <param name="hostName">MQTT broker host name</param>
        /// <param name="qosLevel">QoS level</param>
        internal MqttProtocol(string hostName, QosLevel qosLevel)
            : this(new MqttProtocolClient(hostName), qosLevel)
        {
        }

#if M2MQTT_V3
        /// <summary>
        /// constructor (SSL/TLS. Server certificate)
        /// </summary>
        /// <param name="hostName">MQTT broker host name</param>
        /// <param name="port">MQTT broker port number</param>
        /// <param name="secure">Using secure connection</param>
        /// <param name="certificate">Using server certificate</param>
        /// <param name="sslProtocol">SSL/TLS protocol versions</param>
        /// <param name="qosLevel">QoS level</param>
        internal MqttProtocol(string hostName, int port, bool secure, bool certificate, SorterInterfaceClient.SslProtocols sslProtocol, QosLevel qosLevel)
            : this(new MqttProtocolClient(hostName, port, secure, certificate, (MqttSslProtocols)sslProtocol), qosLevel)
        {
        }
#else
        /// <summary>
        /// constructor (Secure. Server certificate)
        /// </summary>
        /// <param name="hostName">MQTT broker host name</param>
        /// <param name="port">MQTT broker port number</param>
        /// <param name="secure">Using secure connection</param>
        /// <param name="certificate">Using server certificate</param>
        /// <param name="qosLevel">QoS level</param>
        internal MqttProtocol(string hostName, int port, bool secure, bool certificate, QosLevel qosLevel)
            : this(new MqttProtocolClient(hostName, port, secure, certificate), qosLevel)
        {
        }
#endif

        #endregion

        #region IDisposable interface implement

        /// <summary>
        /// Dispose
        /// </summary>
        public void Dispose()
        {
            if( this.client.IsConnected == true)
            {
                this.Disconnect();
            }
            this.client.Dispose();
            this.client = null;
        }

        #endregion

        #region receive process

        /// <summary>
        /// event handler for message received
        /// </summary>
        /// <param name="sender">sender</param>
        /// <param name="eventArgs">eventArgs</param>
        private void ReceiveMqttMessage(object sender, MqttMsgPublishEventArgs eventArgs)
        {
            string topic = eventArgs.Topic;
            string message = Encoding.UTF8.GetString(eventArgs.Message, 0, eventArgs.Message.GetLength(0));

            if (this.receiveProcess != null)
            {
                this.receiveProcess(topic, message);
            }
        }

        /// <summary>
        /// set receive process
        /// </summary>
        /// <param name=" function">receive process</param>
        public void SetReceiveProcess(ReceiveProcessDelegate.ReceiveProcess receiveProcess)
        {
            this.receiveProcess = receiveProcess;
        }

        #endregion

        #region send CONNECT

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
        public bool Connect(string clientId, string username, string password, bool willFlag, string willTopic, string willMessage)
        {
            lock (lockObj)
            {
                this.clientId = clientId;
                this.username = username;
                this.password = password;
                if (willFlag == true)
                {
                    this.willFlag = true;
                    this.willTopic = willTopic;
                    this.willMessage = willMessage;
                }
                else
                {
                    this.willFlag = false;
                    this.willTopic = string.Empty;
                    this.willMessage = string.Empty;
                }

                // send CONNECT message
                return SendConnect();
            }
        }

        /// <summary>
        /// reconnect to MQTT broker
        /// </summary>
        /// <returns>is connect success</returns>
        public bool Reconnect()
        {
            lock (lockObj)
            {
                // send CONNECT message
                return SendConnect();
            }
        }

        /// <summary>
        /// send CONNECT message
        /// </summary>
        /// <returns>is connect success</returns>
        private bool SendConnect()
        {
            byte ret = this.SendConnect(
                this.clientId,
                this.username,
                this.password,
                this.willRetainFlag,
                (byte)this.willQosLevel,
                this.willFlag,
                this.willTopic,
                this.willMessage,
                this.cleanSessionFlag,
                this.keepAlivePeriod);

            return (ret == MqttMsgConnack.CONN_ACCEPTED);
        }

        /// <summary>
        /// send CONNECT message
        /// </summary>
        /// <param name="clientId">Client identifier</param>
        /// <param name="username">Username</param>
        /// <param name="password">Password</param>
        /// <param name="willRetain">Will retain flag</param>
        /// <param name="willQosLevel">Will QOS level</param>
        /// <param name="willFlag">Will flag</param>
        /// <param name="willTopic">Will topic</param>
        /// <param name="willMessage">Will message</param>
        /// <param name="cleanSession">Clean sessione flag</param>
        /// <param name="keepAlivePeriod">Keep alive period</param>
        /// <returns>Return code of CONNACK message from broker</returns>
        private byte SendConnect(
            string clientId, string username, string password, bool willRetainFlag, byte willQosLevel,
            bool willFlag, string willTopic, string willMessage, bool cleanSessionFlag, ushort keepAlivePeriod)
        {
            if (this.client == null)
            {
                return MqttMsgConnack.CONN_REFUSED_SERVER_UNAVAILABLE;
            }
            if (this.client.IsConnected == true)
            {
                return MqttMsgConnack.CONN_REFUSED_SERVER_UNAVAILABLE;
            }

            try
            {
                if (willFlag == true)
                {
                    // Connect to broker
                    return this.client.Connect(
                        clientId,
                        username,
                        password,
                        willRetainFlag,
                        willQosLevel,
                        willFlag,
                        willTopic,
                        willMessage,
                        cleanSessionFlag,
                        keepAlivePeriod);
                }
                else
                {
                    // Connect to broker
                    return this.client.Connect(
                        clientId,
                        username,
                        password,
                        cleanSessionFlag,
                        keepAlivePeriod);
                }
            }
            catch(MqttClientException ex)
            {
                throw new SorterIfConnectionException(ex);
            }
            catch (MqttConnectionException ex)
            {
                throw new SorterIfConnectionException(ex);
            }
        }

        #endregion

        #region send DISCONNECT

        /// <summary>
        /// Disconnect from broker
        /// </summary>
        public void Disconnect()
        {
            // disconnect client
            if (this.client != null)
            {
                if( this.client.IsConnected == true)
                {
                    this.client.Disconnect();
                    while (this.client.IsConnected == true) ;
                }
            }
        }

        #endregion

        #region send PUBLISH

        /// <summary>
        /// send PUBLISH
        /// </summary>
        /// <param name="topic">Message topic</param>
        /// <param name="message">Message data </param>
        /// <param name="retain">Retain flag</param>
        /// <returns>Message Id related to PUBLISH message</returns>
        public bool SendPublish(string topic, string message, bool retain)
        {
            return this.SendPublish(topic, message, this.qosLevel, retain);
        }

        /// <summary>
        /// send PUBLISH
        /// </summary>
        /// <param name="topic">Message topic</param>
        /// <param name="message">Message data </param>
        /// <param name="qosLevel">QoS Level</param>
        /// <param name="retain">Retain flag</param>
        /// <returns>Message Id related to PUBLISH message</returns>
        private bool SendPublish(string topic, string message, QosLevel qosLevel, bool retain)
        {
            if ((this.client == null) || (this.client.IsConnected == false))
            {
                return false;
            }

            try
            {
                byte[] byteMessage = Encoding.UTF8.GetBytes(message);
                this.client.Publish(topic, byteMessage, (byte)qosLevel, retain);
                return true;
            }
            catch (MqttClientException)
            {
                return false;
            }
            catch (MqttCommunicationException)
            {
                return false;
            }
        }

        #endregion

        #region send SUBSCRIBE

        /// <summary>
        /// send SUBSCRIBE
        /// </summary>
        /// <param name="topic">Subscribe topic</param>
        /// <returns>is success</returns>
        public bool SendSubscribe(string topic)
        {
            string[] topics = { topic };
            return this.SendSubscribe(topics, this.qosLevel);
        }

        /// <summary>
        /// send SUBSCRIBE
        /// </summary>
        /// <param name="topics">Subscribe topic list</param>
        /// <returns>is success</returns>
        public bool SendSubscribe(string[] topics)
        {
            return this.SendSubscribe(topics, this.qosLevel);
        }

        /// <summary>
        /// send SUBSCRIBE
        /// </summary>
        /// <param name="topics">Subscribe topic list</param>
        /// <param name="qosLevel">Subscribe QoS level</param>
        /// <returns>is success</returns>
        public bool SendSubscribe(string[] topics, QosLevel qosLevel)
        {
            if ((this.client == null) || (this.client.IsConnected == false))
            {
                return false;
            }
            
            try
            {
                byte[] qosLevels = new byte[topics.Length];
                for (int i = 0; i < qosLevels.Length; i++)
                {
                    qosLevels[i] = (byte)qosLevel;
                }

                ushort ret = this.client.Subscribe(topics, qosLevels);

                return (ret == 0x00);
            }
            catch (MqttClientException)
            {
                return false;
            }
            catch (MqttCommunicationException)
            {
                return false;
            }
        }

        #endregion

        #region send UNSUBSCRIBE

        /// <summary>
        /// send UNSUBSCRIBE
        /// </summary>
        /// <param name="topic">UnSubscribe topic</param>
        /// <returns>is success</returns>
        public bool SendUnSubscribe(string topic)
        {
            string[] topics = { topic };
            return this.SendUnSubscribe(topics);
        }

        /// <summary>
        /// send UNSUBSCRIBE
        /// </summary>
        /// <param name="topics">UnSubscribe topic list</param>
        /// <returns>is success</returns>
        public bool SendUnSubscribe(string[] topics)
        {
            if ((this.client == null) || (this.client.IsConnected == false))
            {
                return false;
            }
            
            try
            {
                ushort ret = this.client.Unsubscribe(topics);

                return (ret == 0x00);
            }
            catch (MqttClientException)
            {
                return false;
            }
            catch (MqttCommunicationException)
            {
                return false;
            }
        }

        #endregion

    }
}
