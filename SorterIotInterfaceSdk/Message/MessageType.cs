using System;

namespace Glory.SorterInterface.Message
{
    /// <summary>
    /// message type
    /// </summary>
    public enum MessageType
    {
        /// <summary>
        /// Undeffine message type
        /// </summary>
        Undef,

        /// <summary>
        /// Command
        /// </summary>
        Command,

        /// <summary>
        /// Response
        /// </summary>
        Response,

        /// <summary>
        /// Event
        /// </summary>
        Event,
    }
}
