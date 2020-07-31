using Glory.SorterInterface.Message;
using Glory.SorterInterface.MessageFormat.Message;
using System;
using System.Runtime.Serialization;

namespace Glory.SorterInterface.MessageFormat.JsonSerializer.Message.Command
{
    /// <summary>
    /// CreateSession Command クラス
    /// </summary>
    [DataContract]
    public class CreateSession : SorterIfMessageFormat
    {
        /// <summary>
        /// コンストラクタ
        /// </summary>
        public CreateSession()
            : base(MessageType.Command, MessageName.CreateSession)
        {
        }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="key"></param>
        public CreateSession(string systemName, string systemVersion)
            : this()
        {
            this.Detail = new detail(systemName, systemVersion);
        }

        /// <summary>
        /// Detail
        /// </summary>
        [DataMember]
        public detail Detail { get; set; }

        /// <summary>
        /// Detail
        /// </summary>
        public class detail
        {
            /// <summary>
            /// コンストラクタ
            /// </summary>
            public detail()
            {
            }

            /// <summary>
            /// コンストラクタ
            /// </summary>
            /// <param name="key"></param>
            public detail(string systemName, string systemVersion)
            {
                this.SystemName = systemName;
                this.SystemVersion = systemVersion;
            }

            /// <summary>
            /// SystemName
            /// </summary>
            [DataMember]
            public string SystemName { get; set; }

            /// <summary>
            /// SystemVersion
            /// </summary>
            [DataMember]
            public string SystemVersion { get; set; }
        }
    }
}
