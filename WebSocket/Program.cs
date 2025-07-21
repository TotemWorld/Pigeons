using Geohash;
using GeoUtils.Data;
using Microsoft.AspNetCore.Http;
using Pigeons;
using System.Collections.Concurrent;
using System.Dynamic;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

var app = builder.Build();

app.UseWebSockets();

WebSocket? client = null;
var pigeonPositions = new ConcurrentDictionary<string, Coordinate>();


app.Use(async (context, next) =>
{
    if (context.WebSockets.IsWebSocketRequest)
    {
        client = await context.WebSockets.AcceptWebSocketAsync();

        var buffer = new byte[1024];

        while (client.State == WebSocketState.Open)
        {
            var result = await client.ReceiveAsync(buffer, CancellationToken.None);
            Console.WriteLine($"Received message: {Encoding.UTF8.GetString(buffer, 0, result.Count)}");

        }
    }
    else
    {
        await next(context);
    }
});






_ = Task.Run(async () =>
{
    while (true)
    {
        if (client?.State == WebSocketState.Open && pigeonPositions.Any())
        {
            var dataList = pigeonPositions.Select(p => new Dictionary<string, object>
                {
                    { p.Key, new { lat = p.Value.Latitude, lng = p.Value.Longitude } }
                }).ToList();

            var json = JsonSerializer.Serialize(dataList);
            var bytes = Encoding.UTF8.GetBytes(json);

            try
            {
                await client.SendAsync(bytes, WebSocketMessageType.Text, true, CancellationToken.None);
            }
            catch
            {
                client = null;
            }
        }
        await Task.Delay(500);
    }
});

BoundingBox? boundingBox = null;
string geohash = "c2b2md6";

for (var i = 0; i < 20; i++)
{
    var pigeon = new PigeonBuilder()
        .SetGeohash(geohash)
        .SetRandomStartCoordinate()
        .SetAleatory(Mode.Geodesic)
        //.SetGeodesicMode(90)
        .SetDistanceStep(1)
        .Build();

    if(i == 0)
    {
        boundingBox = pigeon.BoundingBox;
    }

    var pigeonId = $"pigeon_{i}";

    pigeon.PositionChanged += (sender, coordinate) =>
    {
        pigeonPositions[pigeonId] = coordinate;
    };

    _ = Task.Run(async () => await pigeon.Initialize());
}


var centerLong = (boundingBox!.MinLng + boundingBox.MaxLng )/ 2;
var centerLat = (boundingBox.MinLat + boundingBox.MaxLat) / 2;


var data = new ExpandoObject();
data.TryAdd("precision", geohash.Length);
data.TryAdd("center", new { lat = centerLat, lng = centerLong });

var dict = new Dictionary<string, object>();
dict.Add("init", data);

var json = JsonSerializer.Serialize(dict);

_ = Task.Run(async () =>
{
    while (true)
    {
        if (client is not null && client.State == WebSocketState.Open)
        {
            try
            {
                var bytes = Encoding.UTF8.GetBytes(json);
                await client.SendAsync(bytes, WebSocketMessageType.Text, true, CancellationToken.None);
                break;
            }
            catch (WebSocketException ex)
            {
                Console.WriteLine($"WebSocket error: {ex.Message}");
                client = null;
            }
        }
        else
        {
            Console.WriteLine("WebSocket client is not connected or in a closed state.");
        }

        await Task.Delay(1500);
    }
});


app.Run("http://localhost:11111");





