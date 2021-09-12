using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using GeoJSON.Net.Feature;
using GeoJSON.Net.Geometry;
using Newtonsoft.Json;
using SeabatsData.Core;

namespace SeabatsData.MaritimeBoundaries
{
    internal class Program : ProgramBase
    {
        private static readonly string[] Countries =
        {
            "Greece", "Italy", "Libya", "Malta", "Tunisia", "Turkey"
        };

        private static async Task Main(string[] args)
        {
            await ExportSarZones();
            await ExportMaritimeBoundaries();
        }

        private static async Task ExportSarZones()
        {
            var sarZones = new[]
            {
                new
                {
                    Title = "Maritime SAR Region of Libya",
                    Attribution = "<TODO>",
                    DegreeCoords = new[]
                    {
                        ((32, 22.00), (11, 30.00)),
                        ((34, 20.00), (11, 30.00)),
                        ((34, 20.00), (23, 35.00)),
                        ((34, 00.00), (24, 10.00)),
                        ((31, 40.00), (25, 10.00)),
                        ((30, 16.00), (19, 05.00)),
                        ((32, 22.00), (11, 30.00))
                    }
                },
                new
                {
                    Title = "Malta SRR",
                    Attribution = "<TODO>",
                    DegreeCoords = new[]
                    {
                        ((36, 30.00), (11, 30.00)),
                        ((36, 30.00), (19, 00.00)),
                        ((34, 20.00), (23, 35.00)),
                        ((34, 20.00), (11, 30.00)),
                        ((36, 30.00), (11, 30.00)),
                    }
                },
                new
                {
                    Title = "MRCC ROMA",
                    Attribution = "<TODO>",
                    DegreeCoords = new[]
                    {
                        ((43, 47.10), (07, 31.80)),
                        ((43, 30.00), (07, 42.00)),
                        ((43, 30.00), (09, 30.00)),
                        ((43, 10.00), (09, 45.00)),
                        ((42, 05.00), (09, 45.00)),
                        ((41, 20.00), (09, 45.00)),
                        ((41, 20.00), (07, 44.00)),
                        ((38, 32.00), (07, 44.00)),
                        ((38, 32.00), (09, 05.00)),
                        ((38, 00.00), (10, 21.00)),
                        ((37, 30.00), (11, 30.00)),
                        ((36, 30.00), (11, 30.00)),
                        ((35, 15.00), (12, 14.00)),
                        ((35, 15.00), (12, 40.00)),
                        ((36, 30.00), (14, 08.00)),
                        ((36, 00.00), (16, 00.00)),
                        ((36, 00.00), (19, 00.00)),
                        ((40, 25.00), (19, 00.00)),
                        ((41, 23.50), (18, 19.50)),
                        ((41, 30.00), (18, 09.00)),
                        ((41, 34.20), (18, 00.00)),
                        ((42, 15.00), (16, 33.20)),
                        ((42, 31.10), (16, 01.40)),
                        ((42, 40.50), (15, 43.50)),
                        ((42, 46.10), (15, 33.10)),
                        ((42, 55.30), (15, 16.20)),
                        ((43, 17.30), (14, 45.60)),
                        ((43, 29.90), (14, 30.00)),
                        ((44, 18.10), (13, 28.10)),
                        ((44, 32.00), (13, 13.90)),
                        ((44, 45.10), (13, 08.10)),
                        ((45, 09.80), (13, 00.00)),
                        ((45, 27.30), (13, 12.70)),
                        ((45, 27.20), (13, 12.70)),
                        ((45, 32.70), (13, 18.80)),
                        ((45, 37.80), (13, 37.80)),
                        ((45, 35.90), (13, 42.80)),
                        ((45, 35.70), (13, 43.40)),
                        ((43, 47.10), (07, 31.80))
                    }
                }
            };

            var export = new FeatureCollection();

            foreach (var sarZone in sarZones)
            {
                var coords = sarZone.DegreeCoords.Select(c =>
                {
                    var latDeg = c.Item1.Item1;
                    var latMin = c.Item1.Item2;
                    var lat = ConvertDegreeAngleToDouble(latDeg, latMin);

                    var lonDeg = c.Item2.Item1;
                    var lonMin = c.Item2.Item2;
                    var lon = ConvertDegreeAngleToDouble(lonDeg, lonMin);

                    return new[] {lon, lat};
                });
                var polygon = new Polygon(new[] {coords});
                export.Features.Add(
                    new Feature(
                        polygon,
                        new Dictionary<string, object>
                        {
                            {"Type", "SAR"},
                            {"Title", sarZone.Title},
                            {"Attribution", sarZone.Attribution},
                        }));
            }

            var serialized = JsonConvert.SerializeObject(export);
            var outPath = Path.Combine(OutputDir, "sar.geojson");
            await File.WriteAllTextAsync(outPath, serialized);
        }

        private static async Task ExportMaritimeBoundaries()
        {
            var maritimeBoundaries = new[]
            {
                new
                {
                    Type = "12NM",
                    Attribution =
                        @"Flanders Marine Institute (2019). Maritime Boundaries Geodatabase: Territorial Seas (12NM), version 3. Available online at <a href=""https://www.marineregions.org/"">https://www.marineregions.org/</a> <a href=""https://doi.org/10.14284/387"">https://doi.org/10.14284/387</a>."
                },
                new
                {
                    Type = "24NM",
                    Attribution =
                        @"Flanders Marine Institute (2019). Maritime Boundaries Geodatabase: Contiguous Zones (24NM), version 3. Available online at <a href=""https://www.marineregions.org/"">https://www.marineregions.org/</a> <a href=""https://doi.org/10.14284/384"">https://doi.org/10.14284/384</a>."
                }
            };

            foreach (var boundary in maritimeBoundaries)
            {
                var json = await File.ReadAllTextAsync(
                    $@"U:\seabats\maritime-boundaries\eez_{boundary.Type}_v3.geojson");

                var original = JsonConvert.DeserializeObject<FeatureCollection>(json);

                if (original == null)
                    throw new InvalidOperationException();

                var featuresToExport = original.Features.Where(f =>
                    Countries.Contains(f.Properties["TERRITORY1"]) || Countries.Contains(f.Properties["TERRITORY1"]));

                var export = new FeatureCollection();

                foreach (var feature in featuresToExport)
                    export.Features.Add(
                        new Feature(
                            feature.Geometry,
                            new Dictionary<string, object>
                            {
                                {"Type", boundary.Type},
                                {"Title", feature.Properties["GEONAME"]},
                                {"Attribution", boundary.Attribution}
                            }));

                var serialized = JsonConvert.SerializeObject(export);
                var outPath = Path.Combine(OutputDir, $"{boundary.Type}.geojson");
                await File.WriteAllTextAsync(outPath, serialized);
            }
        }

        private static double ConvertDegreeAngleToDouble(double degrees, double minutes)
        {
            return degrees + minutes / 60;
        }
    }
}