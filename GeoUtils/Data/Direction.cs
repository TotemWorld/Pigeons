using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GeoUtils.Data
{
    public enum Direction
    {
        ToWest,
        ToEast,
        ToSouth,
        ToNorth,
        ToNorthWest,
        ToNorthEast,
        ToSouthWest,
        ToSouthEast
    }

    public static class DirectionExtensions
    {
        public static Direction Reverse(this Direction direction)
        {
            return direction switch
            {
                Direction.ToWest => Direction.ToEast,
                Direction.ToEast => Direction.ToWest,
                Direction.ToSouth => Direction.ToNorth,
                Direction.ToNorth => Direction.ToSouth,
                Direction.ToNorthWest => Direction.ToSouthEast,
                Direction.ToNorthEast => Direction.ToSouthWest,
                Direction.ToSouthWest => Direction.ToNorthEast,
                Direction.ToSouthEast => Direction.ToNorthWest,
                _ => throw new ArgumentException("Invalid direction")
            };
        }
    }
}
