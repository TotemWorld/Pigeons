#include "pch.h"
#include <GeographicLib/Geodesic.hpp>
using namespace GeographicLib;

extern "C"
__declspec(dllexport)
void GeoDirect(double  lat1, double  lon1, double  azi1,
    double  s12,
    double* lat2, double* lon2, double* azi2)
{
    static const Geodesic& wgs84 = Geodesic::WGS84();
    wgs84.Direct(lat1, lon1, azi1, s12, *lat2, *lon2, *azi2);
}
