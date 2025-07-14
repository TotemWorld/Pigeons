using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

namespace GeoUtils.Interop
{


    public static class NativeMethods
    {
        [DllImport(@"GeographicLibWrapper.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void GeoDirect(
            double lat1, double lon1, double azi1,
            double s12,
            out double lat2, out double lon2, out double azi2);
    }

}