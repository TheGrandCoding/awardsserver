using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AwardsServer
{
    public static class Logging
    {
        public class LogMessage
        {
            public readonly LogSeverity Severity;
            public readonly string Source;
            public readonly Exception Error = null;
            public readonly string Message;

            public LogMessage(LogSeverity severity, string message, string source)
            {
                Severity = severity;
                Source = source;
                Message = message;
            }
            public LogMessage(string source, Exception ex)
            {
                Severity = LogSeverity.Error;
                Error = ex;
                Source = source;
            }
            public LogMessage(LogSeverity severity, string message)
            {
                Severity = severity;
                Message = message;
                Source = "App";
            }
            public LogMessage(string message)
            {
                Severity = LogSeverity.Debug;
                Message = message;
                Source = "App";
            }
            public LogMessage(LogSeverity severity, string source, Exception ex)
            {
                Severity = severity;
                Source = source;
                Error = ex;
            }
        }
        public enum LogSeverity
        {
            Debug = 0,
            Info,
            Warning,
            Severe,
            Error
        }
        private static object lockObj = new object();
        private static string _logDirectory = Path.Combine( Directory.GetCurrentDirectory(), "Logs");
        private static string _logFile => Path.Combine(_logDirectory, $"{DateTime.UtcNow.ToString("yyyy-MM-dd")}.txt");
        private static string longest = LogSeverity.Warning.ToString() + 1;
        public static void Log(LogMessage msg)
        {
            lock(lockObj)
            {
                if (!Directory.Exists(_logDirectory))     // Create the log directory if it doesn't exist
                    Directory.CreateDirectory(_logDirectory);
                if (!File.Exists(_logFile))               // Create today's log file if it doesn't exist
                    File.Create(_logFile).Dispose();

                int spaces = longest.Length;
                spaces -= msg.Severity.ToString().Length;

                int startLength = "04:37:08.[Info] App: ".Length; // these make it look nice in the console 
                                                                  // by making it line up
                string spaceGap = String.Concat(Enumerable.Repeat(" ", spaces)); 
                //for(int i = 0; i < spaces; i++) { spaceGap += " "; }

                string logText = $"{DateTime.Now.ToString("hh:mm:ss.fff")}{spaceGap}[{msg.Severity}] {msg.Source}: {msg.Error?.ToString() ?? msg.Message}";
                File.AppendAllText(_logFile, logText + "\r\n");     // Write the log text to a file
                logText = logText.Replace("\n", "\n    ..." + (new string(' ', startLength)));
                // again, makes it look fancy by colour and stuff
                switch (msg.Severity)
                {
                    case LogSeverity.Severe:
                        Console.ForegroundColor = ConsoleColor.Magenta;
                        break;
                    case LogSeverity.Error:
                        Console.ForegroundColor = ConsoleColor.Red;
                        break;
                    case LogSeverity.Warning:
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        break;
                    case LogSeverity.Info:
                        Console.ForegroundColor = ConsoleColor.White;
                        break;
                    case LogSeverity.Debug:
                            Console.ForegroundColor = ConsoleColor.DarkGray;
                        break;
                }
                bool shouldDisplay = true;
                if(msg.Severity < Program.Options.Only_Show_Above_Severity)
                {
                    shouldDisplay = false;
                }
                if(msg.Source.EndsWith("/Rec") && !Program.Options.Display_Recieve_Client)
                {
                    shouldDisplay = false;
                }
                if(msg.Source.EndsWith("/Send") && !Program.Options.Display_Send_Client)
                {
                    shouldDisplay = false;
                }
                if(shouldDisplay)
                    Console.Out.WriteLineAsync(logText);       // Write the log text to the console
                Console.ForegroundColor = ConsoleColor.Blue;
            }
        }
        public static void Log(string message)
        {
            Log(new LogMessage(message));
        }
        public static void Log(LogSeverity severity, string message)
        {
            Log(new LogMessage(severity, message));
        }
        public static void Log(LogSeverity severity, string message, string source)
        {
            Log(new LogMessage(severity, message, source));
        }
        public static void Log(string source, Exception ex)
        {
            Log(new LogMessage(source, ex));
        }
    }
}
