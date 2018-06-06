using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

using Skycap.IO;

using Windows.Storage;

namespace Skycap.Net.Common
{
    /// <summary>
    /// Represents the file for logging all application activity.
    /// </summary>
    public class LogFile
    {
        /// <summary>
        /// The semaphore slim to control access to a file resource.
        /// </summary>
        private static SemaphoreSlim _mutex = new SemaphoreSlim(1);
        /// <summary>
        /// The log file path format.
        /// </summary>
        private const string LogFilePathFormat = @"{0}\Logs\{1}.log";
        /// <summary>
        /// The log file name format.
        /// </summary>
        private const string LogFileNameFormat = "yyyy-MM-dd";
        /// <summary>
        /// The log file instance.
        /// </summary>
        private static LogFile _instance;
        /// <summary>
        /// The log file.
        /// </summary>
        private volatile StorageFile _logFile;

        /// <summary>
        /// Initialises a new instance of the Skycap.Data.LogFile class.
        /// </summary>
        private LogFile()
        { 
            // Initialise local variables
            Task.Run(async () =>
            {
                string logFileName = DateTime.Now.ToString(LogFileNameFormat);
                _logFile = await IOUtil.GetCreateFile(string.Format(LogFilePathFormat, ApplicationData.Current.LocalFolder.Path, logFileName), CreationCollisionOption.OpenIfExists);
            }).Wait();
        }

        /// <summary>
        /// Gets the log file instance.
        /// </summary>
        public static LogFile Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new LogFile();

                return _instance;
            }
        }

        /// <summary>
        /// Logs the specified information to the log file.
        /// </summary>
        /// <param name="accountName">The account name.</param>
        /// <param name="mailboxName">The mailbox name.</param>
        /// <param name="message">The message.</param>
        public void LogInformation(string accountName, string mailboxName, string message)
        {
            Log(DateTime.Now, "INF", accountName, mailboxName, message);
        }

        /// <summary>
        /// Logs the specified warning to the log file.
        /// </summary>
        /// <param name="accountName">The account name.</param>
        /// <param name="mailboxName">The mailbox name.</param>
        /// <param name="message">The message.</param>
        public void LogWarning(string accountName, string mailboxName, string message)
        {
            Log(DateTime.Now, "WAR", accountName, mailboxName, message);
        }

        /// <summary>
        /// Logs the specified error to the log file.
        /// </summary>
        /// <param name="accountName">The account name.</param>
        /// <param name="mailboxName">The mailbox name.</param>
        /// <param name="message">The message.</param>
        public void LogError(string accountName, string mailboxName, string message)
        {
            Log(DateTime.Now, "ERR", accountName, mailboxName, message);
        }

        /// <summary>
        /// Logs the specified information to the log file.
        /// </summary>
        /// <param name="time">The log entry time.</param>
        /// <param name="logType">The log type.</param>
        /// <param name="accountName">The account name.</param>
        /// <param name="mailboxName">The mailbox name.</param>
        /// <param name="message">The message.</param>
        private void Log(DateTime time, string logType, string accountName, string mailboxName, string message)
        {
            Task.Run(async() =>
            {
                await _mutex.WaitAsync();
                try
                {
                    // If the log file exists
                    if (_logFile != null)
                    {
                        // If no account name
                        if (string.IsNullOrEmpty(accountName))
                        {
                            string logEntryFormat = "{0}-{1}: {2}{3}";
                            await FileIO.AppendTextAsync(_logFile, string.Format(logEntryFormat, time.ToString("HH:mm:ss tt"), logType, message, Environment.NewLine));
                        }
                        // If no mailbox name
                        else if (string.IsNullOrEmpty(mailboxName))
                        {
                            string logEntryFormat = "{0}-{1}: {2} - {3}{4}";
                            await FileIO.AppendTextAsync(_logFile, string.Format(logEntryFormat, time.ToString("HH:mm:ss tt"), logType, accountName, message, Environment.NewLine));
                        }
                        // Else if all info is available
                        else
                        {
                            string logEntryFormat = "{0}-{1}: {2}|{3} - {4}{5}";
                            await FileIO.AppendTextAsync(_logFile, string.Format(logEntryFormat, time.ToString("HH:mm:ss tt"), logType, accountName, mailboxName, message, Environment.NewLine));
                        }
                    }
                }
                catch(Exception ex)
                {
                    Debug.WriteLine(ex);
                }
                finally
                {
                    _mutex.Release();
                }
            });
        }
    }
}
