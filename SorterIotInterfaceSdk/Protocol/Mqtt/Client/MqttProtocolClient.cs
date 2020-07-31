using System;
#if M2MQTT_V3
using System.Net.Security;
using System.Security.Authentication;
#endif
using System.Security.Cryptography.X509Certificates;
using uPLibrary.Networking.M2Mqtt;
using uPLibrary.Networking.M2Mqtt.Exceptions;
using Glory.SorterInterface.Exceptions;

namespace Glory.SorterInterface.Protocol.Mqtt
{
    /// <summary>
    /// MQTT protocol client
    /// </summary>
    internal class MqttProtocolClient : IMqttProtocolClient
    {
        #region fixed parameter

        #endregion

        #region property

        /// <summary>
        /// Connection status between client and broker
        /// </summary>
        public bool IsConnected
        {
            get { return this.client.IsConnected; }
        }

        #endregion

        #region parameter

        /// <summary>
        /// M2Mqtt client
        /// </summary>
        private MqttClient client = null;

        /// <summary>
        /// event handler for PUBLISH message received
        /// </summary>
        public event MqttClient.MqttMsgPublishEventHandler PublishReceived;

        /// <summary>
        /// is set event handler for PUBLISH message received
        /// </summary>
        private bool isSetPublishReceived = false;

        /// <summary>
        /// use server certificate
        /// </summary>
        private bool isUseServerCertificate = false;
        
#if M2MQTT_V3
        /// <summary>
        /// result that verified the remote Secure Sockets Layer (SSL) certificate
        /// </summary>
        private SslPolicyErrors resultVerified = SslPolicyErrors.None;
#endif

        #endregion

        #region constructor

        /// <summary>
        /// constructor (plain text)
        /// </summary>
        /// <param name="hostName">HostName</param>
        public MqttProtocolClient(string hostName)
        {
            this.client = new MqttClient(hostName);
        }

#if M2MQTT_V3
        /// <summary>
        /// Constructor (SSL/TLS. Server certificate)
        /// </summary>
        /// <param name="brokerHostName">Broker Host Name or IP Address</param>
        /// <param name="brokerPort">Broker port</param>
        /// <param name="secure">Using secure connection</param>
        /// <param name="certificate">Using server certificate</param>
        /// <param name="sslProtocol">SSL/TLS protocol version</param>
        public MqttProtocolClient(
            string brokerHostName,
            int brokerPort,
            bool secure,
            bool certificate,
            MqttSslProtocols sslProtocol)
        {
            this.isUseServerCertificate = certificate;

            this.client = new MqttClient(
                brokerHostName,
                brokerPort,
                secure,
                // CA certificate for secure connection
                null,
                // Client certificate
                null,
                sslProtocol,
                this.OnRemoteCertificateValidationCallback);
        }

#else
        /// <summary>
        /// Constructor (Secure. Server certificate)
        /// </summary>
        /// <param name="brokerHostName">Broker Host Name or IP Address</param>
        /// <param name="brokerPort">Broker port</param>
        /// <param name="secure">Using secure connection</param>
        /// <param name="certificate">Using server certificate</param>
        public MqttProtocolClient(
            string brokerHostName,
            int brokerPort,
            bool secure,
            bool certificate)
        {
            this.isUseServerCertificate = certificate;

            this.client = new MqttClient(
                brokerHostName,
                brokerPort,
                secure,
                // CA certificate for secure connection
                null);
        }
#endif

        #endregion

        #region IDisposable interface

        /// <summary>
        /// Dispose
        /// </summary>
        public void Dispose()
        {
            if (this.client.IsConnected == true)
            {
                this.Disconnect();
            }
            this.client = null;
        }

        #endregion

        #region RemoteCertificateValidationCallback

#if M2MQTT_V3
        /// <summary>
        /// Verifies the remote Secure Sockets Layer (SSL) certificate used for authentication.
        /// </summary>
        /// <param name="sender">An object that contains state information for this validation.</param>
        /// <param name="certificate">The certificate used to authenticate the remote party.</param>
        /// <param name="chain">The chain of certificate authorities associated with the remote certificate.</param>
        /// <param name="sslPolicyErrors">One or more errors associated with the remote certificate.</param>
        /// <returns>A System.Boolean value that determines whether the specified certificate is accepted for authentication.</returns>
        private bool OnRemoteCertificateValidationCallback(
            Object sender,
            X509Certificate certificate,
            X509Chain chain,
            SslPolicyErrors sslPolicyErrors)
        {
            if (this.isUseServerCertificate == false)
            {
                // not use server certificate
                this.resultVerified = SslPolicyErrors.None;
                return true;
            }

            if (sslPolicyErrors == SslPolicyErrors.None)
            {
                // no error
                this.resultVerified = SslPolicyErrors.None;
                return true;
            }
            else
            {
                bool ret = true;

                if ((sslPolicyErrors & SslPolicyErrors.RemoteCertificateNotAvailable) != 0)
                {
                    // error NotAvailable
                    ret &= false;
                }
                if ((sslPolicyErrors & SslPolicyErrors.RemoteCertificateNameMismatch) != 0)
                {
                    // error NameMismatch
                    ret &= false;
                }
                if ((sslPolicyErrors & SslPolicyErrors.RemoteCertificateChainErrors) != 0)
                {
                    // error ChainErrors
                    ret &= false;
                }

                this.resultVerified = sslPolicyErrors;
                return ret;
            }
        }
#endif

        #endregion

        #region Connect


#if M2MQTT_V3
        /// <summary>
        /// Connect to broker
        /// </summary>
        /// <param name="clientId">Client identifier</param>
        /// <param name="username">Username</param>
        /// <param name="password">Password</param>
        /// <param name="cleanSession">Clean sessione flag</param>
        /// <param name="keepAlivePeriod">Keep alive period</param>
        /// <returns>Return code of CONNACK message from broker</returns>
        public byte Connect(
            string clientId,
            string username,
            string password,
            bool cleanSession,
            ushort keepAlivePeriod)
        {
            try
            {
                this.resultVerified = SslPolicyErrors.None;
                return this.client.Connect(
                    clientId,
                    username,
                    password,
                    cleanSession,
                    keepAlivePeriod);
            }
            catch (MqttConnectionException ex)
            {
                if (this.resultVerified != SslPolicyErrors.None)
                {
                    string message = this.GetAuthenticationExceptionMessage(this.resultVerified);
                    throw new AuthenticationException(message, ex);
                }
                else
                {
                    throw;
                }
            }
        }

        /// <summary>
        /// Connect to broker
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
        public byte Connect(
            string clientId,
            string username,
            string password,
            bool willRetain,
            byte willQosLevel,
            bool willFlag,
            string willTopic,
            string willMessage,
            bool cleanSession,
            ushort keepAlivePeriod)
        {
            try
            {
                this.resultVerified = SslPolicyErrors.None;
                return this.client.Connect(
                    clientId,
                    username,
                    password,
                    willRetain,
                    willQosLevel,
                    willFlag,
                    willTopic,
                    willMessage,
                    cleanSession,
                    keepAlivePeriod);
            }
            catch (MqttConnectionException ex)
            {
                if (this.resultVerified != SslPolicyErrors.None)
                {
                    string message = this.GetAuthenticationExceptionMessage(this.resultVerified);
                    throw new AuthenticationException(message, ex);
                }
                else
                {
                    throw;
                }
            }
        }

        /// <summary>
        /// get AuthenticationException message
        /// </summary>
        /// <param name="sslPolicyErrors">One or more errors associated with the remote certificate.</param>
        /// <returns>AuthenticationException message</returns>
        private string GetAuthenticationExceptionMessage(SslPolicyErrors src)
        {
            string str = string.Empty;
            if ((src & SslPolicyErrors.RemoteCertificateNotAvailable) != 0)
            {
                // error NotAvailable
                str = SslPolicyErrors.RemoteCertificateNotAvailable.ToString();
            }
            else if ((src & SslPolicyErrors.RemoteCertificateNameMismatch) != 0)
            {
                // error NameMismatch
                str = SslPolicyErrors.RemoteCertificateNameMismatch.ToString();
            }
            else if ((src & SslPolicyErrors.RemoteCertificateChainErrors) != 0)
            {
                // error ChainErrors
                str = SslPolicyErrors.RemoteCertificateChainErrors.ToString();
            }
            else
            {
                return str;
            }
            return string.Format("SslPolicyErrors.{0} (0x{1:X02})", str, (int)this.resultVerified);
        }
#else
        /// <summary>
        /// Connect to broker
        /// </summary>
        /// <param name="clientId">Client identifier</param>
        /// <param name="username">Username</param>
        /// <param name="password">Password</param>
        /// <param name="cleanSession">Clean sessione flag</param>
        /// <param name="keepAlivePeriod">Keep alive period</param>
        /// <returns>Return code of CONNACK message from broker</returns>
        public byte Connect(string clientId,
            string username,
            string password,
            bool cleanSession,
            ushort keepAlivePeriod)
        {
            return this.client.Connect(
                clientId,
                username,
                password,
                cleanSession,
                keepAlivePeriod);
        }

        /// <summary>
        /// Connect to broker
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
        public byte Connect(string clientId,
            string username,
            string password,
            bool willRetain,
            byte willQosLevel,
            bool willFlag,
            string willTopic,
            string willMessage,
            bool cleanSession,
            ushort keepAlivePeriod)
        {
            return this.client.Connect(
                clientId,
                username,
                password,
                willRetain,
                willQosLevel,
                willFlag,
                willTopic,
                willMessage,
                cleanSession,
                keepAlivePeriod);
        }
#endif

        #endregion

        #region Disconnect

        /// <summary>
        /// Disconnect from broker
        /// </summary>
        public void Disconnect()
        {
            this.client.Disconnect();
        }

        #endregion

        #region Publish

        /// <summary>
        /// Publish a message asynchronously
        /// </summary>
        /// <param name="topic">Message topic</param>
        /// <param name="message">Message data (payload)</param>
        /// <param name="qosLevel">QoS Level</param>
        /// <param name="retain">Retain flag</param>
        /// <returns>Message Id related to PUBLISH message</returns>
        public ushort Publish(string topic, byte[] message, byte qosLevel, bool retain)
        {
            return this.client.Publish(topic, message, qosLevel, retain);
        }

        #endregion

        #region Subscribe

        /// <summary>
        /// Subscribe for message topics
        /// </summary>
        /// <param name="topics">List of topics to subscribe</param>
        /// <param name="qosLevels">QOS levels related to topics</param>
        /// <returns>Message Id related to SUBSCRIBE message</returns>
        public ushort Subscribe(string[] topics, byte[] qosLevels)
        {
            if (this.isSetPublishReceived == false)
            {
                this.client.MqttMsgPublishReceived += PublishReceived;
                this.isSetPublishReceived = true;
            }
            return this.client.Subscribe(topics, qosLevels);
        }

        #endregion

        #region Unsubscribe

        /// <summary>
        /// Unsubscribe for message topics
        /// </summary>
        /// <param name="topics">List of topics to unsubscribe</param>
        /// <returns>Message Id in UNSUBACK message from broker</returns>
        public ushort Unsubscribe(string[] topics)
        {
            return this.client.Unsubscribe(topics);
        }

        #endregion

    }
}
