using Glory.SorterInterface.Message;
using Glory.SorterInterface.MessageFormat.Message;
using SorterIotInterfaceSimulator.Common.Logic;
using System;
using System.Xml;
using Command = Glory.SorterInterface.MessageFormat.JsonSerializer.Message.Command;
using Event = Glory.SorterInterface.MessageFormat.JsonSerializer.Message.Event;
using Response = Glory.SorterInterface.MessageFormat.JsonSerializer.Message.Response;

namespace SorterIoTInterfaceSimulatorCommon.Logic.AutoAction
{
    /// <summary>
    /// 自動実行機能 ベースクラス
    /// </summary>
    class AutoAction_Base :IDisposable
    {
        /// <summary>
        /// 自動実行機能
        /// </summary>
        protected AutoActionAbstract autoAction;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="autoAction"></param>
        public AutoAction_Base(AutoActionAbstract autoAction)
        {
            this.autoAction = autoAction;
        }

        /// <summary>
        /// Dispose処理
        /// </summary>
        public virtual void Dispose()
        {
            this.autoAction = null;
        }

        /// <summary>
        /// 自動実行機能
        /// </summary>
        /// <param name="topic"></param>
        /// <param name="messageType"></param>
        /// <param name="messageName"></param>
        /// <param name="message"></param>
        public virtual void AutoAction(string topic, MessageType messageType, MessageName messageName, string message) { }

    }
}
