using Glory.SorterInterface.Message;
using Glory.SorterInterface.MessageFormat.Message;
using System;
using System.Runtime.Serialization;

namespace Glory.SorterInterface.MessageFormat.JsonSerializer.Message.Command
{
    /// <summary>
    /// SetOperator Command クラス
    /// </summary>
    [DataContract]
    public class SetOperator : SorterIfMessageFormat
    {
        /// <summary>
        /// コンストラクタ
        /// </summary>
        public SetOperator()
            : base(MessageType.Command, MessageName.SetOperator)
        {
        }
        
        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="id"></param>
        /// <param name="password"></param>
        public SetOperator(string id, string password)
            : this()
        {
            this.Detail = new detail(id, password);
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
            /// <param name="id"></param>
            /// <param name="password"></param>
            public detail(string id, string password)
            {
                this.ID = id;
                this.Password = password;
            }

            /// <summary>
            /// ID
            /// </summary>
            [DataMember]
            public string ID { get; set; }

            /// <summary>
            /// Password
            /// </summary>
            [DataMember]
            public string Password { get; set; }
        }
    }
}
