using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Threading.Tasks;
using GeoJSON.Net.Feature;
using GeoJSON.Net.Geometry;
using Newtonsoft.Json;
using SeabatsData.App.AdsbExchange;
using SeabatsData.Core;

namespace SeabatsData.App
{
    internal class Program : ProgramBase
    {
        public static async Task Main(string[] args)
        {
            WriteHeading("Files");

            var files = Directory.GetFiles(InputDir, "*.*", SearchOption.AllDirectories);
            var gzFiles = Directory.GetFiles(InputDir, "*.gz", SearchOption.AllDirectories);
            var jsonGzFiles = Directory.GetFiles(InputDir, "*.json.gz", SearchOption.AllDirectories);
            var csvGzFiles = Directory.GetFiles(InputDir, "*.csv.gz", SearchOption.AllDirectories);

            Console.WriteLine($"Files:           {files.Length}");
            Console.WriteLine($"*.gz Files:      {gzFiles.Length}");
            Console.WriteLine($"*.json.gz Files: {jsonGzFiles.Length}");
            Console.WriteLine($"*.csv.gz Files:  {csvGzFiles.Length}");

            var flights = new FeatureCollection();

            foreach (var icao in Icaos.Split(","))
            {
                WriteHeading($"Exporting traces of ICAO: #{icao}...");

                var icaoFiles = Directory.GetFiles(InputDir, $"*{icao}*", SearchOption.AllDirectories);

                foreach (var file in icaoFiles)
                {
                    if (!file.EndsWith(".json.gz")) continue;

                    await using var gzStream = File.OpenRead(file);
                    await using var srcStream = new GZipStream(gzStream, CompressionMode.Decompress);
                    using var streamReader = new StreamReader(srcStream);
                    var fileContent = await streamReader.ReadToEndAsync();

                    var adsbExchangeFlight = new Flight(fileContent);

                    if (adsbExchangeFlight.IsInvalid) continue;

                    var flight = new Feature(
                        new LineString(adsbExchangeFlight.Coords),
                        new Dictionary<string, object>
                        {
                            {"type", "flight"},
                            {"icao", adsbExchangeFlight.Icao},
                            {"aircraft", GetAircraftNameByIcao(adsbExchangeFlight.Icao)},
                            {"from", adsbExchangeFlight.From},
                            {"to", adsbExchangeFlight.To},
                            {"durationSeconds", adsbExchangeFlight.DurationSeconds }
                        },
                        Guid.NewGuid().ToString());

                    flights.Features.Add(flight);
                }
            }

            var serialized = JsonConvert.SerializeObject(flights);
            await File.WriteAllTextAsync(Path.Combine(OutputDir, "flights.geojson"), serialized);
        }
    }
}