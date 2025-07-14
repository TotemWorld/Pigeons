using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GeoUtils.Data
{
    public class Coordinate : EventArgs
    {
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public Coordinate(double latitude, double longitude)
        {
            Latitude = latitude;
            Longitude = longitude;
        }
        public override string ToString()
        {
            return $"{Latitude}, {Longitude}";
        }
    }
}
