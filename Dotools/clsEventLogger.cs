using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dotools
{
    /// <summary>
    /// Provides static methods for writing entries to the Windows Event Log, including application-specific logging.
    /// </summary>
    public class clsEventLogger
    {

        /// <summary>
        /// Writes a new entry to the Windows Event Log with the specified log name, source, message, and entry type.
        /// </summary>
        /// <param name="LogName">The name of the event log (e.g., "Application", "System").</param>
        /// <param name="SourceName">The source name by which the application is registered on the event log.</param>
        /// <param name="Message">The message to write into the event log.</param>
        /// <param name="Type">The type of event log entry (e.g., Information, Warning, Error).</param>
        public static void WriteEntry(string LogName, string SourceName, string Message, EventLogEntryType Type)
        {
            if (!string.IsNullOrEmpty(LogName) && !string.IsNullOrEmpty(SourceName))
            {
                try
                {
                    using (EventLog eventLog = new EventLog(LogName))
                    {
                        eventLog.Source = SourceName;

                        if (!EventLog.SourceExists(eventLog.Source))
                        {
                            EventLog.CreateEventSource(eventLog.Source, LogName);
                        }

                        eventLog.WriteEntry(Message, Type);
                    }
                }
                catch (Exception ex)
                {
                    
                }
            }
        }

        /// <summary>
        /// Writes a message to the Application event log with the specified entry type.
        /// The log source is set to the name of the current process.
        /// </summary>
        /// <param name="Message">The message to be logged.</param>
        /// <param name="Type">The type of the log entry (e.g., Information, Warning, Error).</param>
        public static void WriteEntryInApplicationLog(string Message, EventLogEntryType Type)
        {
            WriteEntry("Application", Process.GetCurrentProcess().ProcessName, Message, Type);
        }

        /// <summary>
        /// Writes a message to the Application event log using the specified source name and entry type.
        /// </summary>
        /// <param name="SourceName">The name of the event log source.</param>
        /// <param name="Message">The message to be logged.</param>
        /// <param name="Type">The type of the log entry (e.g., Information, Warning, Error).</param>
        public static void WriteEntryInApplicationLog(string SourceName, string Message, EventLogEntryType Type)
        {
            WriteEntry("Application", SourceName, Message, Type);
        }
    }
}
