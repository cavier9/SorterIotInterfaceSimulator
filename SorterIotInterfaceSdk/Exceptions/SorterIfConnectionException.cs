using System;

namespace Glory.SorterInterface.Exceptions
{
    /// <summary>
    ///Connection to the broker/device exception
    /// </summary>
    public class SorterIfConnectionException : Exception
    {
        /// <summary>
        /// constructor
        /// </summary>
        public SorterIfConnectionException()
        {
        }

        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="e">Inner Exception</param>
        public SorterIfConnectionException(Exception e)
            : base(e.Message, e)
        {
        }
    }
}
