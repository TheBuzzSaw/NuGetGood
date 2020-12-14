using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;

namespace NuGetGood
{
    class Program
    {
        static async Task ScanPackageAsync(
            HttpClient httpClient,
            string package,
            string version,
            bool enableColor,
            CancellationToken cancellationToken = default)
        {
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.Write("  - ");
            Console.Write(package);
            Console.Write(" -> ");

            var id = package.ToLowerInvariant();
            var url = $"https://api.nuget.org/v3-flatcontainer/{id}/index.json";

            using (var response = await httpClient.GetAsync(url, cancellationToken))
            {
                if (response.IsSuccessStatusCode)
                {
                    var list = await response.Content.ReadFromJsonAsync<VersionList>(cancellationToken: cancellationToken);
                    var current = string.IsNullOrWhiteSpace(version) || version == "*" ? "unspecified" : version;
                    var hasDash = current.Contains('-');
                    var latest = list.Versions.Last(v => hasDash || !v.Contains('-'));
                    if (current != latest)
                    {
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.Write(current);
                        Console.Write(" to ");
                        Console.WriteLine(latest);
                    }
                    else
                    {
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine(current);
                    }
                }
                else
                {
                    var statusCode = (int)response.StatusCode;
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"error ({statusCode})");
                }
            }
        }

        static async Task ScanProjectAsync(
            HttpClient httpClient,
            string projectPath,
            bool enableColor,
            CancellationToken cancellationToken = default)
        {
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine(projectPath);
            var xmlReaderSettings = new XmlReaderSettings
            {
                Async = true
            };
            using var fileStream = File.OpenRead(projectPath);
            using var xmlReader = XmlReader.Create(fileStream, xmlReaderSettings);

            while (await xmlReader.ReadAsync())
            {
                if (xmlReader.NodeType == XmlNodeType.Element && xmlReader.Name == "PackageReference")
                {
                    var package = xmlReader.GetAttribute("Include");
                    var version = xmlReader.GetAttribute("Version");

                    if (!string.IsNullOrWhiteSpace(package))
                    {
                        await ScanPackageAsync(
                            httpClient,
                            package,
                            version,
                            enableColor,
                            cancellationToken);
                    }
                }
            }

        }

        static async Task<int> Main(string[] args)
        {
            var enableColor = true;

            try
            {
                using var httpClient = new HttpClient();
                // var index = await httpClient.GetFromJsonAsync<NuGetIndex>("https://api.nuget.org/v3/index.json");

                // var baseUrl = index.Resources.First(r => r.Type == "SearchQueryService").Id;
                // var url = $"{baseUrl}?q=Extensions&skip=0&take=8"; 
                // var result = await httpClient.GetStringAsync(url);
                // Console.WriteLine(result);
                
                // var id = "Microsoft.Extensions.Logging.Abstractions".ToLowerInvariant();
                // var url = $"https://api.nuget.org/v3-flatcontainer/{id}/index.json";
                // var result = await httpClient.GetStringAsync(url);
                // Console.WriteLine(result);

                foreach (var arg in args)
                {
                    await ScanProjectAsync(httpClient, arg, enableColor);
                }

                return 0;
            }
            catch (Exception ex)
            {
                if (enableColor)
                    Console.ForegroundColor = ConsoleColor.Red;
                
                Console.WriteLine();
                Console.WriteLine(ex);
                
                return 1;
            }
            finally
            {
                if (enableColor)
                    Console.ResetColor();
            }
        }
    }
}
