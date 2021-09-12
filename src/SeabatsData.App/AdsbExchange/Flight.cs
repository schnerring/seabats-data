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

            if (flight.Trace == null || !flight.Trace.Any())
            {
                IsInvalid = true;
                return;
            }

            // See: https://www.adsbexchange.com/version-2-api-wip/
            const int secondsAfterTimestamp = 0;
            const int latIndex = 1;
            const int lonIndex = 2;

            var traceStartStr = flight.Trace.First()[secondsAfterTimestamp].ToString();
            var traceEndStr = flight.Trace.Last()[secondsAfterTimestamp].ToString();
            var traceStart = Date.AddSeconds(double.Parse(traceStartStr!)); // TODO?
            var traceEnd = Date.AddSeconds(double.Parse(traceEndStr!));
            DurationSeconds = (traceEnd - traceStart).TotalSeconds;

            Coords = flight.Trace.Select(t => new[] {(double) t[latIndex], (double) t[lonIndex]}).ToList();
        }

        public bool IsInvalid { get; }

        public double DurationSeconds { get; }

        public string Icao { get; }

        public DateTime Date { get; }

        public IReadOnlyCollection<double[]> Coords { get; }

        private class JsonModel
        {
            public string Hex { get; set; }
            public string Icao { get; set; }
            public double Timestamp { get; set; }
            public object[][] Trace { get; set; }
        }
    }
}