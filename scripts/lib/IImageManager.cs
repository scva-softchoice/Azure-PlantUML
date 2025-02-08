using System.Drawing;

/// <summary>
/// Defines an interface for managing image operations.
/// </summary>
public interface IImageManager
{
    /// <summary>
    /// Exports an image to PNG format.
    /// </summary>
    /// <param name="inputPath">The path to the input image.</param>
    /// <param name="outputPath">The path to save the output PNG image.</param>
    /// <param name="targetImageHeight">The target height of the output image.</param>
    /// <param name="omitBackground">Whether to omit the background of the image.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains a boolean value indicating whether the export was successful.</returns>
    Task<bool> ExportToPng(string inputPath, string outputPath, int targetImageHeight, bool omitBackground = true);

    /// <summary>
    /// Resizes and copies an image.
    /// </summary>
    /// <param name="inputPath">The path to the input image.</param>
    /// <param name="outputPath">The path to save the resized image.</param>
    /// <param name="targetImageHeight">The target height of the image.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains a boolean value indicating whether the operation was successful.</returns>
    Task<bool> ResizeAndCopy(string inputPath, string outputPath, int targetImageHeight);

    /// <summary>
    /// Exports an image to a monochrome version.
    /// </summary>
    /// <param name="inputPath">The path to the input image.</param>
    /// <param name="outputPath">The path to save the monochrome image.</param>
    /// <param name="color">The base color to use for the monochrome conversion.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains a boolean value indicating whether the export was successful.</returns>
    Task<bool> ExportToMonochrome(string inputPath, string outputPath, Color color);
}
