using Glory.SorterInterface.SorterInterfaceClient.Client;
using SorterIotInterfaceSimulator.Common.Logic;

namespace SorterIotInterfaceSimulator.ViewModel
{
    /// <summary>
    /// 自動実行機能(MQTT用)
    /// </summary>
    public class AutoAction : AutoActionAbstract
    {
        /// <summary>
        /// コンストラクタ(MQTT用)
        /// </summary>
        /// <param name="client"></param>
        /// <param name="deviceName"></param>
        public AutoAction(
            ISorterIfClient client,
            string deviceName)
            : base (AutoAction.ProtocolType.MQTT, deviceName)
        {
            this.client = client;
        }

        /// <summary>
        /// Sorter IoT Interface Client
        /// </summary>
        protected ISorterIfClient client = null;

        /// <summary>
        /// メッセージ送信(MQTT用)
        /// </summary>
        /// <param name="topic"></param>
        /// <param name="message"></param>
        /// <param name="retain"></param>
        protected override void SendCommandMQTT(string topic, string message, bool retain)
        {
            this.client.SendPublish(topic, message, retain);
        }

        /// <summary>
        /// メッセージ送信(TCP用)
        /// </summary>
        /// <param name="message"></param>
        protected override void SendCommandTCP(string message)
        {
        }
    }
}
