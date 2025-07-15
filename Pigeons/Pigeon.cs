using Geohash;
using GeoUtils;
using GeoUtils.Data;
using GeoUtils.Extensions;
using System.Runtime.CompilerServices;
using System.Threading;
using static System.Net.Mime.MediaTypeNames;

namespace Pigeons
{
    public class PigeonBuilder
    {
        private Pigeon _pigeon = new Pigeon();

        //Checks
        private bool _baseCoordinateFlow = false;
        private bool _geoHashFlow = false;
        private bool _startCoordinateCheck = false;
        private bool _modeCheck = false;

        /// <summary>
        /// Sets the base coordinate and geohash precision for the builder. If you are unsure which geohash to use,
        /// specify the coordinate of interest and the desired geohash precision. The geohash will be calculated
        /// based on the base coordinate and precision level.
        /// </summary>
        /// <param name="baseCoordinate">The base coordinate for geohash calculation.</param>
        /// <param name="geoHashPrecision">The geohash precision level. Must be a positive integer.</param>
        /// <returns>The current <see cref="PigeonBuilder"/> instance for method chaining.</returns>
        /// <exception cref="InvalidOperationException">Thrown if the base coordinate is set after the geohash has already been configured.</exception>
        public PigeonBuilder SetBaseCoordinate(Coordinate baseCoordinate, int geoHashPrecision)
        {
            if(_geoHashFlow)
            {
                throw new InvalidOperationException("Cannot set base coordinate after geohash has been set.");
            }

            _baseCoordinateFlow = true;

            var geohasher = new Geohasher();
            _pigeon.GeoHash = geohasher.Encode(baseCoordinate.Latitude, baseCoordinate.Longitude, geoHashPrecision);
            _pigeon.BoundingBox = geohasher.GetBoundingBox(_pigeon.GeoHash);

            return this;
        }

        public PigeonBuilder SetGeohash(string geohash)
        {
            if(_baseCoordinateFlow)
            {
                throw new InvalidOperationException("Cannot set geohash after base coordinate has been set.");
            }
            _pigeon.GeoHash = geohash;
            _geoHashFlow = true;
            var geohasher = new Geohasher();
            _pigeon.BoundingBox = geohasher.GetBoundingBox(_pigeon.GeoHash);

            return this;
        }

        public PigeonBuilder SetStartCoordinate(Coordinate startCoordinate)
        {

            if (!_geoHashFlow || !_baseCoordinateFlow)
            {
                throw new InvalidOperationException("Cannot set start coordinate before geohash has been set.");
            }
            var isPointInside = _pigeon.BoundingBox.CheckPointInside(startCoordinate);
            if (!isPointInside)
            {
                throw new ArgumentException("Start coordinate is outside the bounding box defined by the geohash.");
            }
            _pigeon.StartCoordinate = startCoordinate;
            _startCoordinateCheck = true;
            return this;
        }

        /// <summary>
        /// Sets a random start coordinate for the pigeon within the bounding box defined by the geohash.
        /// This method automatically generates a random point that is guaranteed to be inside the geohash area.
        /// </summary>
        /// <returns>The current instance of <see cref="PigeonBuilder"/> for method chaining.</returns>
        public PigeonBuilder SetRandomStartCoordinate()
        {
            var random = new Random();
            var randomLong = random.NextDouble() * (_pigeon.BoundingBox.MaxLng - _pigeon.BoundingBox.MinLng) + _pigeon.BoundingBox.MinLng;

            var randomLat = random.NextDouble() * (_pigeon.BoundingBox.MaxLat - _pigeon.BoundingBox.MinLat) + _pigeon.BoundingBox.MinLat;

            _pigeon.StartCoordinate = new Coordinate(randomLat, randomLong);

            _startCoordinateCheck = true;
            return this;
        }

        /// <summary>
        /// Sets the distance step for the pigeon's movement. This value determines how far the pigeon 
        /// will move in each step.
        /// </summary>
        /// <param name="distanceStep">The distance in meters that the pigeon should move in each step. Must be greater than zero.</param>
        /// <returns>The current instance of <see cref="PigeonBuilder"/> for method chaining.</returns>
        /// <exception cref="ArgumentException">Thrown if the distance step is zero or negative.</exception>
        public PigeonBuilder SetDistanceStep(double distanceStep)
        {
            if (distanceStep <= 0)
            {
                throw new ArgumentException("Distance step must be a positive value.");
            }
            _pigeon.DistanceStep = distanceStep;
            return this;
        }

        /// <summary>
        /// Sets the time interval between consecutive position updates for the pigeon.
        /// </summary>
        /// <param name="cooldown">The time interval to wait between position updates. Must be a positive time span.</param>
        /// <returns>The current instance of <see cref="PigeonBuilder"/> for method chaining.</returns>
        /// <exception cref="ArgumentException">Thrown if the cooldown value is zero or negative.</exception>
        public PigeonBuilder SetCooldown(TimeSpan cooldown)
        {
            if (cooldown <= TimeSpan.Zero)
            {
                throw new ArgumentException("Cooldown must be a positive time span.");
            }
            _pigeon.Cooldown = cooldown;
            return this;
        }

        public PigeonBuilder SetLifetime(TimeSpan lifetime)
        {
            if (lifetime <= TimeSpan.Zero)
            {
                throw new ArgumentException("Lifetime must be a positive time span.");
            }
            _pigeon.Lifetime = lifetime;
            return this;
        }

        /// <summary>
        /// Configures the builder to use loxodrome mode with the specified direction.
        /// Loxodrome is a path with constant bearing relative to true north.
        /// </summary>
        /// <param name="direction">The constant compass direction in which the pigeon will travel.</param>
        /// <returns>The current <see cref="PigeonBuilder"/> instance, allowing for method chaining.</returns>
        /// <exception cref="InvalidOperationException">Thrown if loxodrome mode is set after geodesic mode has been configured.</exception>
        /// <remarks>
        /// Unlike geodesic paths, loxodromes maintain a constant heading but are not the shortest path between points.
        /// This method cannot be called if geodesic mode has already been set.
        /// </remarks>
        public PigeonBuilder SetLoxodromeMode(GeoUtils.Data.Direction direction)
        {
            if (_pigeon.Aleatory) throw new InvalidOperationException("Cannot set loxodrome mode after aleatory mode has been set.");
            if (_pigeon.Mode == Mode.Geodesic) throw new InvalidOperationException("Cannot set loxodrome mode after geodesic mode has been set.");
            _pigeon.Mode = Mode.Loxodrome;
            _pigeon.Direction = direction;
            _modeCheck = true;
            return this;
        }

        /// <summary>
        /// Configures the builder to use geodesic mode with the specified bearing. Is using GeographicLib (C++) direct method.
        /// </summary>
        /// <remarks>Geodesic mode calculates the shortest path between two points on a sphere.  This
        /// method cannot be called if loxodrome mode has already been set.</remarks>
        /// <param name="bearing">The bearing, in degrees, to be used for geodesic calculations. Must be a valid angle between 0 and 360.</param>
        /// <returns>The current <see cref="PigeonBuilder"/> instance, allowing for method chaining.</returns>
        /// <exception cref="InvalidOperationException">Thrown if geodesic mode is set after loxodrome mode has been configured.</exception>
        public PigeonBuilder SetGeodesicMode(double bearing)
        {
            if(_pigeon.Aleatory) throw new InvalidOperationException("Cannot set geodesic mode after aleatory mode has been set.");
            if (_pigeon.Mode == Mode.Loxodrome) throw new InvalidOperationException("Cannot set geodesic mode after loxodrome mode has been set.");
            _pigeon.Mode = Mode.Geodesic;
            _pigeon.Bearing = bearing;
            _modeCheck = true;  
            return this;
        }
        /// <summary>
        /// Instead of going in one direction, the pigeon will randomly choose a direction at each step, making its path unpredictable.
        /// </summary>
        /// <param name="mode">Geodesy or Loxodrome modes</param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        public PigeonBuilder SetAleatory(Mode mode)
        {
            if (_pigeon.Mode != null) throw new InvalidOperationException("Cannot set aleatory mode after mode has been set.");
            _pigeon.Mode = mode;
            _modeCheck = true;
            _pigeon.Aleatory = true;
            return this;
        }

        public PigeonBuilder SetCancellationToken(CancellationToken cancellationToken)
        {
            _pigeon.ExternalCancellationToken = cancellationToken;
            return this;
        }

        public Pigeon Build()
        {
            if (!_baseCoordinateFlow && !_geoHashFlow)
            {
                throw new InvalidOperationException("base coordinate or geo hash must be set");
            }
            if (!_startCoordinateCheck)
            {
                throw new InvalidOperationException("Pigeon cannot be built without setting start coordinate.");
            }
            if (!_modeCheck)
            {
                throw new InvalidOperationException("Pigeon cannot be built without setting mode (loxodrome or geodesic).");
            }

            return _pigeon;
        }
    }

    public class Pigeon
    {
        public EventHandler<Coordinate>? PositionChanged;
        public string? GeoHash { get; internal set; }
        public GeoUtils.Data.Direction Direction { get; internal set; } = GeoUtils.Data.Direction.ToEast;
        public double Bearing { get; internal set; } = 90; 
        public double DistanceStep { get; internal set; } = 10;
        public Coordinate StartCoordinate { get; internal set; }
        public TimeSpan Cooldown { get; internal set; } = TimeSpan.FromSeconds(1);
        public TimeSpan Lifetime { get; internal set; } = TimeSpan.FromMinutes(5);
        public Mode? Mode { get; internal set; }
        public CancellationToken ExternalCancellationToken { get; internal set; }
        public BoundingBox BoundingBox { get; internal set; }
        public bool Aleatory { get; internal set; } = false;

        private CancellationTokenSource _src = new CancellationTokenSource();

        private CancellationToken _cancellationToken;

        internal Pigeon() { }

        protected virtual void OnPositionChanged(Coordinate coordinate)
        {
            PositionChanged?.Invoke(this, coordinate);
        }

        public async Task Initialize()
        {
            var internalToken = _src.Token;
            _cancellationToken = CancellationTokenSource.CreateLinkedTokenSource(internalToken, ExternalCancellationToken).Token;

            _src.CancelAfter(Lifetime);

            var position = StartCoordinate;
            var currentDirection = Direction;
            var currentBearing = Bearing;

            while (!_cancellationToken.IsCancellationRequested)
            {
                Coordinate next;
                bool isPointInside;

                if(Aleatory)
                {
                    if(Mode == GeoUtils.Data.Mode.Geodesic)
                    {
                        var random = new Random();
                        currentBearing = random.Next(0, 360);
                        next = Translate(position, ref currentBearing, DistanceStep);
                    }
                    else
                    {
                        var random = new Random();
                        currentDirection = (GeoUtils.Data.Direction)random.Next(0, 8);
                        next = Translate(position, ref currentDirection, DistanceStep);
                    }
                }
                else
                {
                    if (Mode == GeoUtils.Data.Mode.Loxodrome)
                    {
                        next = Translate(position, ref currentDirection, DistanceStep);
                    }
                    else
                    {
                        next = Translate(position, ref currentBearing, DistanceStep);
                    }
                }


                position = next;
                OnPositionChanged(position);
                await Task.Delay(Cooldown);
            }
        }

        private Coordinate Translate(Coordinate position, ref GeoUtils.Data.Direction currentDirection, double step)
        {
            Coordinate next;
            bool isPointInside;

            next = Calculations.TranslateByDistanceLoxodrome(position, currentDirection, step);
            isPointInside = BoundingBox.CheckPointInside(next);

            if (!isPointInside)
            {
                currentDirection = currentDirection.Reverse();
                next = Calculations.TranslateByDistanceLoxodrome(position, currentDirection, step);
            }

            return next;
        }

        private Coordinate Translate(Coordinate position, ref double currentBearing, double step)
        {
            Coordinate next;
            bool isPointInside;

            next = Calculations.TranslateByDistanceGeodesic(position, currentBearing, step);
            isPointInside = BoundingBox.CheckPointInside(next);

            if (!isPointInside)
            {
                currentBearing = (currentBearing + 180) % 360;
                next = Calculations.TranslateByDistanceGeodesic(position, currentBearing,step);
            }

            return next;
        }
    }
}
