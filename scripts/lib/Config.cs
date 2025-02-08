using System.Linq;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

/// <summary>
/// Represents a configuration lookup entry for an Azure service.
/// </summary>
public class ConfigLookupEntry
{
    /// <summary>
    /// Gets or sets the category of the service.
    /// </summary>
    public string? Category { get; set; }
    /// <summary>
    /// Gets or sets the source name of the service.
    /// </summary>
    public string? ServiceSource { get; set; }
    /// <summary>
    /// Gets or sets the target name of the service.
    /// </summary>
    public string? ServiceTarget { get; set; }
    /// <summary>
    /// Gets or sets a value indicating whether the service icon should fit to canvas.
    /// </summary>
    public bool FitToCanvas { get; set; }
}

/// <summary>
/// Represents the configuration for Azure services.
/// </summary>
public class Config
{
    /// <summary>
    /// Gets or sets the list of Azure categories.
    /// </summary>
    public List<AzureCategory>? Categories { get; set; }

    /// <summary>
    /// Reads the configuration from a YAML file.
    /// </summary>
    /// <param name="configFilePath">The path to the configuration file.</param>
    /// <returns>A collection of configuration lookup entries.</returns>
    public static IEnumerable<ConfigLookupEntry> ReadConfig(string configFilePath)
    {
        Config config;
        using (var input = File.OpenText("Config.yaml"))
        {
            var deserializer = new DeserializerBuilder()
                .Build();

            config = deserializer.Deserialize<Config>(input);
        }

        var lookupTable = config.Categories!.SelectMany(cat => cat.Services!.Select(service => new ConfigLookupEntry
        {
            Category = cat.Name,
            ServiceSource = service.Source,
            ServiceTarget = service.Target,
            FitToCanvas = service.Fit
        }));

        return lookupTable;
    }
}

/// <summary>
/// Represents a category of Azure services.
/// </summary>
public class AzureCategory
{
    /// <summary>
    /// Gets or sets the name of the category.
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    /// Gets or sets the list of services in the category.
    /// </summary>
    public List<AzureService>? Services { get; set; }
}

/// <summary>
/// Represents an Azure service.
/// </summary>
public class AzureService
{
    /// <summary>
    /// Gets or sets the source name of the service.
    /// </summary>
    public string? Source { get; set; }

    /// <summary>
    /// Gets or sets the target name of the service.
    /// </summary>
    public string? Target { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the service icon should fit to canvas.
    /// </summary>
    public bool Fit { get; set; }
}





