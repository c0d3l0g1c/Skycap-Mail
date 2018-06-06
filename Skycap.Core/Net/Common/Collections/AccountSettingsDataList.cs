using System;
using System.Collections.ObjectModel;
using System.Linq;

namespace Skycap.Data
{
    /// <summary>
    /// Represents the account settings list.
    /// </summary>
    public class AccountSettingsDataList : ObservableCollection<AccountSettingsData>
    {
        /// <summary>
        /// Initialises a new instance of the 
        /// </summary>
        public AccountSettingsDataList()
            : base()
        { 
        
        }

        /// <summary>
        /// Gets account settings data by email address.
        /// </summary>
        /// <param name="emailAddress">The email address.</param>
        /// <returns>Account settings data.</returns>
        public AccountSettingsData this[string emailAddress]
        {
            get
            { 
                return (from accountSettingsData in this
                        where (accountSettingsData.EmailAddress == emailAddress)
                        select accountSettingsData)
                       .FirstOrDefault();
            }
        }
    }
}
