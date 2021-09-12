using System.IO;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace SeabatsData.MaritimeBoundaries
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var geoJson12nm = await File.ReadAllTextAsync(@"U:\seabats\maritime-boundaries\eez_12nm_v3.geojson");
            var bounds12nm = JsonConvert.DeserializeObject(geoJson12nm);

            var geoJson24nm = await File.ReadAllTextAsync(@"U:\seabats\maritime-boundaries\eez_24nm_v3.geojson");
            var bounds24nm = JsonConvert.DeserializeObject(geoJson24nm);
        }
    }
}
