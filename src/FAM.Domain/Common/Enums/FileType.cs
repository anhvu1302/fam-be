namespace FAM.Domain.Common.Enums;

/// <summary>
/// Supported file types in the system
/// </summary>
public enum FileType
{
    /// <summary>
    /// Image files (jpg, png, gif, webp, bmp)
    /// </summary>
    Image = 1,

    /// <summary>
    /// Media files (video, audio)
    /// </summary>
    Media = 2,

    /// <summary>
    /// Document files (pdf, doc, xls, ppt, txt, csv)
    /// </summary>
    Document = 3
}