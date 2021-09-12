using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Threading.Tasks;
using GeoJSON.Net.Geometry;
using Newtonsoft.Json;
using SeabatsData.Core;

namespace SeabatsData.App
{
    class Program : ProgramBase
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
            
            foreach (var icao in Icaos.Split(","))
            {
                WriteHeading($"Exporting traces of ICAO: #{icao}...");

                var icaoFiles = Directory.GetFiles(InputDir, $"*{icao}*", SearchOption.AllDirectories);

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
                            Label = GetAircraftNameByIcao(adsbExchangeFlight.Icao)
                        });

                    flights.Add(geoJson);
                }

                var serialized = JsonConvert.SerializeObject(flights);
                await File.WriteAllTextAsync(Path.Combine(OutputDir, $"{icao}.json"), serialized);
            }
        }
    }
}
