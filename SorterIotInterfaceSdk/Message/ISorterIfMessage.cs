using System;
using Glory.SorterInterface.MessageFormat.Message;

namespace Glory.SorterInterface.Message
{
    /// <summary>
    /// sorter IoT interface message
    /// </summary>
    public interface ISorterIfMessage : IDisposable
    {
        /// <summary>
        /// message type
        /// </summary>
        MessageType Type { get; }

        /// <summary>
        /// message name
        /// </summary>
        MessageName Name { get; }

        /// <summary>
        /// message version
        /// </summary>
        int Version { get; }

        /// <summary>
        /// message ID
        /// </summary>
        string ID { get; }
    }
}
