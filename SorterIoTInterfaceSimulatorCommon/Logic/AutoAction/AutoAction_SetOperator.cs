using Glory.SorterInterface.Message;
using Glory.SorterInterface.MessageFormat.Message;
using Glory.SorterInterface.Protocol.Mqtt;
using SorterIotInterfaceSimulator.Common.Logic;
using System;
using System.Threading;
using Command = Glory.SorterInterface.MessageFormat.JsonSerializer.Message.Command;
using Event = Glory.SorterInterface.MessageFormat.JsonSerializer.Message.Event;
using Response = Glory.SorterInterface.MessageFormat.JsonSerializer.Message.Response;

namespace SorterIoTInterfaceSimulatorCommon.Logic.AutoAction
{
    /// <summary>
    /// 自動実行機能 SetOperator Command 自動送信
    /// </summary>
    class AutoAction_SetOperator : AutoAction_Base
    {
        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="autoAction"></param>
        public AutoAction_SetOperator(AutoActionAbstract autoAction)
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
            if (this.autoAction.IsEnableSetting("AutoAction_SetOperator") == false)
            {
                return;
            }

            // GetDeviceInformation Response 受信処理
            if ((messageType == MessageType.Response) && (messageName == MessageName.CreateSession))
            {
                this.RecvCreateSession(topic, messageType, messageName, message);
            }

            // ログアウト機能無効なら何もしない
            if (this.autoAction.IsEnableSetting("AutoAction_SetOperator_Logout") == false)
            {
                return;
            }

            // OperatorChanged Event 受信処理
            if ((messageType == MessageType.Event) && (messageName == MessageName.OperatorChanged))
            {
                this.RecvOperatorChanged(topic, messageType, messageName, message);
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
            //Response.GetDeviceInformation recvObj = SorterIfMessageFormat.DeSerialize<Response.GetDeviceInformation>(message);

            if (string.IsNullOrEmpty(this.autoAction.SessionID) == false)
            {
                Thread.Sleep(500);

                // SetOperator Command 生成(ログイン)
                string id = this.autoAction.GetSetting("AutoAction_SetOperator_ID");
                string password = this.autoAction.GetSetting("AutoAction_SetOperator_Password");
                Command.SetOperator sendObj = new Command.SetOperator(id, password);

                // JSON 変換
                string sendMessage = SorterIfMessageFormat.Serialize<Command.SetOperator>(sendObj);

                // コマンド送信
                this.autoAction.SendCommand(sendMessage);
            }
        }

        /// <summary>
        /// OperatorChanged Event 受信処理
        /// </summary>
        /// <param name="topic"></param>
        /// <param name="messageType"></param>
        /// <param name="messageName"></param>
        /// <param name="message"></param>
        private void RecvOperatorChanged(string topic, MessageType messageType, MessageName messageName, string message)
        {
            // デシリアライズ
            Event.OperatorChanged recvObj = SorterIfMessageFormat.DeSerialize<Event.OperatorChanged>(message);

            if (string.IsNullOrEmpty(this.autoAction.SessionID) == false)
            {
                string id = this.autoAction.GetSetting("AutoAction_SetOperator_ID");
                if (recvObj.Detail.ID == id)
                {
                    // SetOperator Command 生成(ログアウト)
                    Command.SetOperator sendObj = new Command.SetOperator(null, null);

                    // JSON 変換
                    string sendMessage = SorterIfMessageFormat.Serialize<Command.SetOperator>(sendObj);
                    sendMessage = sendMessage.Replace("\"ID\":null", "");
                    sendMessage = sendMessage.Replace("\"Password\":null", "");
                    sendMessage = sendMessage.Replace("{,}", "{}");

                    // コマンド送信
                    this.autoAction.SendCommand(sendMessage);
                }
            }
        }
    }
}
