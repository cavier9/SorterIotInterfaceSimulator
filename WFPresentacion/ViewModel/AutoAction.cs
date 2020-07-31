using SorterIotInterfaceSimulator.Common.Logic;
using WFPresentacion.Tcp;

namespace WFPresentacion.ViewModel
{
    // 自動実行機能(TCP用)
    public class AutoAction : AutoActionAbstract
    {
        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="client"></param>
        public AutoAction(TcpClientProtocol client)
            : base(AutoAction.ProtocolType.TCP)
        {
            this.tcpClient = client;
        }

        /// <summary>
        /// TCP client
        /// </summary>
        TcpClientProtocol tcpClient = null;

        /// <summary>
        /// メッセージ送信(MQTT用)
        /// </summary>
        /// <param name="topic"></param>
        /// <param name="message"></param>
        /// <param name="retain"></param>
        protected override void SendCommandMQTT(string topic, string message, bool retain)
        {
        }

        /// <summary>
        /// メッセージ送信(TCP用)
        /// </summary>
        /// <param name="message"></param>
        protected override void SendCommandTCP(string message)
        {
            this.tcpClient.Send(message);
        }
    }
}
