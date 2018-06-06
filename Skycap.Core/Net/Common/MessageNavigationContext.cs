using System.Runtime.Serialization;

namespace Skycap.Net.Common
{
    /// <summary>
    /// Represents a message navigation context.
    /// </summary>
    [DataContract]
    internal class MessageNavigationContext
    {
        /// <summary>
        /// Initialises a new instance of the Skycap.Data.MessageNavigationContext class.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="navigationContext">The navigation context.</param>
        public MessageNavigationContext(StructuredMessage message, NavigationContext navigationContext)
        { 
            // Initialise local variables
            Message = message;
            NavigationContext = navigationContext;
        }

        /// <summary>
        /// Gets the message.
        /// </summary>
        [DataMember]
        public StructuredMessage Message
        {
            get;
            set;
        }

        /// <summary>
        /// Gets the navigation context.
        /// </summary>
        [DataMember]
        public NavigationContext NavigationContext
        {
            get;
            set;
        }
    }
}
