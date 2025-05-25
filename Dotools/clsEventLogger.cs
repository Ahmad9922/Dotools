using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dotools
{
    public class clsEventLogger
    {
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
        /// This method sets the source using the current Process name.
        /// </summary>
        public static void WriteEntryInApplicationLog(string Message, EventLogEntryType Type)
        {
            WriteEntry("Application", Process.GetCurrentProcess().ProcessName, Message, Type);
        }

        public static void WriteEntryInApplicationLog(string SourceName, string Message, EventLogEntryType Type)
        {
            WriteEntry("Application", SourceName, Message, Type);
        }
    }
}
