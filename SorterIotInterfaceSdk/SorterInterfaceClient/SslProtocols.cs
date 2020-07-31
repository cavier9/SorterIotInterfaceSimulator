using System;
using uPLibrary.Networking.M2Mqtt;

namespace Glory.SorterInterface.SorterInterfaceClient
{
    /// <summary>
    /// Supported SSL/TLS protocol versions
    /// </summary>
    public enum SslProtocols
    {
#if M2MQTT_V3
        /// <summary>
        /// SSL protocol no use
        /// </summary>
        None = MqttSslProtocols.None,

        /// <summary>
        /// SSL 3.0
        /// </summary>
        SSLv3 = MqttSslProtocols.SSLv3,

        /// <summary>
        /// TLS 1.0
        /// </summary>
        TLSv1_0 = MqttSslProtocols.TLSv1_0,

        /// <summary>
        /// TLS 1.1
        /// </summary>
        TLSv1_1 = MqttSslProtocols.TLSv1_1,

        /// <summary>
        /// TLS 1.2
        /// </summary>
        TLSv1_2 = MqttSslProtocols.TLSv1_2
#else
#endif
    }
}
