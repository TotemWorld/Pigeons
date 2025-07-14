using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GeoUtils
{
    public class Geodesy
    {
        // WGS84 semi-major axis in meters
        static double SEMI_MAJOR_AXIS = 6378137.0;
        // WGS84 semi-minor axis in meters
        static double SEMI_MINOR_AXIS = 6356752.314245;
        // WGS84 flattening
        static double FLATTENING = (SEMI_MAJOR_AXIS - SEMI_MINOR_AXIS) / SEMI_MAJOR_AXIS;
        // WGS84 eccentricity squared
        static double ECCENTRICITY_SQUARED = FLATTENING * (2 - FLATTENING);

        public static double DegreesToRadians(double degrees)
        {
            return degrees * (Math.PI / 180.0);
        }

        public static double RadiansToDegrees(double radians)
        {
            return radians * (180.0 / Math.PI);
        }

        //~40,075 km
        public static double GetEquatorialCircumference()
        {
            return 2 * Math.PI * SEMI_MAJOR_AXIS;
        }

        // Meridional Circumsference formula.
        //~40,008 km
        public static double GetMeridionalCircumference()
        {
            double eSquared = ECCENTRICITY_SQUARED;
            double a = SEMI_MAJOR_AXIS;
            double integral = 0.0;
            int n = 10000;
            // One quarter of the ellipse, from 0 to π/2
            double upperLimit = Math.PI / 2.0;

            for (int i = 0; i < n; i++)
            {
                double theta1 = upperLimit * i / n;
                double theta2 = upperLimit * (i + 1) / n;

                // elliptic integral of the second kind for first sub limit
                double y1 = Math.Sqrt(1 - eSquared * Math.Sin(theta1) * Math.Sin(theta1));
                // elliptic integral of the second kind for end sub limit
                double y2 = Math.Sqrt(1 - eSquared * Math.Sin(theta2) * Math.Sin(theta2));

                //trapezoidal rule to approximate the integral
                integral += (y1 + y2) / 2 * (theta2 - theta1);
            }

            return 4 * a * integral;
        }

        //Geodetic position computations page 12
        public static double GetPrimeVerticalRadius(double latitude)
        {
            double latRad = DegreesToRadians(latitude);
            double denominator = Math.Sqrt((1 - ECCENTRICITY_SQUARED * Math.Sin(latRad) * Math.Sin(latRad)));
            return SEMI_MAJOR_AXIS / denominator;
        }

        public static double GetParallelLatitudeRadius(double latitude)
        {
            double latRad = DegreesToRadians(latitude);
            return GetPrimeVerticalRadius(latitude) * Math.Cos(latRad);
        }

        // Geodetic position computations page 10
        public static double GetMeridianRadiusCurvature(double latitude)
        {
            double latRad = DegreesToRadians(latitude);

            double numerator = SEMI_MAJOR_AXIS * (1 - ECCENTRICITY_SQUARED);
            double denominator = Math.Pow(1 - ECCENTRICITY_SQUARED * Math.Sin(latRad) * Math.Sin(latRad), 1.5);

            return numerator / denominator;
        }
    }
}
