using Glory.SorterInterface.Message;
using Glory.SorterInterface.MessageFormat.Message;
using SorterIotInterfaceSimulator.Common.Logic;
using System;
using Command = Glory.SorterInterface.MessageFormat.JsonSerializer.Message.Command;
using Event = Glory.SorterInterface.MessageFormat.JsonSerializer.Message.Event;
using Response = Glory.SorterInterface.MessageFormat.JsonSerializer.Message.Response;

namespace SorterIoTInterfaceSimulatorCommon.Logic.AutoAction
{
    /// <summary>
    /// 自動実行機能 GetDeviceInformation Command 自動送信
    /// </summary>
    class AutoAction_GetDeviceInformation : AutoAction_Base
    {
        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="autoAction"></param>
        public AutoAction_GetDeviceInformation(AutoActionAbstract autoAction)
            : base(autoAction)
        {
        }
        
        /// <summary>
        /// 自動実行機能
        /// </summary>
        /// <param name="topic"></param>
        /// <param name="messageType"></param>
        /// <param name="messageName"></param>
        /// <param name="message"></param>
        public override void AutoAction(string topic, MessageType messageType, MessageName messageName, string message)
        {
            // 機能無効なら何もしない
            if (this.autoAction.IsEnableSetting("AutoAction_GetDeviceInformation") == false)
            {
                return;
            }

            // CreateSession Response 受信処理
            if ((messageType == MessageType.Response) && (messageName == MessageName.CreateSession))
            {
                this.RecvCreateSession(topic, messageType, messageName, message);
            }
        }

        /// <summary>
        /// CreateSession Response 受信処理
        /// </summary>
        /// <param name="topic"></param>
        /// <param name="messageType"></param>
        /// <param name="messageName"></param>
        /// <param name="message"></param>
        private void RecvCreateSession(string topic, MessageType messageType, MessageName messageName, string message)
        {
            // デシリアライズ
            //Response.CreateSession recvObj = SorterIfMessageFormat.DeSerialize<Response.CreateSession>(message);

            if (string.IsNullOrEmpty(this.autoAction.SessionID) == false)
            {
                // GetDeviceInformation Command 生成
                Command.GetDeviceInformation sendObj = new Command.GetDeviceInformation();

                // JSON 変換
                string sendMessage = SorterIfMessageFormat.Serialize<Command.GetDeviceInformation>(sendObj);

                // コマンド送信
                this.autoAction.SendCommand(sendMessage);
            }
        }
    }
}
