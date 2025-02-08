using Microsoft.Extensions.Logging;
using System.Text.RegularExpressions;
using System.Xml;
using Microsoft.Playwright;
using System.Globalization;
using System.Drawing;

/// <summary>
/// Manages SVG image operations, including exporting to PNG, resizing, and applying monochrome effects.
/// </summary>
public class SvgManager : IImageManager
{
    private readonly ILogger _logger;
    private IPage? page;
    private IPlaywright? playwrightContext;
    private IBrowser? browser;

    /// <summary>
    /// Initializes a new instance of the <see cref="SvgManager"/> class.
    /// </summary>
    /// <param name="logger">The logger.</param>
    public SvgManager(ILogger<SvgManager> logger)
    {
        _logger = logger;
    }

    private async Task Init()
    {
        // Ensure browsers are installed: https://playwright.dev/dotnet/docs/browsers#prerequisites-for-net
        Microsoft.Playwright.Program.Main(["install"]);
        
        try
        {
            playwrightContext = await Playwright.CreateAsync();
            browser = await playwrightContext.Chromium.LaunchAsync(new() { Headless = true });
            page = await browser.NewPageAsync();
        }
        catch(Exception ex)
        {
            _logger.LogCritical(ex.Message);
            if (OperatingSystem.IsLinux())
            {
                _logger.LogWarning("Ensure Linux dependencies are installed using the Microsoft.Playwright.CLI. See: https://playwright.dev/dotnet/docs/browsers#prerequisites-for-net");
            }
        }
    }

    /// <summary>
    /// Exports an SVG file to a PNG image.
    /// </summary>
    /// <param name="inputPath">The path to the input SVG file.</param>
    /// <param name="outputPath">The path to save the output PNG image.</param>
    /// <param name="targetImageHeight">The target height of the output image.</param>
    /// <param name="omitBackground">Whether to omit the background of the image.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains a boolean value indicating whether the export was successful.</returns>
    public async Task<bool> ExportToPng(string inputPath, string outputPath, int targetImageHeight, bool omitBackground = true)
    {
        var currentDirectory = Environment.CurrentDirectory;
        var rootDirectory = System.IO.Directory.GetParent(currentDirectory);

        var inputPathNormalized = inputPath.Replace(@"..\", "").Replace(@"../", "").Replace(@"\", @"/");
        var inputTokens = inputPathNormalized.Split(@"/");
        var inputFile = inputTokens.Last();

        var outputPathNormalized = outputPath.Replace(@"..\", "").Replace(@"../", "").Replace(@"\", @"/");
        var outputTokens = outputPathNormalized.Split(@"/");
        var outputFile = outputTokens.Last();

        var newInputPath = Path.Combine(rootDirectory!.FullName, inputTokens[0], inputTokens[1], inputFile);

        if (!OperatingSystem.IsWindows())
            newInputPath = "file://" + newInputPath;

        var newOutputPath = Path.Combine(rootDirectory.FullName, outputTokens[0],  outputTokens[1], outputFile);

        if (page == null)
            await Init();

        try
        {
            await page!.SetViewportSizeAsync(targetImageHeight, targetImageHeight);
            await page!.GotoAsync(newInputPath);
            var item = await page.QuerySelectorAsync("svg");
            await item!.ScreenshotAsync(new() { Path = newOutputPath, OmitBackground = omitBackground });
            return await Task.FromResult(true);
        }
        catch(Exception ex)
        {
            _logger.LogError($"Failed to export png {inputPath}  Message: {ex.Message}");
            return await Task.FromResult(false);
        }
    }

    /// <summary>
    /// Resizes and copies an SVG file.
    /// </summary>
    /// <param name="inputPath">The path to the input SVG file.</param>
    /// <param name="outputPath">The path to save the resized SVG file.</param>
    /// <param name="targetImageHeight">The target height of the image.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains a boolean value indicating whether the operation was successful.</returns>
    public async Task<bool> ResizeAndCopy(string inputPath, string outputPath, int targetImageHeight)
    {
        try
        {
            XmlDocument xmldoc = new XmlDocument();
            xmldoc.Load(inputPath);
            var svg = xmldoc.DocumentElement;
            if (svg == null)
            {
                throw new Exception("unable to load svg element from file");
            }

            // Check for a valid svg attribute in the document namespace.
            var docNamespace = svg.Attributes["xmlns"];

            if (docNamespace == null || docNamespace!.Value != "http://www.w3.org/2000/svg")
            {
                throw new Exception($"Invalid svg namespace attribute in {inputPath}. Confirm file has xmlns='http://www.w3.org/2000/svg' in the svg element.");
            }

            // Set document dimensions
            if (svg.Attributes["width"] == null)
                svg.Attributes.Append(xmldoc.CreateAttribute("width"));

            // svg.Attributes["width"]!.Value = targetImageHeight.ToString();
            svg.Attributes["width"]!.Value = "100%";

            if (svg.Attributes["height"] == null)
                svg.Attributes.Append(xmldoc.CreateAttribute("height"));

            svg.Attributes["height"]!.Value = targetImageHeight.ToString();
            
            // Set aspect ratio
            if (svg.Attributes["preserveAspectRatio"] == null)
                svg.Attributes.Append(xmldoc.CreateAttribute("preserveAspectRatio"));
            
            svg.Attributes["preserveAspectRatio"]!.Value = "xMidYMid meet";

            XmlWriter writer = XmlWriter.Create(outputPath);
            xmldoc.Save(writer);
            writer.Close();
            return await Task.FromResult(true);
        }
        catch (Exception ex)
        {
            _logger.LogCritical(ex.Message);
            throw;
        }
    }

    /// <summary>
    /// Exports an SVG file to a monochrome version.
    /// </summary>
    /// <param name="inputPath">The path to the input SVG file.</param>
    /// <param name="outputPath">The path to save the monochrome SVG file.</param>
    /// <param name="azureColor">The base color to use for the monochrome conversion.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains a boolean value indicating whether the export was successful.</returns>
    public async Task<bool> ExportToMonochrome(string inputPath, string outputPath, Color azureColor)
    {
        // Making it first grayscale and after that monochrom gives best result
        await ManipulateSvgFills(inputPath, outputPath, (color) =>
        {
            var grayScale = (int)((color.R * 0.3f) + (color.G * 0.59f) + (color.B * 0.11f));
            return Color.FromArgb(grayScale, grayScale, grayScale);
        });

        await ManipulateSvgFills(outputPath, outputPath, (color) =>
        {
            HSLColor lowerColor = new HSLColor(color);
            var upperColor = new HSLColor(azureColor) { Luminosity = lowerColor.Luminosity };
            return upperColor;
        });

        return await Task.FromResult(true);
    }

    private async Task<bool> ManipulateSvgFills(string inputPath, string outputPath, Func<Color, Color> manipulation)
    {
        var content = File.ReadAllText(inputPath);

        // get all hexidecimal colors in the SVG
        var hexColors = Regex.Matches(content, "(#[a-fA-F0-9]{6})(\")|(#[a-fA-F0-9]{3})(\")|(#[a-fA-F0-9]{6})(;)");

        foreach(var hexColor in hexColors)
        {
            var hexColorString = hexColor.ToString().Trim('"');
            hexColorString = hexColorString.Replace(";", "");
            var currentColor = System.Drawing.ColorTranslator.FromHtml(hexColorString);
            var newColor = manipulation(currentColor);
            content = content.Replace(hexColorString, newColor.ToHexString());
        }

        // Handle RGB colors
        var start = "rgb(";
        var end = ")";
        var delimiter = ',';
        var fillStartPos = content.IndexOf(start, 0);

        while (fillStartPos > 0)
        {
            var fillContentStartPos = fillStartPos + start.Length;
            var fillContentEndPos = content.IndexOf(end, fillContentStartPos);

            var rgbPerc = content
                .Substring(fillContentStartPos, fillContentEndPos - fillContentStartPos)
                .Split(delimiter);

            int rValue, gValue, bValue;
            if (rgbPerc[0].Contains('%'))
            {
                var rPercValue = float.Parse(rgbPerc[0].TrimEnd('%'), CultureInfo.InvariantCulture);
                var gPercValue = float.Parse(rgbPerc[1].TrimEnd('%'), CultureInfo.InvariantCulture);
                var bPercValue = float.Parse(rgbPerc[2].TrimEnd('%'), CultureInfo.InvariantCulture);

                rValue = (int)Math.Round(rPercValue / 100 * 255);
                gValue = (int)Math.Round(gPercValue / 100 * 255);
                bValue = (int)Math.Round(bPercValue / 100 * 255);    
            }
            else
            {
                rValue = int.Parse(rgbPerc[0]);
                gValue = int.Parse(rgbPerc[1]);
                bValue = int.Parse(rgbPerc[2]);
            }

            var currentColor = Color.FromArgb(rValue, gValue, bValue);

            var newColor = manipulation(currentColor);
            content = content.ReplaceAt(fillStartPos, fillContentEndPos + 1 - fillStartPos, $"rgb({newColor.R.ToString(CultureInfo.InvariantCulture)},{newColor.G.ToString(CultureInfo.InvariantCulture)},{newColor.B.ToString(CultureInfo.InvariantCulture)})");

            fillStartPos = content.IndexOf(start, fillContentEndPos);
        }

        await File.WriteAllTextAsync(outputPath, content);
        return await Task.FromResult(true);
    }

}
