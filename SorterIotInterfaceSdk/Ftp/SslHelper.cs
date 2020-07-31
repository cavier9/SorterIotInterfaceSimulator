using System;
using System.Globalization;
using System.Diagnostics;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

//#if WINXP
using System.Net.Security;
using System.Threading;
//#endif

namespace Glory.SorterInterface.Ftp
{
    /// <summary>
    /// SSL Interface
    /// </summary>
    public interface ISslHelper : IDisposable
    {
        /// <summary>
        /// StartSSL
        /// </summary>
        /// <param name="validate">true:validate certificate</param>
        void StartSSL(bool validate);

        /// <summary>
        /// DataSendProcess
        /// </summary>
        /// <param name="buf">Data</param>
        /// <param name="size">DataSize</param>
        /// <returns>SendDataCount</returns>
        int Send(byte[] buf, int size);

        /// <summary>
        /// DataReceiveProcess
        /// </summary>
        /// <param name="buf">buffer</param>
        /// <param name="timeout">timeout(sec)</param>
        /// <returns>bufferCount</returns>
        int Recv(byte[] buf, int timeout);

        /// <summary>
        /// Available
        /// </summary>
        bool Available { get; }

    }


    /// <summary>
    /// .NET Framework - SSL process
    /// </summary>
    class SslHelperFW : ISslHelper
    {
        public SslHelperFW(Socket socket, string host)
        {
            this.socket = socket;
            this.host = host;
        }


        public void Dispose()
        {
            if (stream != null)
            {
                stream.Dispose();
            }
        }

        private Socket socket;
        private string host;
        private SslStream stream = null;
        private bool validate = true;


        private bool OnCertificateValidation(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors errors)
        {

            // Return true if there are no policy errors

            // The certificate can also be manually verified to 

            //make sure it meets your specific // policies by 

            //     interrogating the x509Certificate object.

            if (validate == false)
            {
                // not use server certificate
                return true;
            }

            if (errors == SslPolicyErrors.None)
            {
                // no error
                return true;
            }
            else
            {
                bool ret = true;

                if ((errors & SslPolicyErrors.RemoteCertificateNotAvailable) != 0)
                {
                    // error NotAvailable
                    ret &= false;
                }
                if ((errors & SslPolicyErrors.RemoteCertificateNameMismatch) != 0)
                {
                    // error NameMismatch
                    ret &= false;
                }
                if ((errors & SslPolicyErrors.RemoteCertificateChainErrors) != 0)
                {
                    // error ChainErrors
                    ret &= false;
                }

                return ret;
            }

            //if (validate && errors == SslPolicyErrors.None)
            //{

            //    Console.WriteLine(errors.ToString());

            //    return false;

            //}

            //else
            //{

            //    return true;

            //}

        }

        private bool getSslStream(Socket Csocket)
        {
            bool ret = false;
            RemoteCertificateValidationCallback callback = new RemoteCertificateValidationCallback(OnCertificateValidation);
            SslStream _sslStream = new SslStream(new NetworkStream(Csocket), false, callback);

            try
            {


                _sslStream.AuthenticateAsClient(
                    this.host,
                    null,
                    System.Security.Authentication.SslProtocols.Ssl3 | System.Security.Authentication.SslProtocols.Tls,
                    true);

                if (_sslStream.IsAuthenticated)
                {
                    stream = _sslStream;
                    ret = true;
                }

            }
            catch
            {
                throw;
            }
            return (ret);
        }

        /// <summary>
        /// StartSSL
        /// </summary>
        public void StartSSL(bool validate)
        {
            this.validate = validate;
            if (this.stream != null)
            {
                this.stream.Dispose();
            }
            getSslStream(this.socket);
        }

        /// <summary>
        /// DataSendProcess
        /// </summary>
        /// <param name="buf">Data</param>
        /// <param name="size">DataSize</param>
        /// <returns>SendDataCount</returns>
        public int Send(byte[] buf, int size)
        {
            if (this.stream == null)
            {
                size = this.socket.Send(buf, size, SocketFlags.None);
            }
            else
            {
                this.stream.Write(buf, 0, size);
            }

            return (size);
        }

        /// <summary>
        /// DataReceiveProcess
        /// </summary>
        /// <param name="buf">buffer</param>
        /// <param name="timeout">timeout(sec)</param>
        /// <returns>bufferCount</returns>
        public int Recv(byte[] buf, int timeout)
        {
            int count = 0;

            Array.Clear(buf, 0, buf.Length);

            this.socket.ReceiveTimeout = timeout * 1000;

            // デバッグ用
            //int logBufLength = 0;
            //int logCount = 0;
            //int logLoopCount = 0;

            if (this.stream == null)
            {
                do
                {
                    count += this.socket.Receive(buf, count, buf.Length - count, SocketFlags.None);

                    //logBufLength = buf.Length;
                    //logCount = count;
                    //logLoopCount++;

                } while (Available && count < buf.Length);


                //int aaa = logBufLength;
                //aaa = logCount;
                //aaa = logLoopCount;

            }
            else
            {
                count = this.stream.Read(buf, 0, buf.Length);
            }
            return (count);
        }
        /// <summary>
        /// Available
        /// </summary>
        public bool Available
        {
            get
            {
                // 100msec ポーリングする

                // WinXP で socket.Poll するとだんまりになるので、自前のfor文でポーリングする
                //return (this.socket.Poll(100000, SelectMode.SelectRead));

                for (int count = 0; count < 10; count++)
                {
                    if (this.socket.Available != 0)
                    {
                        return (true);
                    }
                    else
                    {
                        Thread.Sleep(1);
                    }
                }
                return (false);
            }
        }
    }

}
