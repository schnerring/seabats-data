using System;
using GeoJSON.Net.Feature;
using GeoJSON.Net.Geometry;

namespace SeabatsData.App.GeoJSON
{
    public class Flight : Feature<MultiPoint, FlightProperties> {
        public Flight(MultiPoint geometry, FlightProperties properties) : base(geometry, properties, Guid.NewGuid().ToString())
        {
        }
    }
}

