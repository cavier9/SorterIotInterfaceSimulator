using Glory.SorterInterface.Message;
using Glory.SorterInterface.MessageFormat.Message;
using Glory.SorterInterface.Protocol.Mqtt;
using SorterIotInterfaceSimulator.Common.Logic;
using System;
using Command = Glory.SorterInterface.MessageFormat.JsonSerializer.Message.Command;
using Event = Glory.SorterInterface.MessageFormat.JsonSerializer.Message.Event;
using Response = Glory.SorterInterface.MessageFormat.JsonSerializer.Message.Response;

namespace SorterIoTInterfaceSimulatorCommon.Logic.AutoAction
{
    /// <summary>
    /// 自動実行機能 CreateSession Command 自動送信
    /// </summary>
    class AutoAction_CreateSession : AutoAction_Base
    {
        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="autoAction"></param>
        public AutoAction_CreateSession(AutoActionAbstract autoAction)
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
            if (this.autoAction.IsEnableSetting("AutoAction_CreateSession") == false)
            {
                return;
            }

            // Connected Event 受信処理
            if ((messageType == MessageType.Event) && (messageName == MessageName.Connected))
            {
                this.RecvConnected(topic, messageType, messageName, message);
            }
        }

        /// <summary>
        /// Connected Event 受信処理
        /// </summary>
        /// <param name="topic"></param>
        /// <param name="messageType"></param>
        /// <param name="messageName"></param>
        /// <param name="message"></param>
        private void RecvConnected(string topic, MessageType messageType, MessageName messageName, string message)
        {
            // デシリアライズ
            Event.Connected recvObj = SorterIfMessageFormat.DeSerialize<Event.Connected>(message);

            if (string.IsNullOrEmpty(this.autoAction.SessionID) == true)
            {
                // CreateSession Command 生成
                string systemName = this.autoAction.GetSetting("AutoAction_CreateSession_SystemName");
                string systemVersion = this.autoAction.GetSetting("AutoAction_CreateSession_SystemVersion");
                Command.CreateSession sendObj = new Command.CreateSession(systemName, systemVersion);

                // JSON 変換
                string sendMessage = SorterIfMessageFormat.Serialize<Command.CreateSession>(sendObj);

                string sendTopic = Topic.GetCommandTopic(this.autoAction.DeviceName);

                // コマンド送信
                this.autoAction.SendCommand(sendTopic, sendMessage);
            }
        }
    }
}
