using System.Text.RegularExpressions;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;

/// <summary>
/// Provides functionality to generate VS Code snippets for PlantUML diagrams.
/// </summary>
public static class VSCodeSnippets
{
    /// <summary>
    /// Generates VS Code snippets based on the PlantUML files in the specified directory.
    /// </summary>
    /// <param name="distFolder">The directory containing the PlantUML files.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains a boolean value indicating whether the snippet generation was successful.</returns>
    public static async Task<bool> GenerateSnippets(string distFolder, ILogger logger)
    {
        logger.LogInformation("Generating VSCode Snippets...");

        var snippets = new Dictionary<string, Snippet>();

        try
        {
            foreach (var filePath in Directory.GetFiles(distFolder, "*.puml", SearchOption.AllDirectories))
            {
                var entityName = Path.GetFileNameWithoutExtension(filePath);
                if (entityName == "all" || snippets.ContainsKey(entityName))
                {
                    continue;
                }

                logger.LogInformation($"Processing PlantUML file: {filePath}");

                snippets.Add($"{entityName}", new Snippet{
                    prefix = $"{SplitCamelCase(entityName)}",
                    description = $"Add {SplitCamelCase(entityName)} to diagram",
                    body = new List<string>{
                        $"{entityName}(${{1:alias}}, \"${{2:label}}\", \"${{3:technology}}\")",
                        "$0"
                    }
                });

                snippets.Add($"{entityName}_Descr", new Snippet{
                    prefix = $"{SplitCamelCase(entityName)} with Description",
                    description = $"Add {SplitCamelCase(entityName)} with Description to diagram",
                    body = new List<string>{
                        $"{entityName}(${{1:alias}}, \"${{2:label}}\", \"${{3:technology}}\", \"${{4:description}}\")",
                        "$0"
                    }
                });
            }

            var snippetsDirectory = Path.Combine(distFolder, ".vscode", "snippets");
            Directory.CreateDirectory(snippetsDirectory);

            var options = new JsonSerializerOptions
            {
                WriteIndented = true,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
            };

            using (FileStream fileStream = File.Create(Path.Combine(snippetsDirectory, "diagram.json")))
            {
                await JsonSerializer.SerializeAsync(fileStream, snippets, options);
            }

            logger.LogInformation("VSCode Snippets generated successfully.");
            return await Task.FromResult(true);
        }
        catch (Exception ex)
        {
            logger.LogError($"Error generating VSCode Snippets: {ex.Message}");
            return await Task.FromResult(false);
        }
    }

    /// <summary>
    /// Splits a camel case string into separate words.
    /// </summary>
    /// <param name="camelCaseString">The camel case string to split.</param>
    /// <returns>The string with spaces inserted between the words.</returns>
    private static string SplitCamelCase(string camelCaseString) => Regex.Replace(camelCaseString, "(\\B[A-Z])", " $1");

    /// <summary>
    /// Represents a VS Code snippet.
    /// </summary>
    private class Snippet
    {
        /// <summary>
        /// Gets or sets the prefix for the snippet.
        /// </summary>
        public string? prefix { get; set; }

        /// <summary>
        /// Gets or sets the description for the snippet.
        /// </summary>
        public string? description { get; set; }

        /// <summary>
        /// Gets or sets the body for the snippet.
        /// </summary>
        public List<string>? body { get; set; }
    }

}



