using GeoUtils.Data;
using GeoUtils.Interop;
using Pigeons;

//var pigeon = new PigeonBuilder()
//    .SetGeohash("c2b2md6")
//    .SetRandomStartCoordinate()
//    .SetAleatory(Mode.Geodesic)
//    .SetDistanceStep(1)
//    .Build();

var pigeon = new PigeonBuilder()
    .SetGeohash("c2b2md6")
    .SetRandomStartCoordinate()
    .SetGeodesicMode(90)
    .SetDistanceStep(1)
    .Build();

pigeon.PositionChanged += (sender, args) =>
{
    Console.WriteLine($"Pigeon moved to: {args.Latitude}, {args.Longitude}");
};

await pigeon.Initialize();
