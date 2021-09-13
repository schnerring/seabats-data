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
            var date = DateTimeOffset.FromUnixTimeMilliseconds(unixMilliseconds).DateTime;

            if (flight.Trace == null || flight.Trace.Count() < 2)
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
            From = date.AddSeconds(double.Parse(traceStartStr!)).ToUniversalTime(); // TODO?
            To = date.AddSeconds(double.Parse(traceEndStr!)).ToUniversalTime();
            DurationSeconds = (To - From).TotalSeconds;

            Coords = flight.Trace.Select(t => new[] {(double) t[lonIndex], (double) t[latIndex]}).ToList();
        }

        public bool IsInvalid { get; }

        public string Icao { get; }

        public DateTime From { get; }

        public DateTime To { get; }

        public double DurationSeconds { get; }

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