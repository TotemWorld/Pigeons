

```
string geohash = "c2b2md6";

for (var i = 0; i < 20; i++)
{
    var pigeon = new PigeonBuilder()
        .SetGeohash(geohash)
        .SetRandomStartCoordinate()
        .SetLoxodromeMode(GeoUtils.Data.Direction.ToWest)
        .SetDistanceStep(1)
        .SetCooldown(TimeSpan.FromMilliseconds(500))
        .Build();
```

https://github.com/user-attachments/assets/58454975-6e72-44fd-88c0-379a9e158c5e

