using System;

/// <summary>
/// Represents the various enumerations for the download emails from options.
/// </summary>
public enum DownloadEmailsFromOptions
{ 
    /// <summary>
    /// Indicates emails from the last 3 days should be downloaded.
    /// </summary>
    TheLast3Days = 3,
    /// <summary>
    /// Indicates emails from the last 7 days should be downloaded.
    /// </summary>
    TheLast7Days = 7,
    /// <summary>
    /// Indicates emails from the last 2 weeks should be downloaded.
    /// </summary>
    TheLast2Weeks = 14,
    /// <summary>
    /// Indicates emails from the last month should be downloaded.
    /// </summary>
    TheLastMonth = 30,
    /// <summary>
    /// Indicates emails from anytime should be downloaded.
    /// </summary>
    Anytime = 0
}