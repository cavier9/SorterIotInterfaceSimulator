using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SorterIotInterfaceSimulator.Tcp
{
    /// <summary>
    /// シミュレータ用 TCPクライアント機能
    /// </summary>
    public class TcpClientProtocol : IDisposable
    {
        //private const int port = 11000;

        // ManualResetEvent instances signal completion.
        private ManualResetEvent connectDone = new ManualResetEvent(false);
        private ManualResetEvent sendDone = new ManualResetEvent(false);
        private ManualResetEvent receiveDone = new ManualResetEvent(false);

        #region constractor

        /// <summary>
        /// constractor
        /// </summary>
        /// <param name="host"></param>
        /// <param name="portNo"></param>
        public TcpClientProtocol(string host, int portNo)
        {
            this.StartClient(host, portNo);
        }

        #endregion

        #region dispose

        /// <summary>
        /// Dispose
        /// </summary>
        public void Dispose()
        {
            if( this.client.Connected == true)
            {
                this.client.Disconnect(false);
            }
            connectDone = null;
            sendDone = null;
            receiveDone = null;
        }

        #endregion

        #region start client

        Socket client = null;

        /// <summary>
        /// start client
        /// </summary>
        /// <param name="host"></param>
        /// <param name="portNo"></param>
        private void StartClient(string host, int portNo)
        {
            // Connect to a remote device.
            try
            {
                // Establish the remote endpoint for the socket.
                // The name of the 
                // remote device is "host.contoso.com".
                IPHostEntry ipHostInfo = Dns.Resolve(host);
                IPAddress ipAddress = ipHostInfo.AddressList[0];
                IPEndPoint remoteEP = new IPEndPoint(ipAddress, portNo);

                // Create a TCP/IP socket.
                this.client = new Socket(AddressFamily.InterNetwork,
                    SocketType.Stream, ProtocolType.Tcp);

                // Connect to the remote endpoint.
                this.client.BeginConnect(remoteEP,
                    new AsyncCallback(ConnectCallback), this.client);

                Thread.Sleep(1000);
                if (this.client.Connected == false)
                {
                    throw new Exception("Connect to TCP Server fail.");
                }
                connectDone.WaitOne();

                // receive start
                this.Receive();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                throw;
            }
        }

        /// <summary>
        /// Connect Callback
        /// </summary>
        /// <param name="host"></param>
        /// <param name="portNo"></param>
        private void ConnectCallback(IAsyncResult ar)
        {
            try
            {
                // Retrieve the socket from the state object.
                Socket client = (Socket)ar.AsyncState;

                // Complete the connection.
                client.EndConnect(ar);

                Console.WriteLine("Socket connected to {0}",
                    client.RemoteEndPoint.ToString());

                // Signal that the connection has been made.
                connectDone.Set();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        #endregion

        #region receive

        ///// <summary>
        ///// State object for receiving data from remote device.
        ///// </summary>
        //public class StateObject
        //{
        //    // Client socket.
        //    public Socket workSocket = null;
        //    // Size of receive buffer.
        //    public const int BufferSize = 256;
        //    // Receive buffer.
        //    public byte[] buffer = new byte[BufferSize];
        //    // Received data string.
        //    public StringBuilder sb = new StringBuilder();
        //}


        /// <summary>
        /// Receive
        /// </summary>
        public void Receive()
        {
            //while(true)
            {
                this.Receive(this.client);
            }
        }

        /// <summary>
        /// Receive
        /// </summary>
        /// <param name="client">this client</param>
        private void Receive(Socket client)
        {
            try
            {
                // Create the state object.
                StateObject state = new StateObject();
                state.WorkSocket = client;

                // Begin receiving the data from the remote device.
                client.BeginReceive(state.bytBuffer, 0, StateObject.BufferSize, 0,
                    new AsyncCallback(readCallback), state);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        /// <summary>
        /// lock object
        /// </summary>
        private object lockobj = new Object();

        // The response from the remote device.
        private String response = String.Empty;

        ///// <summary>
        ///// ReceiveCallback
        ///// </summary>
        ///// <param name="ar"></param>
        //private void ReceiveCallback(IAsyncResult ar)
        //{
        //}

        #region USF-200 のTcPサーバーから一部移植

        /// <summary>
        /// データ受信処理
        /// </summary>
        /// <param name="ar">非同期操作のステータス情報</param>
        /// <remarks></remarks>
        private void readCallback(IAsyncResult ar)
        {
            //Log.Write(Log.LogType.InternalInformation, Log.LogLevel.Debug, "readCallback", ":Start ID:", Thread.CurrentThread.ManagedThreadId);
            try
            {
                ////継続受信待ちタイマーを一時停止
                //if (waitContinueRecvTimer != null)
                //{
                //    waitContinueRecvTimer.Change(Timeout.Infinite, Timeout.Infinite);
                //}

                ////無通信監視タイマーを一時停止
                //if (nonRecvTimer != null)
                //{
                //    nonRecvTimer.Change(Timeout.Infinite, Timeout.Infinite);
                //}

                //読み込んだデータバッファです。
                byte[] CommandData = new byte[DEF_BUF_SIZE + 1];

                // オブジェクトを受け取ります
                StateObject state = (StateObject)ar.AsyncState;
                Socket handler = state.WorkSocket;

                ////継続受信待ちタイマーが発生したため、データをクリア
                //if (fClearData == true)
                //{
                //    state = null;
                //    state = new StateObject();
                //    state.WorkSocket = handler;
                //}

                //strIpAddress = IPAddress.Parse(((IPEndPoint)handler.RemoteEndPoint).Address.ToString()).ToString();

                // ReadSizedata from the client socket.
                int ReadSize = 0;
                if (handler != null && GetConnectedState(handler) == true)
                {
                    ReadSize = handler.EndReceive(ar);
                    //Log.Write(Log.LogType.InternalInformation, Log.LogLevel.Debug, "readCallback", ":Size:",ReadSize);
                }

                StateObject nextState = null;
                // データ受信処理
                this.readCallback(state, ReadSize, out nextState);

                // 処理されていないデータが残っているなら、データ受信処理を繰り返す
                while (nextState != null)
                {
                    StateObject nowState = nextState;
                    nextState = null;
                    this.readCallback(nowState, nowState.bytBuffer.Length, out nextState);
                }
            }
            catch (System.Exception ex)
            {
                TcpLog(string.Format("Exception readCallback(), Message={0},\n stackTrace={1}",
                    ex.Message, ex.StackTrace));

                //ExceptionLog(string.Format("Exception {0}, {1}", "Receive", ex.Message), ex);
                //DebugLog("readCallback Exception", ex);

                ////再接続し、再度受信待ちに
                //if (connectedSocket != null)
                //{
                //    try
                //    {
                //        connectedSocket.Shutdown(SocketShutdown.Both);
                //    }
                //    catch (SocketException err)
                //    {
                //        ExceptionLog(string.Format("Exception {0}, {1}", "Receive", err.Message), err);
                //        DebugLog("readCallback Shutdown SocketException", err);
                //    }
                //    catch (ObjectDisposedException err)
                //    {
                //        ExceptionLog(string.Format("Exception {0}, {1}", "Receive", err.Message), err);
                //        DebugLog("readCallback Shutdown ObjectDisposedException", err);
                //    }
                //    catch (Exception err)
                //    {
                //        ExceptionLog(string.Format("Exception {0}, {1}", "Receive", err.Message), err);
                //        DebugLog("readCallback Shutdown Exception", err);
                //    }

                //    connectedSocket.Close();
                //    if (nonRecvTimer != null)
                //    {
                //        nonRecvTimer.Dispose();
                //        nonRecvTimer = null;
                //    }
                //    if (waitContinueRecvTimer != null)
                //    {
                //        waitContinueRecvTimer.Dispose();
                //        waitContinueRecvTimer = null;
                //    }
                //    connectedSocket = null;
                //}

                //try
                //{
                //    string msg = "Disconnect Socket";
                //    NotifyInterfaceServerArgs e = new NotifyInterfaceServerArgs(NOTIFY_EVENT.DISCONNECT, msg);
                //    OnNotifyInterfaceServer(e);
                //}
                //catch (Exception err)
                //{
                //    ExceptionLog(string.Format("Exception {0}, {1}", "Receive", err.Message), err);
                //}

                ////WaitOneにて、ブロックされている箇所をノンブロック状態に（BegeinAcceptを可能に）
                //this.socketEvent.Set();

                //return;
            }
        }

        /// <summary>
        /// 接続状態確認
        /// </summary>
        /// <param name="sock"></param>
        /// <returns><para>true:接続状態</para><para>false:非接続状態</para></returns>
        private bool GetConnectedState(Socket sock)
        {
            return sock.Connected;
        }

        /// <summary>
        /// デフォルトバッファーサイズサイズ
        /// </summary>
        private const int DEF_BUF_SIZE = 1280;

        /// <summary>
        /// ヘッダバイト数
        /// </summary>
        private const int CMD_SIZE_PREFIX = 8;

        #region 非同期用データオブジェクト StateObject
        /// <summary>
        /// 非同期用データオブジェクト
        /// </summary>
        /// <remarks></remarks>
        public class StateObject
        {
            public const int BufferSize = DEF_BUF_SIZE;
            private Socket workSocket;
            private byte[] bBuffer = new byte[BufferSize + 1];
            private byte[] bAllData;
            private int iReceived;
            private int iToReceive;
            private int iSendCnt;
            public int iSended;	//_ 暫定：送信済みバイト数
            public int iToSend;	//_ 暫定：送信バイト数

            private byte[] bSzeBuff = new byte[CMD_SIZE_PREFIX];

            /// <summary>
            /// 接続済みソケット
            /// </summary>
            /// <value></value>
            /// <returns></returns>
            /// <remarks></remarks>
            public Socket WorkSocket
            {
                get { return this.workSocket; }
                set { workSocket = value; }
            }


            /// <summary>
            /// 受信バッファー
            /// </summary>
            /// <value></value>
            /// <returns></returns>
            /// <remarks></remarks>
            public byte[] bytBuffer
            {
                get { return bBuffer; }
                set { bBuffer = value; }
            }


            /// <summary>
            /// 受信データ
            /// </summary>
            /// <value></value>
            /// <returns></returns>
            /// <remarks>複数に分けて受信したデータを結合したすべてのデータ</remarks>
            public byte[] bytAllData
            {
                get { return bAllData; }
                set { bAllData = value; }
            }


            /// <summary>
            /// 受信データを格納したバイト数
            /// </summary>
            /// <value></value>
            /// <returns></returns>
            /// <remarks></remarks>
            public int intReceived
            {
                get { return iReceived; }
                set { iReceived = value; }
            }


            /// <summary>
            /// 受信しないといけないバイト数
            /// </summary>
            /// <value></value>
            /// <returns></returns>
            /// <remarks></remarks>
            public int intToReceive
            {
                get { return iToReceive; }
                set { iToReceive = value; }
            }


            /// <summary>
            /// バッファーサイズ
            /// </summary>
            /// <value></value>
            /// <returns></returns>
            /// <remarks></remarks>
            public byte[] bytSizeBuffer
            {
                get { return bSzeBuff; }
                set { bSzeBuff = value; }
            }


            /// <summary>
            /// 送信回数
            /// </summary>
            /// <value></value>
            /// <returns></returns>
            /// <remarks></remarks>
            public int intSendCnt
            {
                get { return iSendCnt; }
                set { iSendCnt = value; }
            }

            /// <summary>
            /// コンストラクター
            /// </summary>
            /// <remarks></remarks>
            public StateObject()
            {
            }
        }
        //StateObject
        #endregion


        /// <summary>
        /// データ受信処理
        /// </summary>
        /// <param name="state">受信データ情報</param>
        /// <param name="ReadSize">今回の受信データサイズ</param>
        /// <remarks></remarks>
        private void readCallback(StateObject state, int ReadSize, out StateObject nextState)
        {
            Socket handler = state.WorkSocket;

            // 受信サイズが0の場合、切断通知
            if (ReadSize < 1)
            {
            //    #region 切断処理
            //    if (connectedSocket != null)
            //    {

            //        try
            //        {
            //            connectedSocket.Shutdown(SocketShutdown.Both);
            //        }
            //        catch (SocketException err)
            //        {
            //            ExceptionLog(string.Format("Exception {0}, {1}", "ShutDown", err.Message), err);
            //            DebugLog("readCallback Shutdown SocketException", err);
            //        }
            //        catch (ObjectDisposedException err)
            //        {
            //            ExceptionLog(string.Format("Exception {0}, {1}", "ShutDown", err.Message), err);
            //            DebugLog("readCallback Shutdown ObjectDisposedException", err);
            //        }
            //        catch (Exception err)
            //        {
            //            ExceptionLog(string.Format("Exception {0}, {1}", "ShutDown", err.Message), err);
            //            DebugLog("readCallback Shutdown Exception", err);
            //        }

            //        connectedSocket.Close();
            //        if (nonRecvTimer != null)
            //        {
            //            nonRecvTimer.Dispose();
            //            nonRecvTimer = null;
            //        }
            //        if (waitContinueRecvTimer != null)
            //        {
            //            waitContinueRecvTimer.Dispose();
            //            waitContinueRecvTimer = null;
            //        }
            //        connectedSocket = null;
            //    }

            //    try
            //    {
            //        string msg = "Disconnect Socket";
            //        NotifyInterfaceServerArgs e = new NotifyInterfaceServerArgs(NOTIFY_EVENT.DISCONNECT, msg);
            //        OnNotifyInterfaceServer(e);
            //        InformationLog("Client Disconnected");
            //    }
            //    catch (Exception err)
            //    {
            //        ExceptionLog(string.Format("Exception {0}, {1}", "ShutDown", err.Message), err);
            //    }

            //    //WaitOneにて、ブロックされている箇所をノンブロック状態に（BegeinAcceptを可能に）
            //    this.socketEvent.Set();

            //    #endregion

                TcpLog(string.Format("[TCP Disconnected] readCallback(), receive size 0."));
                nextState = null;
                return;
            }

            // 前回までの受信データサイズ
            int intReadSizePrevFunc = state.intReceived;

            // まだヘッダ読込完了していない場合、ヘッダ読込処理
            if (intReadSizePrevFunc < CMD_SIZE_PREFIX)
            {
                // ヘッダ情報を取得
                int outPos = intReadSizePrevFunc;
                for (int i = 0; ((i < ReadSize) && (outPos < CMD_SIZE_PREFIX)); i++, outPos++)
                {
                    state.bytSizeBuffer[outPos] = state.bytBuffer[i];
                }

                // ヘッダーサイズ(8Byte)分読み込んだら、ヘッダ解析
                if (CMD_SIZE_PREFIX <= (intReadSizePrevFunc + ReadSize))
                {
                    byte[] bytJSonSize = new byte[4];
                    bytJSonSize[0] = state.bytSizeBuffer[0];
                    bytJSonSize[1] = state.bytSizeBuffer[1];
                    bytJSonSize[2] = state.bytSizeBuffer[2];
                    bytJSonSize[3] = state.bytSizeBuffer[3];

                    byte[] bytRawSize = new byte[4];
                    bytRawSize[0] = state.bytSizeBuffer[4];
                    bytRawSize[1] = state.bytSizeBuffer[5];
                    bytRawSize[2] = state.bytSizeBuffer[6];
                    bytRawSize[3] = state.bytSizeBuffer[7];

                    if (BitConverter.IsLittleEndian)
                    {
                        //リトルエンディアンの場合は反転
                        Array.Reverse(bytJSonSize);
                        Array.Reverse(bytRawSize);
                    }

                    // JSONデータサイズ
                    UInt32 nJSonLen = System.BitConverter.ToUInt32(bytJSonSize, 0);
                    // Rawデータサイズ
                    UInt32 nRawLen = System.BitConverter.ToUInt32(bytRawSize, 0);

                    // データ長を取得する
                    state.intToReceive = (int)nJSonLen + (int)nRawLen;
                    state.intToReceive += CMD_SIZE_PREFIX;

                    // 受信データ領域を確保
                    state.bytAllData = new byte[state.intToReceive];

                    TcpLog(string.Format("TCP read header. JsonLen={0}, rawLen={1}",
                        nJSonLen, nRawLen));
                }
            }

            byte[] nextData = null;
            // 今回受信したデータが、受信継続中のデータなら受信データを全てコピー
            if ((intReadSizePrevFunc + ReadSize) <= state.intToReceive)
            {
                // 読込んだ実データを求める
                byte[] readData = new byte[ReadSize];
                Array.Copy(state.bytBuffer, 0, readData, 0, ReadSize);
                readData.CopyTo(state.bytAllData, state.intReceived);

                // 読込みサイズを計算
                state.intReceived += ReadSize;

                TcpLog(string.Format("TCP recv(size={0}). receiveSize={1}/{2}, over=0",
                    ReadSize, state.intReceived, state.intToReceive));
            }
            else
            {
                // 次のメッセージのデータも受信しているなら、次メッセージのデータとして保存しておく

                int inPos = 0;
                // 受信継続中のデータをコピー
                {
                    int outPos = intReadSizePrevFunc;
                    for (inPos = 0; outPos < state.intToReceive; inPos++, outPos++)
                    {
                        state.bytAllData[outPos] = state.bytBuffer[inPos];
                    }
                    state.intReceived = state.intToReceive;
                }

                // 次のメッセージの受信データコピー
                {
                    //InformationLog("Receive OverSize");

                    int nextDataSize = ReadSize - inPos;
                    nextData = new byte[nextDataSize];

                    for (int outPos = 0; inPos < ReadSize; inPos++, outPos++)
                    {
                        nextData[outPos] = state.bytBuffer[inPos];
                    }

                    TcpLog(string.Format("TCP recv(size={0}). receiveSize={1}/{2}, over={3}",
                        ReadSize, state.intReceived, state.intToReceive, nextDataSize));
                }
            }

            bool receiveContinue;

            // 受信データを処理する
            if (state.intToReceive == CMD_SIZE_PREFIX)
            {
                // JSONデータ、バイナリデータなしの電文はポーリングなので、ポーリング応答返すだけ

                //// ポーリング応答
                //ProcessData(handler, state);

                receiveContinue = false;

                // 後処理へ
                ////再読み込み。まだヘッダサイズまで読み込めていない。
                //handler.BeginReceive(state.bytBuffer, 0, StateObject.BufferSize, 0, new AsyncCallback(readCallback), state);

                //return;
            }
            else if ((CMD_SIZE_PREFIX < state.intReceived) && (state.intToReceive <= state.intReceived))
            {
                // 受信継続中のデータを全て受信した場合、受信データを処理
                ProcessData(handler, state);

                receiveContinue = false;

                // 後処理へ
            }
            else
            {
                // まだ受信継続中の場合
                receiveContinue = true;

                // 後処理へ
            }

            // 次データも受信しているなら、上に戻して、それも処理する
            if (nextData != null)
            {
                nextState = new StateObject();
                nextState.WorkSocket = handler;
                nextState.bytBuffer = new byte[nextData.Length];
                Array.Copy(nextData, nextState.bytBuffer, nextData.Length);
                return;
            }

            nextState = null;

            //後処理
            //if (this.mustStop == false)
            {
                if (receiveContinue == true)
                {
                    ////無通信監視タイマーを再開
                    //nonRecvTimer.Change(this.nonRecvTimeSec, Timeout.Infinite);

                    ////継続受信待ちタイマー起動
                    //waitContinueRecvTimer.Change(WAIT_CONTINUE_RECV_TIMER, Timeout.Infinite);

                    try
                    {
                        //再読み込み。データがまだ規定サイズまで読み込めていない。
                        handler.BeginReceive(state.bytBuffer, 0, state.bytBuffer.Length, 0, new AsyncCallback(readCallback), state);
                    }
                    catch
                    {
                        TcpLog(string.Format("receiveContinue BeginReceive(size={0}). nextReceiveBufferSize={4}.",
                            ReadSize, state.bytBuffer.Length));
                        throw;
                    }
                }
                else
                {
                    state = null;
                    state = new StateObject();
                    state.WorkSocket = handler;
                    //// 再読み込み
                    if (handler.Connected == true)
                    {
                        ////無通信監視タイマーを再開
                        //nonRecvTimer.Change(this.nonRecvTimeSec, Timeout.Infinite);

                        //// 継続受信待ちタイマー停止
                        //waitContinueRecvTimer.Change(Timeout.Infinite, Timeout.Infinite);

                        // 再読み込み
                        handler.BeginReceive(state.bytBuffer, 0, StateObject.BufferSize, 0, new AsyncCallback(readCallback), state);
                    }
                    else
                    {
                        // 切断時、切断処理
                        //this.DisconnectConnectedSocket();
                        handler = null;
                    }
                }
            }
            //else
            //{
            //    // 停止要求受けていれば、切断処理
            //    //this.DisconnectConnectedSocket();
            //    handler = null;
            //}


            return;

        }
        
        /// <summary>
        /// 受信データ解析処理
        /// </summary>
        /// <param name="handler">ソケット</param>
        /// <param name="state">非同期用のオブジェクト</param>
        /// <remarks><para>電文をそろえた状態、JSonデータとRawデータを分割し、上位ハンドラーへ渡す</para><para>データなしの場合はポーリングとみなし接続先へ同じデータを返送</para></remarks>
        private void ProcessData(Socket handler, StateObject state)
        {
            byte[] bytJSonSize = new byte[4];
            byte[] bytRawSize = new byte[4];

            bytJSonSize[0] = state.bytSizeBuffer[0];
            bytJSonSize[1] = state.bytSizeBuffer[1];
            bytJSonSize[2] = state.bytSizeBuffer[2];
            bytJSonSize[3] = state.bytSizeBuffer[3];
            bytRawSize[0] = state.bytSizeBuffer[4];
            bytRawSize[1] = state.bytSizeBuffer[5];
            bytRawSize[2] = state.bytSizeBuffer[6];
            bytRawSize[3] = state.bytSizeBuffer[7];

            if (BitConverter.IsLittleEndian)
            {
                //リトルエンディアンの場合は反転
                Array.Reverse(bytJSonSize);
                Array.Reverse(bytRawSize);
            }

            // JSONデータサイズ
            UInt32 nJSonLen = System.BitConverter.ToUInt32(bytJSonSize, 0);
            // Rawデータサイズ
            UInt32 nRawLen = System.BitConverter.ToUInt32(bytRawSize, 0);


            if (nJSonLen == 0 && nRawLen == 0)
            {
                ////ポーリングとみなし返答
                //handler.BeginSend(state.bytSizeBuffer, 0, CMD_SIZE_PREFIX, SocketFlags.None, new AsyncCallback(sendCallback), state);
            }
            else
            {
                // JSon電文とRawデータを分けた後、上位に引き渡す

                string JSonData = Encoding.GetEncoding("UTF-8").GetString(state.bytAllData, CMD_SIZE_PREFIX, (int)nJSonLen);
                byte[] RawData = new byte[nRawLen];
                Array.Copy(state.bytAllData, (int)(CMD_SIZE_PREFIX + nJSonLen), RawData, 0, (int)(nRawLen));
                
                try
                {
                    this.receiveFunc(JSonData, RawData);
                }
                catch
                {
                }
                //ReceiveDataArgs e = new ReceiveDataArgs(JSonData, RawData);
                //// イベント発行
                //OnReceiveData(e);
            }
        }

        #endregion // USF-200 のTcPサーバーから一部移植

        private bool ByteCopy(byte[] dst, long dstOffset, byte[] src, long srcOffset, long length)
        {
            long copyLength = length;
            if ((src.Length - srcOffset) < copyLength)
            {
                copyLength = (src.Length - srcOffset);
            }

            if (dst.Length < (dstOffset + copyLength))
            {
                return false;
            }

            for (int i = 0; i < copyLength; i++)
            {
                dst[dstOffset + i] = src[srcOffset + i];
            }
            return true;
        }



        /// <summary>
        /// receive delegate
        /// </summary>
        /// <param name="message"></param>
        public delegate void ReceiveFunc(string message, byte[] rawData);

        /// <summary>
        /// receive event
        /// </summary>
        public event ReceiveFunc receiveFunc;

        #endregion

        #region send

        public void SendKeepAlive()
        {
            byte[] sendData = { };
            // 送信処理
            // Begin sending the data to the remote device.
            client.BeginSend(sendData, 0, sendData.Length, 0,
                new AsyncCallback(SendCallback), client);
        }

        /// <summary>
        /// 送信処理
        /// </summary>
        /// <param name="jsonData">JSONメッセージ</param>
        public void Send(String jsonData)
        {
            this.Send(this.client, jsonData, new byte[0]);
        }

        /// <summary>
        /// 送信処理
        /// </summary>
        /// <param name="jsonData">JSONメッセージ</param>
        /// <param name="filePath">バイナリデータファイル</param>
        public void Send(String jsonData, String filePath)
        {
            if (System.IO.File.Exists(filePath) == false)
            {
                //ファイルが存在しない場合、何もしない
                return;
            }

            // ファイルの中身を byte 配列に置換
            byte[] rawData = System.IO.File.ReadAllBytes(filePath);

            this.Send(this.client, jsonData, rawData);
        }

        /// <summary>
        /// 送信処理
        /// </summary>
        /// <param name="jsonData">JSONメッセージ</param>
        /// <param name="rawData">RAWデータ</param>
        public void Send(String jsonData, byte[] rawData)
        {
            this.Send(this.client, jsonData, rawData);
        }

        /// <summary>
        /// 送信処理
        /// </summary>
        /// <param name="client">TCPクライアント</param>
        /// <param name="jsonData">JSONメッセージ</param>
        /// <param name="rawData">RAWデータ</param>
        private void Send(Socket client, String jsonData, byte[] rawData)
        {
            //// Convert the string data to byte data using ASCII encoding.
            //byte[] byteData = Encoding.ASCII.GetBytes(jsonData);

            byte[] bytJSon = new byte[jsonData.Length];
            bytJSon = Encoding.GetEncoding("UTF-8").GetBytes(jsonData);

            byte[] jsonLen = new byte[4];
            jsonLen = BitConverter.GetBytes(jsonData.Length);

            byte[] rawDataLen = new byte[4];
            rawDataLen = BitConverter.GetBytes(rawData.Length);

            long sendSize = jsonLen.Length + rawDataLen.Length + bytJSon.Length + rawData.Length;
            byte[] sendData = new byte[sendSize];

            if (BitConverter.IsLittleEndian)
            {
                //リトルエンディアンの場合は反転
                Array.Reverse(jsonLen);
                Array.Reverse(rawDataLen);
            }

            // 電文データ作成
            int offset = 0;
            Array.Copy(jsonLen, 0, sendData, offset, jsonLen.Length);

            offset = jsonLen.Length;
            Array.Copy(rawDataLen, 0, sendData, offset, rawDataLen.Length);

            offset += rawDataLen.Length;
            Array.Copy(bytJSon, 0, sendData, offset, bytJSon.Length);

            offset += bytJSon.Length;
            Array.Copy(rawData, 0, sendData, offset, rawData.Length);


            // 送信処理
            // Begin sending the data to the remote device.
            client.BeginSend(sendData, 0, sendData.Length, 0,
                new AsyncCallback(SendCallback), client);

            TcpLog(string.Format("send(len={0}), jsonLen=0x{1:x02}{2:x02}{3:x02}{4:x02}({5}) rawLen=0x{6:x02}{7:x02}{8:x02}{9:x02}({10})",
                sendData.Length,
                sendData[0], sendData[1], sendData[2], sendData[3], jsonData.Length,
                sendData[4], sendData[5], sendData[6], sendData[7], rawData.Length));
        }

        /// <summary>
        /// SendCallback
        /// </summary>
        /// <param name="ar"></param>
        private void SendCallback(IAsyncResult ar)
        {
            try
            {
                // Retrieve the socket from the state object.
                Socket client = (Socket)ar.AsyncState;

                // Complete sending the data to the remote device.
                int bytesSent = client.EndSend(ar);
                Console.WriteLine("Sent {0} bytes to server.", bytesSent);

                // Signal that all bytes have been sent.
                sendDone.Set();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        #endregion


        public delegate void TcpLogDelegate(string log);

        public event TcpLogDelegate TcpLogEvent;

        private void TcpLog(string log)
        {
            if (TcpLogEvent == null)
            {
                return;
            }

            
            TcpLogEvent(log);
        }
    }
}
