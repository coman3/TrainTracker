using System;
using TrainTracker.Models;

namespace TrainTracker.Helpers
{
    public static class Maths
    {
        private const double EarthRadius = 6371e3;
        public static double Distance(LatLng p1, LatLng p2)
        {
            double theta = p1.Longitude - p2.Longitude;
            double dist = Math.Sin(p1.Latitude.ToRadians()) * Math.Sin(p2.Latitude.ToRadians()) +
                          Math.Cos(p1.Latitude.ToRadians()) * Math.Cos(p2.Latitude.ToRadians()) *
                          Math.Cos(theta.ToRadians());
            dist = Math.Acos(dist);
            dist = dist.ToDegrees();
            dist = dist * 60 * 1.1515 * 1.609344;
            return dist; //KM
        }

        public static LatLng ToBearingDistance(double distance, LatLng point, double bearing)
        {
            var d = distance / EarthRadius;
            var lat1 = point.Latitude.ToRadians();
            var lon1 = point.Longitude.ToRadians();

            var sinLat1 = Math.Sin(lat1);
            var cosLat1 = Math.Cos(lat1);
            var sinD = Math.Sin(d);
            var cosD = Math.Cos(d);
            var sinBearing = Math.Sin(bearing);
            var cosBearing = Math.Cos(bearing);

            var sinφ2 = sinLat1 * cosD + cosLat1 * sinD * cosBearing;
            return new LatLng(Math.Asin(sinφ2).ToDegrees(),
                (lon1 + Math.Atan2(sinBearing*sinD*cosLat1, cosD - sinLat1*sinφ2)).ToDegrees());
        }
        public static double CalculateBearing(LatLng p1, LatLng p2)
        {

            var lat1 = p1.Latitude.ToRadians();
            var lat2 = p2.Latitude.ToRadians();
            var lonTheta = (p2.Longitude - p1.Longitude).ToRadians();

            var y = Math.Sin(lonTheta) * Math.Cos(lat2);
            var x = Math.Cos(lat1) * Math.Sin(lat2) -
                    Math.Sin(lat1) * Math.Cos(lat2) * Math.Cos(lonTheta);
            var theta = Math.Atan2(y, x);

            return (theta.ToDegrees() + 360) % 360;
        }
    }
}