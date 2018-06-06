using System;

/// <summary>
/// Represents the various enumerations for the download new email options.
/// </summary>
public enum DownloadNewEmailOptions
{ 
    /// <summary>
    /// Indicates that items are downloaded as they arrive.
    /// </summary>
    AsItemsArrive = 1,
    /// <summary>
    /// Indicates that items are downloaded every 15 minutes.
    /// </summary>
    Every15Minutes = 15,
    /// <summary>
    /// Indicates that items are downloaded every 30 minutes.
    /// </summary>
    Every30Minutes = 30,
    /// <summary>
    /// Indicates that items are downloaded every hour.
    /// </summary>
    Hourly = 60,
    /// <summary>
    /// Indicates that items are downloaded on demand.
    /// </summary>
    Manual = 0
}