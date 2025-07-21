import 'package:flutter/material.dart';
import 'package:flutter_map/flutter_map.dart';
import 'package:web_socket_channel/web_socket_channel.dart';
import 'package:latlong2/latlong.dart';
import 'package:visualizer/visualizer_manager.dart';

void main() async {
  WidgetsFlutterBinding.ensureInitialized();
  runApp(const MyApp());
}

class MyApp extends StatelessWidget {
  const MyApp({super.key});

  @override
  Widget build(BuildContext context) {
    return MaterialApp(
      title: 'Flutter Demo',
      theme: ThemeData(
        colorScheme: ColorScheme.fromSeed(seedColor: Colors.deepPurple),
      ),
      home: const MyHomePage(title: 'Flutter Demo Home Page'),
    );
  }
}

class MyHomePage extends StatefulWidget {
  const MyHomePage({super.key, required this.title});

  final String title;
  static const String accessToken = String.fromEnvironment("ACCESS_TOKEN");

  @override
  State<MyHomePage> createState() => _MyHomePageState();
}

class _MyHomePageState extends State<MyHomePage> {
  late final MapController _mapController;
  late final wsUrl = Uri.parse('ws://localhost:11111');
  late final WebSocketChannel channel = WebSocketChannel.connect(wsUrl);

  @override
  void initState() {
    super.initState();
    _mapController = MapController();
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(
        backgroundColor: Theme.of(context).colorScheme.inversePrimary,

        title: Text(widget.title),
      ),
      body: SizedBox(
        width: double.infinity,
        height: double.infinity,
        child: StreamBuilder(
          stream: channel.stream,
          builder: (context, asyncSnapshot) {
            if (asyncSnapshot.hasData) {
              final message = asyncSnapshot.data;
              if (VisualizerManager.isInitialized) {
                VisualizerManager.processLocations(message);
              } else {
                VisualizerManager.processInit(message);
              }
              return VisualizerManager.isInitialized
                  ? FlutterMap(
                      mapController: _mapController,
                      options: MapOptions(
                        initialCenter: LatLng(
                          VisualizerManager.startingLat,
                          VisualizerManager.startingLng,
                        ),
                        initialZoom: 18.0,
                      ),
                      children: [
                        TileLayer(
                          urlTemplate:
                              'https://{s}.basemaps.cartocdn.com/light_all/{z}/{x}/{y}.png',
                          subdomains: const ['a', 'b', 'c', 'd'],
                          userAgentPackageName: 'dev.flutter.visualizer',
                          additionalOptions: const {
                            'attribution':
                                '© OpenStreetMap contributors, © CARTO',
                          },
                          maxNativeZoom: 18,
                          errorTileCallback: (tile, error, stackTrace) {
                            print('Tile loading error: $error');
                          },
                        ),
                        MarkerLayer(
                          markers: VisualizerManager.pigeonLocations.isNotEmpty
                              ? [
                                  ...VisualizerManager.pigeonLocations.map(
                                    (location) => Marker(
                                      point: LatLng(
                                        location.latitude,
                                        location.longitude,
                                      ),
                                      width: 70,
                                      height: 80,
                                      child: Column(
                                        mainAxisSize: MainAxisSize.min,
                                        children: [
                                          Container(
                                            width: 40,
                                            height: 40,
                                            decoration: const BoxDecoration(
                                              shape: BoxShape.circle,
                                              color: Colors.transparent,
                                            ),
                                            child: ClipOval(
                                              child: Image.asset(
                                                'assets/pigeon.png',
                                                width: 35,
                                                height: 35,
                                                fit: BoxFit.cover,
                                                errorBuilder:
                                                    (
                                                      context,
                                                      error,
                                                      stackTrace,
                                                    ) {
                                                      return const Icon(
                                                        Icons.pets,
                                                        color: Colors.blue,
                                                        size: 25,
                                                      );
                                                    },
                                              ),
                                            ),
                                          ),
                                          const SizedBox(height: 4),
                                          Container(
                                            padding: const EdgeInsets.symmetric(
                                              horizontal: 6,
                                              vertical: 2,
                                            ),

                                            child: Text(
                                              location.name,
                                              style: const TextStyle(
                                                fontSize: 10,
                                                fontWeight: FontWeight.bold,
                                                color: Colors.black87,
                                              ),
                                              textAlign: TextAlign.center,
                                            ),
                                          ),
                                        ],
                                      ),
                                    ),
                                  ),
                                ]
                              : [],
                        ),
                      ],
                    )
                  : const Center(
                      child: Text('Visualizer not initialized yet.'),
                    );
            } else {
              return const Center(child: CircularProgressIndicator());
            }
          },
        ),
      ),
    );
  }
}
