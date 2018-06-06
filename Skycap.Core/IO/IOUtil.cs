using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using HtmlAgilityPack;
using Skycap.Net.Common;
using Skycap.Net.Common.MessageParts;
using Skycap.Net.Imap;
using Windows.Security.Cryptography;
using Windows.Storage;
using Windows.Storage.Streams;

namespace Skycap.IO
{
    /// <summary>
    /// Represents an IO utility class.
    /// </summary>
    public static class IOUtil
    {
        /// <summary>
        /// Regex pattern for UTF-8 encoded string.
        /// </summary>
        private static readonly Regex Utf8Regex = new Regex(string.Format("^{0}(.+){1}$", Regex.Escape("=?utf-8?B?"), Regex.Escape("?=")), RegexOptions.IgnoreCase);
        /// <summary>
        /// Regex pattern for Iso encoded string.
        /// </summary>
        private static readonly Regex IsoRegex = new Regex(string.Format("^{0}(.+){1}$", Regex.Escape("=?iso-8859-1?Q?"), Regex.Escape("?=")), RegexOptions.IgnoreCase);
        /// <summary>
        /// Regex pattern for invalid folder name.
        /// </summary>
        private static readonly Regex InvalidFolderNameRegex = new Regex(@"[" + string.Join("", Path.GetInvalidFileNameChars()) + "]+");
        /// <summary>
        /// Regex pattern for splitting alphanumerics into words.
        /// </summary>
        private static readonly Regex AlphaNumericPascalCaseRegex = new Regex(@"(?<=[A-Z])(?=[A-Z][a-z])|(?<=[^A-Z])(?=[A-Z])|(?<=[A-Za-z])(?=[^A-Za-z])");

        /// <summary>
        /// Serialises the specified object instance.
        /// </summary>
        /// <param name="instance">The object instance.</param>
        public static string Serialise(object instance)
        {
            using (MemoryStream memoryStream = new MemoryStream())
            {
                DataContractSerializer dataContractSerializer = new DataContractSerializer(instance.GetType());
                dataContractSerializer.WriteObject(memoryStream, instance);
                memoryStream.Position = 0;
                using (StreamReader streamReader = new StreamReader(memoryStream, Encoding.UTF8))
                {
                    return streamReader.ReadToEnd();
                }
            }
        }

        /// <summary>
        /// Deserialises the specified string to an object instance.
        /// </summary>
        /// <typeparam name="T">The System.Type to deserialise to.</typeparam>
        /// <param name="serialisedObject">The serialised object string.</param>
        /// <returns>Deserialised instance of T.</returns>
        public static T Deserialize<T>(string serialisedObject)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(serialisedObject);
            using (MemoryStream memoryStream = new MemoryStream(bytes))
            {
                DataContractSerializer dataContractSerializer = new DataContractSerializer(typeof(T));
                return (T)dataContractSerializer.ReadObject(memoryStream);
            }
        }

        /// <summary>
        /// Deserialises the file at the specified path to an object instance.
        /// </summary>
        /// <typeparam name="T">The System.Type to deserialise to.</typeparam>
        /// <param name="path">The path of file to deserialise.</param>
        /// <returns>Deserialised instance of T.</returns>
        public async static Task<T> DeserializeFromPath<T>(string path)
        {
            StorageFile storageFile = await StorageFile.GetFileFromPathAsync(path);
            return await DeserializeFromFile<T>(storageFile);
        }

        /// <summary>
        /// Deserialises the file at the specified path to an object instance.
        /// </summary>
        /// <typeparam name="T">The System.Type to deserialise to.</typeparam>
        /// <param name="storageFile">The file to deserialise.</param>
        /// <returns>Deserialised instance of T.</returns>
        public async static Task<T> DeserializeFromFile<T>(StorageFile storageFile)
        {
            string text = await FileIO.ReadTextAsync(storageFile);
            return Deserialize<T>(text);
        }

        /// <summary>
        /// Determines if a file at the specified exists.
        /// </summary>
        /// <param name="path">The path of the file.</param>
        /// <returns>true if exists; otherwise, false.</returns>
        public async static Task<StorageFile> FileExists(string path)
        {
            try
            {
                return await StorageFile.GetFileFromPathAsync(path);
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Loads a message from the specified path.
        /// </summary>
        /// <param name="isImapMessage">true if imap message; otherwise, false.</param>
        /// <param name="path">The path.</param>
        /// <returns>A message.</returns>
        public static async Task<StructuredMessage> LoadMessage(bool isImapMessage, string path)
        {
            if (isImapMessage)
                return await IOUtil.DeserializeFromPath<ImapMessage>(path);
            return await IOUtil.DeserializeFromPath<PopMessage>(path);
        }

        /// <summary>
        /// Loads a message from the specified path.
        /// </summary>
        /// <param name="isImapMessage">true if imap message; otherwise, false.</param>
        /// <param name="storageFile">The storage file.</param>
        /// <returns>A message.</returns>
        public static async Task<StructuredMessage> LoadMessage(bool isImapMessage, StorageFile storageFile)
        {
            if (isImapMessage)
                return await IOUtil.DeserializeFromFile<ImapMessage>(storageFile);
            return await IOUtil.DeserializeFromFile<PopMessage>(storageFile);
        }

        /// <summary>
        /// Formats a given name to a valid file system name.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns>A file system name.</returns>
        public static string FormatFileSystemName(string name)
        {
            Match match = Utf8Regex.Match(name);
            if (match.Success)
                name = MailMessageRFCDecoder.GetStringFromBase64(match.Groups[1].Value, "UTF-8");
            else if ((match = IsoRegex.Match(name)).Success)
                name = MailMessageRFCDecoder.GetStringFromQuotedPrintable(match.Groups[1].Value, "iso-8859-1");
            Regex invalidFileSystemNameRegex = new Regex(@"[" + string.Join("", Path.GetInvalidFileNameChars()) + "]+");
            if (invalidFileSystemNameRegex.IsMatch(name))
                return invalidFileSystemNameRegex.Replace(name, "");
            return name;
        }

        /// <summary>
        /// Gets or creates the specified folder name in a given root folder.
        /// </summary>
        /// <param name="rootFolder">The root folder.</param>
        /// <param name="folderName">The folder name.</param>
        /// <returns>A storage folder.</returns>
        private static async Task<StorageFolder> GetCreateFolder(StorageFolder rootFolder, string folderName)
        {
            StorageFolder folder = null;
            Match match = null;
            if ((match = InvalidFolderNameRegex.Match(folderName)).Success)
                folderName = Regex.Replace(folderName, match.Groups[0].Value, "");
            try
            {
                folder = await rootFolder.GetFolderAsync(folderName);
            }
            catch (FileNotFoundException) { }

            if (folder == null)
                folder = await rootFolder.CreateFolderAsync(folderName);

            return folder;
        }

        /// <summary>
        /// Gets or creates the specified file name in a given root folder.
        /// </summary>
        /// <param name="rootFolder">The root folder.</param>
        /// <param name="folderName">The file name.</param>
        /// <param name="creationCollisionOption">The creation collision option.</param>
        /// <returns>A storage folder.</returns>
        public static async Task<StorageFile> GetCreateFile(string path, CreationCollisionOption creationCollisionOption)
        {
            string fileName = Path.GetFileName(path);
            StorageFolder rootFolder = await GetCreateFolder(path.Replace(fileName, ""), FolderType.Message);
            return await GetCreateFile(rootFolder, fileName, creationCollisionOption);
        }

        /// <summary>
        /// Gets or creates the specified file name in a given root folder.
        /// </summary>
        /// <param name="rootFolder">The root folder.</param>
        /// <param name="folderName">The file name.</param>
        /// <param name="creationCollisionOption">The creation collision option.</param>
        /// <returns>A storage folder.</returns>
        public static async Task<StorageFile> GetCreateFile(StorageFolder rootFolder, string fileName, CreationCollisionOption creationCollisionOption)
        {
            return await rootFolder.CreateFileAsync(fileName, CreationCollisionOption.OpenIfExists);
        }

        /// <summary>
        /// Gets or creates the storage folder from the given path.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <param name="folderType">The folder type.</param>
        /// <returns>A storage folder representing the path.</returns>
        public async static Task<StorageFolder> GetCreateFolder(string path, FolderType folderType)
        {
            StorageFolder storageFolder = ApplicationData.Current.LocalFolder;

            path = path.Replace(storageFolder.Path, "");
            string[] folders = path.Split(new char[] { '\\', '/' }, StringSplitOptions.RemoveEmptyEntries);
            string folder;
            string folderPrefix;

            for (int i = 0; i < folders.Length; i++)
            {
                folder = folders[i];
                folderPrefix = null;

                if (folder.StartsWith(MailClient.MailboxFolderPrefix.ToString())
                 || folder.StartsWith(MailClient.MessageFolderPrefix.ToString()))
                    goto Create;

                switch (i)
                { 
                    case 0:
                        break;

                    default:
                        if (i == folders.Length - 1)
                        {
                            switch (folderType)
                            { 
                                case FolderType.Mailbox:
                                    folderPrefix = MailClient.MailboxFolderPrefix.ToString();
                                    break;

                                case FolderType.Message:
                                    folderPrefix = MailClient.MessageFolderPrefix.ToString();
                                    break;

                                default:
                                    break;
                            }
                        }
                        else
                            folderPrefix = MailClient.MailboxFolderPrefix.ToString();
                        break;
                }

                Create:
                storageFolder = await GetCreateFolder(storageFolder, folderPrefix + folder);
            }

            return storageFolder;
        }

        /// <summary>
        /// Transforms the specified file to a base 64 image string.
        /// </summary>
        /// <param name="filename">The file name.</param>
        /// <returns>Returns a Base 64 image string.</returns>
        private async static Task<string> TransformFileToBase64ImageString(string filename)
        {
            StorageFile storageFile = await StorageFile.GetFileFromPathAsync(filename);
            IBuffer buffer = await FileIO.ReadBufferAsync(storageFile);
            byte[] bytes = null;
            CryptographicBuffer.CopyToByteArray(buffer, out bytes);
            return "data:image/png;base64," + Convert.ToBase64String(bytes);
        }

        /// <summary>
        /// Gets the formatted html for the specified message.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <returns>The formatted html.</returns>
        public static async Task<string> FormattedHtml(MailMessage message)
        {
            try
            {
                // Load the html
                HtmlDocument htmlDocument = new HtmlDocument();
                htmlDocument.OptionFixNestedTags = true;
                string html = (message.TextContentType == ETextContentType.Html ? message.Text : string.Format("<p>{0}</p>", (message.Text + string.Empty).Replace(Environment.NewLine, "<br/>")));
                htmlDocument.LoadHtml(html);

                // Get the link nodes
                IEnumerable<HtmlNode> linkNodes = htmlDocument.DocumentNode.Descendants("a")
                                                 .Where(o => !string.IsNullOrEmpty(o.GetAttributeValue("href", null))
                                                          && (o.GetAttributeValue("href", null).StartsWith("http", StringComparison.OrdinalIgnoreCase)
                                                           || o.GetAttributeValue("href", null).StartsWith("www", StringComparison.OrdinalIgnoreCase)));

                // Loop through each external link - ensure it opens in new window
                foreach (HtmlNode linkNode in linkNodes)
                {
                    if (linkNode.Attributes.Contains("target"))
                        linkNode.Attributes["target"].Value = "_blank";
                    else
                        linkNode.Attributes.Add("target", "_blank");
                }

                // Get the image nodes
                IEnumerable<HtmlNode> imageNodes = htmlDocument.DocumentNode.Descendants("img")
                                                  .Where(o => !string.IsNullOrEmpty(o.GetAttributeValue("src", null))
                                                           && (!o.GetAttributeValue("src", null).StartsWith("http", StringComparison.OrdinalIgnoreCase)
                                                            || !o.GetAttributeValue("src", null).StartsWith("www", StringComparison.OrdinalIgnoreCase)));

                // Loop through each local image
                foreach (HtmlNode imageNode in imageNodes)
                {
                    try
                    {
                        // Find the image attachment
                        string srcWithoutCid = imageNode.GetAttributeValue("src", null).Replace("cid:", "");
                        Attachment attachment = message.Attachments[srcWithoutCid];

                        // If found
                        if (attachment != null)
                        {
                            // Convert image to base64
                            StorageFile attachmentFile = await IOUtil.GetCreateFile(attachment.FullFilename, CreationCollisionOption.ReplaceExisting);
                            imageNode.Attributes["src"].Value = await TransformFileToBase64ImageString(attachmentFile.Path);
                        }
                    }
                    catch (Exception ex)
                    {
                        LogFile.Instance.LogError("", "", ex.ToString());
                    }
                }

                // Ensure that the html node exists
                HtmlNode htmlNode = htmlDocument.DocumentNode.Descendants("html").FirstOrDefault();
                if (htmlNode == null)
                {
                    htmlNode = htmlDocument.CreateElement("html");
                    htmlDocument.DocumentNode.AppendChild(htmlNode);
                }

                // Ensure that the head node exists
                HtmlNode headNode = htmlDocument.DocumentNode.Descendants("head").FirstOrDefault();
                if (headNode == null)
                {
                    headNode = htmlDocument.CreateElement("head");
                    htmlNode.AppendChild(headNode);
                }
                
                // Create page css transition
                HtmlNode cssTransitionNode = htmlDocument.CreateElement("style");
                cssTransitionNode.InnerHtml = "body{opacity:0;transition: all 2s ease;}.loaded{opacity:1;}";
                headNode.PrependChild(cssTransitionNode);

                // Create page javascript transition
                HtmlNode javascriptTransitionNode = htmlDocument.CreateElement("script");
                javascriptTransitionNode.Attributes.Add("type", "text/javascript");
                javascriptTransitionNode.InnerHtml = "document.addEventListener('DOMContentLoaded', function () { document.body.classList.add('loaded'); }, false);";
                headNode.AppendChild(javascriptTransitionNode);

                // Ensure that the body node exists
                HtmlNode bodyNode = htmlDocument.DocumentNode.Descendants("body").FirstOrDefault();
                if (bodyNode == null)
                {
                    bodyNode = htmlDocument.CreateElement("body");
                    htmlNode.AppendChild(bodyNode);
                }

                // Add the body tags
                HtmlNodeCollection htmlNodes = new HtmlNodeCollection(bodyNode);
                foreach (HtmlNode node in htmlDocument.DocumentNode.ChildNodes.ToList())
                {
                    if (!node.Name.Equals("html", StringComparison.OrdinalIgnoreCase)
                     && !node.Name.Equals("head", StringComparison.OrdinalIgnoreCase)
                     && !node.Name.Equals("body", StringComparison.OrdinalIgnoreCase))
                    {
                        htmlNodes.Add(node);
                        htmlDocument.DocumentNode.RemoveChild(node);
                    }
                }
                bodyNode.AppendChildren(htmlNodes);

                // Return the html
                return htmlDocument.DocumentNode.InnerHtml;
            }
            catch(Exception ex)
            {
                LogFile.Instance.LogError("", "", ex.ToString());
                return message.Text;
            }
        }

        /// <summary>
        /// Replaces the contents of href by wrapping the content in a javacript notify function.
        /// </summary>
        /// <param name="match">The match.</param>
        /// <returns>The href content wrapped by javascript notify.</returns>
        private static string HrefReplace(Match match)
        {
            return string.Format("{0} target=\"_blank\"", match.Value);
        }

        /// <summary>
        /// Splits a string by capital letters and adds a space between the words that start with capital letters.
        /// </summary>
        /// <param name="string">The string to split into words.</param>
        /// <returns>Returns a space separated string of words.</returns>
        public static string ToWords(this string @string)
        {
            if (@string.Length > 0)
            {
                if (char.IsLower(@string[0]))
                    @string = @string[0].ToString().ToUpper() + @string.Substring(1, @string.Length - 1);
            }
            return AlphaNumericPascalCaseRegex.Replace(@string, " ");
        }

        /// <summary>
        /// Gets the list of stored email addresses.
        /// </summary>
        /// <param name="emailAddress">The email address.</param>
        /// <returns>The list of stored email addresses.</returns>
        public static async Task<List<EmailAddress>> GetStoredEmailAddresses(string emailAddress)
        {
            try
            {
                List<EmailAddress> emailAddresses = null;
                StorageFile emailAddressFile = await FileExists(Path.Combine(ApplicationData.Current.LocalFolder.Path, emailAddress, "ea"));
                if (emailAddressFile != null)
                    emailAddresses = await IOUtil.DeserializeFromFile<List<EmailAddress>>(emailAddressFile);
                return (emailAddresses == null ? new List<EmailAddress>() : emailAddresses);
            }
            catch (Exception ex)
            {
                LogFile.Instance.LogError("", "", ex.ToString());
                return new List<EmailAddress>();
            }
        }

        /// <summary>
        /// Saves the list of stored email addresses.
        /// </summary>
        /// <param name="emailAddress">The email address.</param>
        public static async Task SaveStoredEmailAddresses(string emailAddress, List<EmailAddress> storedEmailAddresses)
        {
            try
            {
                StorageFile emailAddressFile = await IOUtil.GetCreateFile(Path.Combine(ApplicationData.Current.LocalFolder.Path, emailAddress, "ea"), CreationCollisionOption.ReplaceExisting);
                await FileIO.WriteTextAsync(emailAddressFile, IOUtil.Serialise(storedEmailAddresses));
            }
            catch (Exception ex)
            {
                LogFile.Instance.LogError("", "", ex.ToString());
            }
        }

        /// <summary>
        /// Gets the formatted mailbox name.
        /// </summary>
        /// <param name="mailboxFullName">The mailbox full name.</param>
        /// <returns>The formatted mailbox name.</returns>
        public static string GetFormattedMailboxName(string mailboxFullName)
        {
            string[] folders = mailboxFullName.Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
            return string.Join(@"\", folders.Select(o => MailClient.MailboxFolderPrefix + o)).Trim('\\');
        }

        /// <summary>
        /// Gets the mailbox name from the specified path.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <returns>The mailbox name.</returns>
        public static string GetMailboxNameFromPath(string path)
        {
            return string.Join("/", path.Split('\\')
                                        .Where(o => o.StartsWith(MailClient.MailboxFolderPrefix.ToString()))
                                        .Select(o => o.TrimStart(MailClient.MailboxFolderPrefix)));
        }

        /// <summary>
        /// Gets the account directory.
        /// </summary>
        /// <param name="emailAddress">The email address.</param>
        /// <returns>The account directory.</returns>
        public static string GetAccountPath(string emailAddress)
        {
            return Path.Combine(ApplicationData.Current.LocalFolder.Path, emailAddress);
        }

        /// <summary>
        /// Gets the mailbox directory.
        /// </summary>
        /// <param name="emailAddress">The email address.</param>
        /// <param name="mailboxFullName">The mailbox full name.</param>
        /// <returns>The mailbox directory.</returns>
        public static string GetMailboxPath(string emailAddress, string mailboxFullName)
        {
            return Path.Combine(GetAccountPath(emailAddress), GetFormattedMailboxName(mailboxFullName));
        }

        /// <summary>
        /// Gets the message directory.
        /// </summary>
        /// <param name="emailAddress">The email address.</param>
        /// <param name="mailboxFullName">The mailbox full name.</param>
        /// <param name="uid">The message uid.</param>
        /// <returns>The message directory.</returns>
        public static string GetMessagePath(string emailAddress, string mailboxFullName, string uid)
        {
            return Path.Combine(GetMailboxPath(emailAddress, mailboxFullName), MailClient.MessageFolderPrefix + uid);
        }

        /// <summary>
        /// Gets the message path.
        /// </summary>
        /// <param name="emailAddress">The email address.</param>
        /// <param name="mailboxFullName">The mailbox full name.</param>
        /// <param name="uid">The message id.</param>
        /// <returns>The message path.</returns>
        public static string GetMessageFullPath(string emailAddress, string mailboxFullName, string uid)
        {
            return Path.Combine(GetMessagePath(emailAddress, mailboxFullName, uid), MailMessage.Filename);
        }

        /// <summary>
        /// Trims the message body.
        /// </summary>
        /// <param name="body">The body.</param>
        /// <returns>The trimmed body.</returns>
        public static string TrimBody(string body)
        {
            body += " ";
            return body.Substring(0, Math.Min(body.Length, 50));
        }

        /// <summary>
        /// Gets a message size in terms of bytes from the specified long size.
        /// </summary>
        /// <param name="size">The long size.</param>
        /// <returns>The message size.</returns>
        public static string GetMessageSize(double size)
        {
            if (size >= 0 && size < 1024)
                return string.Format("{0} Bytes", Math.Round(size, 2).ToString("N2"));
            else if (size >= 1024 && size < 1048576)
                return string.Format("{0} KB", Math.Round(size / 1024, 2).ToString("N2"));
            else if (size >= 1048576 && size < 1073741824)
                return string.Format("{0} MB", Math.Round(size / 1048576, 2).ToString("N2"));
            else
                return string.Format("{0} GB", Math.Round(size / 1073741824, 2).ToString("N2"));
        }
    }
}
