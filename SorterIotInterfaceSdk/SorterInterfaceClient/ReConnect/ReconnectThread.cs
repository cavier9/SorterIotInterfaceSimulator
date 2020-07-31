using System;
using System.Threading;
using uPLibrary.Networking.M2Mqtt.Messages;
using Glory.SorterInterface.Protocol.Mqtt;
using System.Collections.Generic;

namespace Glory.SorterInterface.SorterInterfaceClient.ReConnect
{
    /// <summary>
    /// reconnect to broker
    /// </summary>
    internal class ReconnectThread : IReconnectThread
    {
        #region fixed parameter

        /// <summary>
        /// retry count
        /// </summary>
        private const int RETRY_COUNT_MAX = 0;

        /// <summary>
        /// retry wait time (msec)
        /// </summary>
        private const int RETRY_WAIT_TIME = 1000;

        /// <summary>
        /// disconnect wait time (msec)
        /// </summary>
        private const int DISCONNECT_WAIT_TIME = 2000;

        #endregion

        #region property

        /// <summary>
        /// is running
        /// </summary>
        public bool IsRunning
        {
            get { return this.isRunning; }
            private set { this.isRunning = value; }
        }

        #endregion

        #region parameter

        /// <summary>
        /// reconnect thread
        /// </summary>
        private Thread thread;

        /// <summary>
        /// should stop flag
        /// </summary>
        private volatile bool shouldStop;

        /// <summary>
        /// subscribe topic when connect success
        /// </summary>
        private IList<string> subscribeTopicList = new List<string>();

        /// <summary>
        /// MqttProtocol
        /// </summary>
        private readonly IMqttProtocol mqttProtocol;

        /// <summary>
        /// SorterIfProtocol
        /// </summary>
        private readonly SorterIfProtocolBase sorterIfProtocol;

        /// <summary>
        /// retry count setting
        /// </summary>
        private readonly int retryCountSetting;

        /// <summary>
        /// retry wait time (msec) setting
        /// </summary>
        private readonly int retryWaitTimeSetting;

        /// <summary>
        /// disconnect wait time (msec) setting
        /// </summary>
        private readonly int disconnectWaitTimeSetting;

        /// <summary>
        /// is running
        /// </summary>
        private bool isRunning = false;

        #endregion

        #region constructor

        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="mqttProtocol">MqttProtocol</param>
        /// <param name="sorterIfProtocol">sorterIfProtocol</param>
        public ReconnectThread(IMqttProtocol mqttProtocol, SorterIfProtocolBase sorterIfProtocol)
            : this(mqttProtocol, sorterIfProtocol, ReconnectThread.RETRY_COUNT_MAX, ReconnectThread.RETRY_WAIT_TIME, ReconnectThread.DISCONNECT_WAIT_TIME)
        {
        }

        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="mqttProtocol">MqttProtocol</param>
        /// <param name="sorterIfProtocol">sorterIfProtocol</param>
        /// <param name="retryCountSetting">retry count</param>
        /// <param name="retryWaitTimeSetting">retry wait time (msec)</param>
        /// <param name="disconnectWaitTimeSetting">disconnect wait time (msec)</param>
        public ReconnectThread(IMqttProtocol mqttProtocol, SorterIfProtocolBase sorterIfProtocol, int retryCountSetting, int retryWaitTimeSetting, int disconnectWaitTimeSetting)
        {
            this.mqttProtocol = mqttProtocol;
            this.sorterIfProtocol = sorterIfProtocol;
            this.retryCountSetting = retryCountSetting;
            this.retryWaitTimeSetting = retryWaitTimeSetting;
            this.disconnectWaitTimeSetting = disconnectWaitTimeSetting;

            this.thread = new Thread(this.Run);
        }

        #endregion

        #region IDisposable interface implement

        /// <summary>
        /// Dispose
        /// </summary>
        public void Dispose()
        {
            this.RequestStop();
            this.thread = null;
        }

        #endregion

        #region run

        /// <summary>
        /// thread process
        /// </summary>
        private void Run()
        {
            this.IsRunning = true;
            int retryCount = this.retryCountSetting;

            while (this.shouldStop == false)
            {
                if (this.mqttProtocol.IsConnected() == true)
                {
                    // connect success 
                    retryCount = this.retryCountSetting;
                    Thread.Sleep(this.disconnectWaitTimeSetting);
                    continue;
                }

                if ((0 < this.retryCountSetting) && (retryCount <= 0))
                {
                    // retry count up
                    break;
                }

                // reconnect
                try
                {
                    bool ret = this.mqttProtocol.Reconnect();
                    if (ret == true)
                    {
                        foreach (string topic in this.subscribeTopicList)
                        {
                            this.sorterIfProtocol.SendSubscribe(topic);
                        }

                        if (this.ReconnectedProcessEvent != null)
                        {
                            this.ReconnectedProcessEvent();
                        }
                    }
                }
                catch
                {
                }

                retryCount--;
                Thread.Sleep(this.retryWaitTimeSetting);
            }

            this.IsRunning = false;
            this.shouldStop = false;
        }

        #endregion

        #region request start

        /// <summary>
        /// request start
        /// </summary>
        /// <param name="subscribeTopicList">subscribe topic when connect success</param>
        public void RequestStart(IList<string> subscribeTopicList)
        {
            if (this.IsRunning == false)
            {
                this.subscribeTopicList.Clear();
                foreach (string topic in subscribeTopicList)
                {
                    this.subscribeTopicList.Add(topic);
                }

                this.thread.Start();
            }
        }

        #endregion

        #region request stop

        /// <summary>
        /// request stop
        /// </summary>
        public void RequestStop()
        {
            while (this.IsRunning == true)
            {
                this.shouldStop = true;
                this.thread.Join();
            }
        }

        #endregion

        #region reconnected process

        /// <summary>
        /// reconnected process delegate
        /// </summary>
        public delegate void ReconnectedProcessDelegate();

        /// <summary>
        /// reconnected process
        /// </summary>
        public event ReconnectedProcessDelegate ReconnectedProcessEvent;

        #endregion
    }
}
