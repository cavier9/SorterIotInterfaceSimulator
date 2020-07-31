using System.Collections.Generic;
using Glory.SorterInterface.Message;
using Glory.SorterInterface.MessageDetail;
using Glory.SorterInterface.MessageFormat.Message;
using Glory.SorterInterface.SorterInterfaceClient.Client;
using SorterIoTInterfaceSimulatorCommon.Logic.AutoAction;
using Command = Glory.SorterInterface.MessageFormat.JsonSerializer.Message.Command;
using Event = Glory.SorterInterface.MessageFormat.JsonSerializer.Message.Event;
using Response = Glory.SorterInterface.MessageFormat.JsonSerializer.Message.Response;
using System.Xml;

namespace SorterIotInterfaceSimulator.Common.Logic
{
    /// <summary>
    /// 自動実行機能
    /// </summary>
    public abstract class AutoActionAbstract
    {
        #region ProtocolType

        /// <summary>
        /// プロトコル種別
        /// </summary>
        public enum ProtocolType
        {
            /// <summary>
            /// MQTT
            /// </summary>
            MQTT,

            /// <summary>
            /// TCP
            /// </summary>
            TCP
        }

        #endregion

        #region パラメータ

        /// <summary>
        /// MQTT用フラグ
        /// </summary>
        protected bool isMqtt = false;

        /// <summary>
        /// TCP用フラグ
        /// </summary>
        protected bool isTcp = false;

        /// <summary>
        /// デバイス名(MQTT用)
        /// </summary>
        public string DeviceName { get; set; }

        /// <summary>
        /// 設定リスト
        /// </summary>
        public Dictionary<string, string> settingList = new Dictionary<string, string>();

        #endregion

        #region コンストラクタ

        /// <summary>
        /// コンストラクタ
        /// </summary>
        private AutoActionAbstract()
        {
            // 設定値読込
            XmlReader xmlReader = XmlReader.Create("AutoActionSetting.xml");
            while (xmlReader.Read())
            {
                if (xmlReader.NodeType == XmlNodeType.Element)
                {
                    settingList.Add(xmlReader.LocalName, xmlReader.ReadString());
                }
            }
        }

        /// <summary>
        /// コンストラクタ(MQTT用)
        /// </summary>
        /// <param name="protocolType"></param>
        /// <param name="deviceName"></param>
        protected AutoActionAbstract(
            ProtocolType protocolType,
            string deviceName)
            : this()
        {
            this.DeviceName = deviceName;

            this.isMqtt = (protocolType == ProtocolType.MQTT);
            this.isTcp = (protocolType == ProtocolType.TCP);
        }

        /// <summary>
        /// コンストラクタ(TCP用)
        /// </summary>
        /// <param name="protocolType"></param>
        protected AutoActionAbstract(ProtocolType protocolType)
            : this()
        {
            this.isMqtt = (protocolType == ProtocolType.MQTT);
            this.isTcp = (protocolType == ProtocolType.TCP);
        }

        #endregion

        #region 有効／無効設定取得

        /// <summary>
        /// 有効／無効設定取得
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public bool IsEnableSetting(string key)
        {
            string val = null;
            if (this.settingList.TryGetValue(key, out val))
            {
                if (val.ToLower() == "true")
                {
                    return true;
                }
            }

            return false;
        }

        #endregion

        #region 設定取得

        /// <summary>
        /// 設定取得
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public string GetSetting(string key)
        {
            string val = null;
            if (this.settingList.TryGetValue(key, out val))
            {
                return val;
            }

            return string.Empty;
        }

        #endregion

        #region メッセージ受信時処理

        /// <summary>
        /// メッセージ受信時処理
        /// </summary>
        /// <param name="topic"></param>
        /// <param name="messageType"></param>
        /// <param name="messageName"></param>
        /// <param name="message"></param>
        public void ReceiveProccess(string topic, MessageType messageType, MessageName messageName, string message)
        {
            // 自動実行機能 無効なら何もしない
            if (this.IsEnableSetting("AutoAction") == false)
            {
                return;
            }

            // 自動実行機能 セッション管理
            this.SessionManage(topic, messageType, messageName, message);

            // 自動実行機能 CreateSession Command 自動送信
            using (AutoAction_CreateSession obj = new AutoAction_CreateSession(this))
            {
                obj.AutoAction(topic, messageType, messageName, message);
            }

            // 自動実行機能 GetDeviceInformation Command 自動送信
            using (AutoAction_GetDeviceInformation obj = new AutoAction_GetDeviceInformation(this))
            {
                obj.AutoAction(topic, messageType, messageName, message);
            }

            // 自動実行機能 SetOperator Command 自動送信
            using (AutoAction_SetOperator obj = new AutoAction_SetOperator(this))
            {
                obj.AutoAction(topic, messageType, messageName, message);
            }

        }

        #endregion メッセージ受信時処理

        #region メッセージ送信処理

        /// <summary>
        /// コマンド送信
        /// </summary>
        /// <param name="message"></param>
        public void SendCommand(string message)
        {
            this.SendCommand(this.CommandTopic, message);
        }

        /// <summary>
        /// コマンド送信
        /// </summary>
        /// <param name="message"></param>
        public void SendCommand(string topic, string message)
        {
            if (isMqtt)
            {
                // メッセージ送信(MQTT用)
                this.SendCommandMQTT(topic, message, false);
            }
            if (isTcp)
            {
                // メッセージ送信(TCP用)
                this.SendCommandTCP(message);
            }
        }

        /// <summary>
        /// メッセージ送信(MQTT用)
        /// </summary>
        /// <param name="topic"></param>
        /// <param name="message"></param>
        /// <param name="retain"></param>
        protected abstract void SendCommandMQTT(string topic, string message, bool retain);

        /// <summary>
        /// メッセージ送信(TCP用)
        /// </summary>
        /// <param name="message"></param>
        protected abstract void SendCommandTCP(string message);

        #endregion メッセージ送信処理

        #region 自動実行機能 セッション管理

        /// <summary>
        /// セッションID
        /// </summary>
        public string SessionID = string.Empty;

        /// <summary>
        /// コマンド Topic
        /// </summary>
        private string CommandTopic = string.Empty;

        /// <summary>
        /// セッション管理
        /// </summary>
        /// <param name="topic"></param>
        /// <param name="messageType"></param>
        /// <param name="messageName"></param>
        /// <param name="message"></param>
        protected void SessionManage(string topic, MessageType messageType, MessageName messageName, string message)
        {
            if (messageType == MessageType.Event)
            {
                if (messageName == MessageName.Disconnected)
                {
                    this.RecvDisconnected(topic, messageType, messageName, message);
                }
                else if (messageName == MessageName.SessionClosed)
                {
                    this.RecvSessionClosed(topic, messageType, messageName, message);
                }
            }
            else if (messageType == MessageType.Response)
            {
                if (messageName == MessageName.CreateSession)
                {
                    this.RecvCreateSession(topic, messageType, messageName, message);
                }
                else if (messageName == MessageName.CloseSession)
                {
                    this.RecvCloseSession(topic, messageType, messageName, message);
                }
            }
        }

        /// <summary>
        /// CreateSession Response 受信処理
        /// </summary>
        /// <param name="topic"></param>
        /// <param name="messageType"></param>
        /// <param name="messageName"></param>
        /// <param name="message"></param>
        protected void RecvCreateSession(string topic, MessageType messageType, MessageName messageName, string message)
        {
            // デシリアライズ
            Response.CreateSession recvObj = SorterIfMessageFormat.DeSerialize<Response.CreateSession>(message);

            if (recvObj.Detail.Result == ResultCode.SUCCESS)
            {
                this.SessionID = recvObj.Detail.SessionID;
                this.CommandTopic = recvObj.Detail.CommandTopic;
            }
        }

        /// <summary>
        /// CloseSession Response 受信処理
        /// </summary>
        /// <param name="topic"></param>
        /// <param name="messageType"></param>
        /// <param name="messageName"></param>
        /// <param name="message"></param>
        protected void RecvCloseSession(string topic, MessageType messageType, MessageName messageName, string message)
        {
            // デシリアライズ
            Response.CloseSession recvObj = SorterIfMessageFormat.DeSerialize<Response.CloseSession>(message);

            if ((recvObj.Detail.Result == ResultCode.SUCCESS) &&
                ((this.SessionID != null) && (this.SessionID == recvObj.Detail.SessionID)))
            {
                this.SessionID = string.Empty;
                this.CommandTopic = string.Empty;
            }
        }

        /// <summary>
        /// Disconnected Response 受信処理
        /// </summary>
        /// <param name="topic"></param>
        /// <param name="messageType"></param>
        /// <param name="messageName"></param>
        /// <param name="message"></param>
        protected void RecvDisconnected(string topic, MessageType messageType, MessageName messageName, string message)
        {
            // デシリアライズ
            Event.Disconnected recvObj = SorterIfMessageFormat.DeSerialize<Event.Disconnected>(message);

            if (recvObj.Detail.DeviceName == this.DeviceName)
            {
                this.SessionID = string.Empty;
                this.CommandTopic = string.Empty;
            }
        }

        /// <summary>
        /// SessionClosed Event 受信処理
        /// </summary>
        /// <param name="topic"></param>
        /// <param name="messageType"></param>
        /// <param name="messageName"></param>
        /// <param name="message"></param>
        protected void RecvSessionClosed(string topic, MessageType messageType, MessageName messageName, string message)
        {
            // デシリアライズ
            Event.SessionClosed recvObj = SorterIfMessageFormat.DeSerialize<Event.SessionClosed>(message);
            this.SessionID = string.Empty;
            this.CommandTopic = string.Empty;
        }

        #endregion 自動実行機能 セッション管理
    }
}
