using GeoUtils.Data;
using Geohash;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GeoUtils.Extensions
{
    public static class BoundingBoxExtensions
    {
        public static bool CheckPointInside(this BoundingBox bbox, Coordinate coordinate)
        {
            if(bbox.MaxLng < coordinate.Longitude || bbox.MinLng > coordinate.Longitude)
            {
                return false;
            }

            if(bbox.MaxLat < coordinate.Latitude || bbox.MinLat > coordinate.Latitude)
            {
                return false;
            }

            return true;
        }
    }
}
