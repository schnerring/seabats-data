using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace SeabatsData.App.AdsbExchange
{
    public class Flight
    {
        public Flight(string json)
        {
            var flight = JsonConvert.DeserializeObject<JsonModel>(json);

            if (flight == null)
                throw new InvalidOperationException($"JSON is not a {typeof(Flight).FullName}");

            Icao = string.IsNullOrWhiteSpace(flight.Icao) ? flight.Hex : flight.Icao;

            var unixMilliseconds = Convert.ToInt64(flight.Timestamp * 1000);
            Date = DateTimeOffset.FromUnixTimeMilliseconds(unixMilliseconds).DateTime;

            if (flight.Trace == null) return;

            // See: https://www.adsbexchange.com/version-2-api-wip/
            const int latIndex = 1;
            const int lonIndex = 2;

            Coords = flight.Trace.Select(t => new[] {(double) t[latIndex], (double) t[lonIndex]});
        }

        public string Icao { get; }

        public DateTime Date { get; }

        public IEnumerable<double[]> Coords { get; set; }
        
        private class JsonModel
        {
            public string Hex { get; set; }
            public string Icao { get; set; }
            public double Timestamp { get; set; }
            public object[][] Trace { get; set; }
        }
    }
}