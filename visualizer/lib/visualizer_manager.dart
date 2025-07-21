import 'dart:convert';

class VisualizerManager {
  static final List<PigeonLocation> pigeonLocations = [];
  static double zoomFromGeohash = 14.0;
  static double startingLat = -10.069296;
  static double startingLng = -75.550806;

  static bool isInitialized = false;

  static void processInit(String message) {
    if (message.contains("init")) {
      Map<String, dynamic> data = jsonDecode(message);
      var geohashPrecision = data['init']['precision'];
      zoomFromGeohash = getZoomFromGeohashPrecision(geohashPrecision);

      startingLat = data['init']['center']['lat'];
      startingLng = data['init']['center']['lng'];
      isInitialized = true;
    }
  }

  static void processLocations(String message) {
    if (!message.contains('init')) {
      dynamic decoded = jsonDecode(message);

      if (decoded is List) {
        List<Map<String, dynamic>> locationList =
            List<Map<String, dynamic>>.from(decoded);

        for (Map<String, dynamic> locationData in locationList) {
          processLocation(locationData);
        }
      }
    }
  }

  static void processLocation(Map<String, dynamic> locationData) {
    if (!locationData.containsKey('init')) {
      var name = locationData.keys.first;
      var body = locationData.values.first;
      var latitude = body['lat'];
      var longitude = body['lng'];

      if (pigeonLocations.any((pigeon) => pigeon.name == name)) {
        var existingLocation = pigeonLocations.firstWhere(
          (pigeon) => pigeon.name == name,
        );
        existingLocation.latitude = latitude;
        existingLocation.longitude = longitude;
      } else {
        var location = PigeonLocation(name, latitude, longitude);
        pigeonLocations.add(location);
      }
    }
  }

  static double getZoomFromGeohashPrecision(int geohashPrecision) {
    switch (geohashPrecision) {
      case 1:
        return 1.0;
      case 2:
        return 5.0;
      case 3:
        return 7.0;
      case 4:
        return 9.0;
      case 5:
        return 11.0;
      case 6:
        return 13.0;
      case 7:
        return 15.0;
      case 8:
        return 17.0;
      case 9:
        return 19.0;
      case 10:
        return 20.0;
      default:
        return 20.0;
    }
  }
}

class PigeonLocation {
  final String name;
  double latitude;
  double longitude;

  PigeonLocation(this.name, this.latitude, this.longitude);

  @override
  String toString() {
    return 'PigeonLocation(name: $name, latitude: $latitude, longitude: $longitude)';
  }
}
