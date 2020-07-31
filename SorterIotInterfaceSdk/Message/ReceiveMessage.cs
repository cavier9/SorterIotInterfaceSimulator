using System;
using Glory.SorterInterface.MessageFormat.Message;

namespace Glory.SorterInterface.Message
{
    /// <summary>
    /// receive message
    /// </summary>
    public class ReceiveMessage : SorterIfMessageFormat
    {
        #region property

        #endregion

        #region parameter

        /// <summary>
        /// receive JSON message
        /// </summary>
        private string receiveJsonMessage;

        #endregion

        #region constructor
        
        /// <summary>
        /// constructor
        /// </summary>
        public ReceiveMessage()
        {
            base.type = MessageType.Undef;
            base.name = MessageName.Undef;
            base.version = 0;
            this.receiveJsonMessage = string.Empty;
        }

        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="receiveJsonMessage">receive JSON message</param>
        public ReceiveMessage(string receiveJsonMessage)
        {
            using (SorterIfMessageFormat obj = SorterIfMessageFormat.DeSerialize<SorterIfMessageFormat>(receiveJsonMessage))
            {
                if (obj == null)
                {
                    base.type = MessageType.Undef;
                    base.name = MessageName.Undef;
                    base.version = 0;
                    this.receiveJsonMessage = receiveJsonMessage;
                }
                else
                {
                    base.type = obj.Type;
                    base.name = obj.Name;
                    base.version = obj.Version;
                    this.receiveJsonMessage = receiveJsonMessage;
                }
            }
        }
        #endregion
    }
}
