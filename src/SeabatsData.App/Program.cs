using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Threading.Tasks;
using GeoJSON.Net.Geometry;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;

namespace SeabatsData.App
{
    class Program
    {
        public static async Task Main(string[] args)
        {
            var configuration = new ConfigurationBuilder()
                .AddEnvironmentVariables()
                .Build();

            WriteHeading("Configuration");

            var inputDir = configuration["SeabatsData:InputDir"];
            var outputDir = configuration["SeabatsData:OutputDir"];
            var icaos = configuration["SeabatsData:ICAOs"];

            Console.WriteLine($"Input Directory:  {inputDir}");
            Console.WriteLine($"Output Directory: {outputDir}");
            Console.WriteLine($"ICAOs:            {icaos}");

            if (!Directory.Exists(outputDir))
                Directory.CreateDirectory(outputDir);

            WriteHeading("Files");

            var files = Directory.GetFiles(inputDir, "*.*", SearchOption.AllDirectories);
            var gzFiles = Directory.GetFiles(inputDir, "*.gz", SearchOption.AllDirectories);
            var jsonGzFiles = Directory.GetFiles(inputDir, "*.json.gz", SearchOption.AllDirectories);
            var csvGzFiles = Directory.GetFiles(inputDir, "*.csv.gz", SearchOption.AllDirectories);

            Console.WriteLine($"Files:           {files.Length}");
            Console.WriteLine($"*.gz Files:      {gzFiles.Length}");
            Console.WriteLine($"*.json.gz Files: {jsonGzFiles.Length}");
            Console.WriteLine($"*.csv.gz Files:  {csvGzFiles.Length}");
            
            foreach (var icao in icaos.Split(","))
            {
                WriteHeading($"Exporting traces of ICAO: #{icao}...");

                var icaoFiles = Directory.GetFiles(inputDir, $"*{icao}*", SearchOption.AllDirectories);

                var flights = new List<GeoJSON.Flight>();

                foreach (var file in icaoFiles)
                {
                    if (!file.EndsWith(".json.gz")) continue;

                    await using var gzStream = File.OpenRead(file);
                    await using var srcStream = new GZipStream(gzStream, CompressionMode.Decompress);
                    using var streamReader = new StreamReader(srcStream);
                    var fileContent = await streamReader.ReadToEndAsync();

                    var adsbExchangeFlight = new AdsbExchange.Flight(fileContent);

                    if (adsbExchangeFlight.IsInvalid) continue;

                    var geoJson = new GeoJSON.Flight(
                        new MultiPoint(adsbExchangeFlight.Coords),
                        new GeoJSON.FlightProperties
                        {
                            Icao = adsbExchangeFlight.Icao,
                            Duration = adsbExchangeFlight.DurationSeconds,
                            Label = GetLabel(adsbExchangeFlight.Icao)
                        });

                    flights.Add(geoJson);
                }

                var serialized = JsonConvert.SerializeObject(flights);
                await File.WriteAllTextAsync(Path.Combine(outputDir, $"{icao}.json"), serialized);
            }
        }

        private static string GetLabel(string icao)
        {
            // TODO move to where it belongs
            icao = icao.ToUpper();
            return icao switch
            {
                "406658" => "G-DMPP",
                "407637" => "G-WKTH",
                "40785B" => "G-WKTI",
                "43ED4B" => "2-WKTJ",
                "4D232D" => "HRON",
                _ => throw new InvalidOperationException($"Unknown ICAO: #{icao}")
            };
        }

        private static void WriteHeading(string heading)
        {
            Console.WriteLine();
            Console.WriteLine(heading.ToUpper());
            Console.WriteLine();
        }
    }
}
