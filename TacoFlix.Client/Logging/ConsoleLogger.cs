using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TacoFlix.Model.Logging;

namespace TacoFlix.Client.Logging
{
    public sealed class ConsoleLogger : ILogger
    {
        /// <summary>
        /// Logs the message as an Info
        /// </summary>
        /// <param name="message">The message.</param>
        public void Debug(string message)
        {
            Console.WriteLine(message);
        }

        /// <summary>
        /// Logs the message as an Info
        /// </summary>
        /// <param name="message">The message.</param>
        public void Info(string message)
        {
            Console.WriteLine(message);
        }

        /// <summary>
        /// Logs a warning message
        /// </summary>
        /// <param name="message">The message.</param>
        public void Warning(string message)
        {
            Console.WriteLine(message);
        }

        /// <summary>
        /// Logs a warning message
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="exception">The exception.</param>
        public void Warning(string message, Exception exception)
        {
            Console.WriteLine(exception.Message);
        }

        /// <summary>
        /// Logs an error message
        /// </summary>
        /// <param name="message">The message.</param>
        public void Error(string message)
        {
            Console.WriteLine(message);
        }

        /// <summary>
        /// Logs an error message
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="exception">The exception.</param>
        public void Error(string message, Exception exception)
        {
            Console.WriteLine(message);
            Console.WriteLine(exception.Message);
        }
    }
}
