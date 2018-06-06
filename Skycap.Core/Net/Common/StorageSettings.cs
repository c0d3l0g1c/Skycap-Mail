using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Skycap.Data;
using Skycap.IO;
using Windows.Storage;

namespace Skycap.Net.Common
{
    /// <summary>
    /// Represents a storage helper class.
    /// </summary>
    public class StorageSettings
    {
        /// <summary>
        /// The semaphore slim to control access to a file resource.
        /// </summary>
        private static SemaphoreSlim _accountSettingsDataDictionaryMutex = new SemaphoreSlim(1);
        /// <summary>
        /// The account settings data dictionary file path.
        /// </summary>
        private static string _accountSettingsDataDictionaryFilePath;
        /// <summary>
        /// The account settings data dictionary file.
        /// </summary>
        private static StorageFile _accountSettingsDataDictionaryFile;
        /// <summary>
        /// The semaphore slim to control access to a file resource.
        /// </summary>
        private static SemaphoreSlim _mailHeaderDictionaryMutex = new SemaphoreSlim(1);
        /// <summary>
        /// The mail header dictionary file path.
        /// </summary>
        private static string _mailHeaderDictionaryFilePath;
        /// <summary>
        /// The mail header dictionary file.
        /// </summary>
        private static StorageFile _mailHeaderDictionaryFile;

        /// <summary>
        /// Initialises a new instance of the Skycap.Net.Common.StorageSettings class.
        /// </summary>
        static StorageSettings()
        {
            // Create UnreadEmails setting
            if (!ApplicationData.Current.LocalSettings.Values.ContainsKey("UnreadEmails"))
                ApplicationData.Current.LocalSettings.Values["UnreadEmails"] = 0;

            // Create UnreadEmails container
            if (!ApplicationData.Current.LocalSettings.Containers.ContainsKey("UnreadEmails"))
                ApplicationData.Current.LocalSettings.CreateContainer("UnreadEmails", ApplicationDataCreateDisposition.Always);
        }

        /// <summary>
        /// Initialises a new instance of the Skycap.Settings.StorageSettings class.
        /// </summary>
        public static async Task InitialiseAsync()
        {
            try
            {
                // Set the account settings data dictionary path
                _accountSettingsDataDictionaryFilePath = Path.Combine(ApplicationData.Current.LocalFolder.Path, "AccountSettingsDataDictionary.xml");

                // Initialise AccountSettingsDataDictionary
                if (_accountSettingsDataDictionaryFile == null)
                {
                    AccountSettingsDataDictionary = new Dictionary<string, AccountSettingsData>();

                    if ((_accountSettingsDataDictionaryFile = await IOUtil.FileExists(_accountSettingsDataDictionaryFilePath)) == null)
                        _accountSettingsDataDictionaryFile = await IOUtil.GetCreateFile(_accountSettingsDataDictionaryFilePath, CreationCollisionOption.OpenIfExists);
                    else
                        AccountSettingsDataDictionary = IOUtil.Deserialize<Dictionary<string, AccountSettingsData>>(await FileIO.ReadTextAsync(_accountSettingsDataDictionaryFile));
                }

                // Set the mail header dictionary path
                _mailHeaderDictionaryFilePath = Path.Combine(ApplicationData.Current.LocalFolder.Path, "MailHeaderDictionary.xml");

                // Initialise MailHeaderDictionary
                if (_mailHeaderDictionaryFile == null)
                {
                    MailHeaderDictionary = new MailHeaderDictionary();

                    if ((_mailHeaderDictionaryFile = await IOUtil.FileExists(_mailHeaderDictionaryFilePath)) == null)
                        _mailHeaderDictionaryFile = await IOUtil.GetCreateFile(_mailHeaderDictionaryFilePath, CreationCollisionOption.OpenIfExists);
                    else
                        MailHeaderDictionary = IOUtil.Deserialize<MailHeaderDictionary>(await FileIO.ReadTextAsync(_mailHeaderDictionaryFile));
                }
            }
            catch (Exception ex)
            {
                LogFile.Instance.LogError("", "", ex.ToString());
            }
        }

        /// <summary>
        /// Gets the account settings data dictionary.
        /// </summary>
        public static Dictionary<string, AccountSettingsData> AccountSettingsDataDictionary
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the mail header dictionary.
        /// </summary>
        public static MailHeaderDictionary MailHeaderDictionary
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets or sets a value indicating if the ui is suspended.
        /// </summary>
        public static bool IsUISuspended
        {
            get
            {
                // Create IsUISuspended setting
                if (!ApplicationData.Current.LocalSettings.Values.ContainsKey("IsUISuspended"))
                    ApplicationData.Current.LocalSettings.Values["IsUISuspended"] = false;

                return (bool)ApplicationData.Current.LocalSettings.Values["IsUISuspended"];
            }
            set
            {
                ApplicationData.Current.LocalSettings.Values["IsUISuspended"] = value;
                ApplicationData.Current.SignalDataChanged();
            }
        }

        /// <summary>
        /// Gets or sets a value indicating if the background task is running.
        /// </summary>
        public static bool IsTaskRunning
        {
            get
            {
                // Create IsUISuspended setting
                if (!ApplicationData.Current.LocalSettings.Values.ContainsKey("IsTaskRunning"))
                    ApplicationData.Current.LocalSettings.Values["IsTaskRunning"] = false;

                return (bool)ApplicationData.Current.LocalSettings.Values["IsTaskRunning"];
            }
            set
            {
                ApplicationData.Current.LocalSettings.Values["IsTaskRunning"] = value;
                ApplicationData.Current.SignalDataChanged();
            }
        }

        /// <summary>
        /// Saves the account settings data dictionary.
        /// </summary>
        public async static Task SaveAccountSettingsDataDictionary()
        {
            try
            {
                await _accountSettingsDataDictionaryMutex.WaitAsync();
                await FileIO.WriteTextAsync(_accountSettingsDataDictionaryFile, IOUtil.Serialise(AccountSettingsDataDictionary));
            }
            catch (Exception ex)
            {
                LogFile.Instance.LogError(_accountSettingsDataDictionaryFile.DisplayName, "", ex.ToString());
            }
            finally
            {
                _accountSettingsDataDictionaryMutex.Release();
            }
        }

        /// <summary>
        /// Saves the mail header dictionary.
        /// </summary>
        public async static Task SaveMailHeaderDictionary()
        {
            try
            {
                await _mailHeaderDictionaryMutex.WaitAsync();
                await FileIO.WriteTextAsync(_mailHeaderDictionaryFile, IOUtil.Serialise(MailHeaderDictionary));
            }
            catch (Exception ex)
            {
                LogFile.Instance.LogError(_mailHeaderDictionaryFile.DisplayName, "", ex.ToString());
            }
            finally
            {
                _mailHeaderDictionaryMutex.Release();
            }
        }
    }
}
