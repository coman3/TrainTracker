using System;

namespace TrainTracker.Web.Helpers
{
    public static class Maths
    {
        private const double EarthRadius = 6371e3;
        public static double Distance(double lat1, double lon1, double lat2, double lon2)
        {
            double theta = lon1 - lon2;
            double dist = Math.Sin(deg2rad(lat1)) * Math.Sin(deg2rad(lat2)) + Math.Cos(deg2rad(lat1)) * Math.Cos(deg2rad(lat2)) * Math.Cos(deg2rad(theta));
            dist = Math.Acos(dist);
            dist = rad2deg(dist);
            dist = dist * 60 * 1.1515 * 1.609344;
            return dist;
        }
        private static double deg2rad(double deg)
        {
            return (deg * Math.PI / 180.0);
        }
        private static double rad2deg(double rad)
        {
            return (rad / Math.PI * 180.0);
        }

        public static Tuple<double, double> ToBearingDistance(double distance, double startLat, double startLon, double bearing)
        {
            var δ = distance / EarthRadius;
            var φ1 = startLat.ToRadians();
            var λ1 = startLon.ToRadians();

            var sinφ1 = Math.Sin(φ1);
            var cosφ1 = Math.Cos(φ1);
            var sinδ = Math.Sin(δ);
            var cosδ = Math.Cos(δ);
            var sinθ = Math.Sin(bearing);
            var cosθ = Math.Cos(bearing);

            var sinφ2 = sinφ1 * cosδ + cosφ1 * sinδ * cosθ;
            return new Tuple<double, double>(Math.Asin(sinφ2).ToDegrees(), (λ1 + Math.Atan2(sinθ * sinδ * cosφ1, cosδ - sinφ1 * sinφ2)).ToDegrees());
        }
        public static double CalculateBearing(double lat1, double lon1, double lat2, double lon2)
        {

            var φ1 = lat1.ToRadians();
            var φ2 = lat2.ToRadians();
            var Δλ = (lon2 - lon1).ToRadians();

            var y = Math.Sin(Δλ) * Math.Cos(φ2);
            var x = Math.Cos(φ1) * Math.Sin(φ2) -
                    Math.Sin(φ1) * Math.Cos(φ2) * Math.Cos(Δλ);
            var θ = Math.Atan2(y, x);

            return (θ.ToDegrees() + 360) % 360;
        }
    }
}
// see http://mathforum.org/library/drmath/view/55417.html