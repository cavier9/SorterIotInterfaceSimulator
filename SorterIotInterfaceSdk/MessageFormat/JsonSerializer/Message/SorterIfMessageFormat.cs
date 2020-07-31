using System;
using Glory.SorterInterface.Message;

#if JSON_SERIALIZER

using System.IO;
using System.Text;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;


namespace Glory.SorterInterface.MessageFormat.Message
{
    /// <summary>
    /// message format
    /// </summary>
    [DataContract]
    public class SorterIfMessageFormat : ISorterIfMessage
    {
        #region fixed parameter

        /// <summary>
        /// メッセージバージョン
        /// </summary>
        internal const MessageVersion MESSAGE_VERSION = MessageVersion.VERSION_1;

        #endregion

        #region property

        /// <summary>
        /// message type
        /// </summary>
        [IgnoreDataMember]
        public MessageType Type
        {
            get { return this.type; }
        }

        /// <summary>
        /// message name
        /// </summary>
        [IgnoreDataMember]
        public MessageName Name
        {
            get { return this.name; }
        }

        /// <summary>
        /// message version
        /// </summary>
        [IgnoreDataMember]
        public int Version
        {
            get { return this.version; }
        }

        /// <summary>
        /// message ID
        /// </summary>
        [IgnoreDataMember]
        public string ID
        {
            get { return this.id; }
        }

        #endregion

        #region parameter

        /// <summary>
        /// message type
        /// </summary>
        [IgnoreDataMember]
        public MessageType type;

        /// <summary>
        /// message type
        /// </summary>
        [DataMember(Name = "Type")]
        public string typeStr
        {
            get { return type.ToString(); }
            set { this.type = (MessageType)(Enum.Parse(typeof(MessageType), value, false)); }
        }

        /// <summary>
        /// message name
        /// </summary>
        [IgnoreDataMember]
        public MessageName name;

        /// <summary>
        /// message name
        /// </summary>
        [DataMember(Name = "Name")]
        public string nameStr
        {
            get { return name.ToString(); }
            set { this.name = (MessageName)(Enum.Parse(typeof(MessageName), value, false)); }
        }

        /// <summary>
        /// message version
        /// </summary>
        [DataMember(Name = "Version")]
        public int version;

        /// <summary>
        /// message ID
        /// </summary>
        [DataMember(Name = "ID")]
        public string id;

        #endregion

        #region constructor

        /// <summary>
        /// constructor
        /// </summary>
        public SorterIfMessageFormat()
            : this(MessageType.Undef, MessageName.Undef, SorterIfMessageFormat.MESSAGE_VERSION)
        {
        }

        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="type">type</param>
        /// <param name="name">name</param>
        public SorterIfMessageFormat(MessageType type, MessageName name)
            : this(type, name, SorterIfMessageFormat.MESSAGE_VERSION)
        {
        }

        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="type">type</param>
        /// <param name="name">name</param>
        /// <param name="version">version</param>
        public SorterIfMessageFormat(MessageType type, MessageName name, MessageVersion version)
        {
            this.type = type;
            this.name = name;
            this.version = (int)version;

            if (type == MessageType.Command)
            {
                this.id = "{" + Guid.NewGuid().ToString() + "}";
            }
            else
            {
                this.id = null;
            }
        }

        #endregion

        #region Dispose

        /// <summary>
        /// Dispose
        /// </summary>
        public void Dispose()
        {
        }

        #endregion

        #region serialize

        /// <summary>
        /// serialize to json from message
        /// </summary>
        /// <param name="message">message object</param>
        /// <returns>JSON string</returns>
        static public string Serialize<T>(T message) where T : SorterIfMessageFormat, new()
        {
            string jsonString = string.Empty;

            try
            {
                DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(T));
                using (MemoryStream ms = new MemoryStream())
                {
                    serializer.WriteObject(ms, message);
                    jsonString = Encoding.UTF8.GetString(ms.ToArray());
                }
            }
            catch (Exception ex)
            {
                throw new SerializationException(string.Format("{0} serialize Exception", typeof(T)), ex);
            }
            return jsonString;
        }

        #endregion

        #region deserialize

        /// <summary>
        /// deserialize to message from json
        /// </summary>
        /// <param name="jsonString">JSON string</param>
        /// <returns>message object</returns>
        static public T DeSerialize<T>(string jsonString) where T : SorterIfMessageFormat, new()
        {
            T message = null;

            try
            {
                DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(T));
                byte[] bytes = Encoding.UTF8.GetBytes(jsonString);

                using (MemoryStream ms = new MemoryStream(bytes))
                {
                    message = (T)(serializer.ReadObject(ms));
                }
            }
            catch (Exception ex)
            {
                throw new SerializationException(string.Format("{0} deserialize Exception", typeof(T)), ex);
            }

            return message;
        }

        #endregion
    }
}

#endif