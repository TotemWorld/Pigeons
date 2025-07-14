using GeoUtils.Data;
using GeoUtils.Interop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GeoUtils
{
    public class Calculations
    {
        public static Coordinate TranslateByDistanceLoxodrome(Coordinate currentPosition, Direction direction, double distance)
        {
            switch (direction)
            {
                case Direction.ToEast:
                    return CalculateLongitudeChange(distance, currentPosition);
                case Direction.ToWest:
                    return CalculateLongitudeChange(-distance, currentPosition);
                case Direction.ToNorth:
                    return CalculateLatitudeChange(distance, currentPosition);
                case Direction.ToSouth:
                    return CalculateLatitudeChange(-distance, currentPosition);
                case Direction.ToNorthEast:
                    var distLatNE = distance * Math.Cos(Math.PI / 4);
                    var distLonNE = distance * Math.Sin(Math.PI / 4);
                    var newPositionNE = CalculateLatitudeChange(distLatNE, currentPosition);
                    return CalculateLongitudeChange(distLonNE, newPositionNE);
                case Direction.ToNorthWest:
                    var distLatNW = distance * Math.Cos(Math.PI / 4);
                    var distLonNW = distance * Math.Sin(Math.PI / 4);
                    var newPositionNW = CalculateLatitudeChange(distLatNW, currentPosition);
                    return CalculateLongitudeChange(-distLonNW, newPositionNW);
                case Direction.ToSouthEast:
                    var distLatSE = distance * Math.Cos(Math.PI / 4);
                    var distLonSE = distance * Math.Sin(Math.PI / 4);
                    var newPositionSE = CalculateLatitudeChange(-distLatSE, currentPosition);
                    return CalculateLongitudeChange(distLonSE, newPositionSE);
                case Direction.ToSouthWest:
                    var distLatSW = distance * Math.Cos(Math.PI / 4);
                    var distLonSW = distance * Math.Sin(Math.PI / 4);
                    var newPositionSW = CalculateLatitudeChange(-distLatSW, currentPosition);
                    return CalculateLongitudeChange(-distLonSW, newPositionSW);
                default:
                    throw new ArgumentException("direction not implemented");
            }
        }

        public static Coordinate TranslateByDistanceGeodesic(Coordinate currentPosition, double bearing, double distance)
        {
            Interop.NativeMethods.GeoDirect(currentPosition.Latitude, currentPosition.Longitude, bearing, distance, out var newLatitude, out var newLongitude, out var azimuth);
            return new Coordinate(newLatitude, newLongitude);
        }

        private static Coordinate CalculateLongitudeChange(double distance, Coordinate currentPosition)
        {
            var parallelLatitudeRadius = Geodesy.GetParallelLatitudeRadius(currentPosition.Latitude);
            var delta = distance / parallelLatitudeRadius;
            var deltaDeg = Geodesy.RadiansToDegrees(delta);
            return new Coordinate(currentPosition.Latitude, currentPosition.Longitude + deltaDeg);
        }

        private static Coordinate CalculateLatitudeChange(double distance, Coordinate currentPosition)
        {
            var delta = distance / Geodesy.GetMeridianRadiusCurvature(currentPosition.Latitude);
            var deltaDeg = Geodesy.RadiansToDegrees(delta);
            return new Coordinate(currentPosition.Latitude + deltaDeg, currentPosition.Longitude);
        }
    }
}
