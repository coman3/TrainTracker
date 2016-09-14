using System.Collections.Generic;

namespace TrainTracker.Models
{
    public struct LatLng
    {
        public static readonly LatLng Invalid = new LatLng(-360, -360);
        public double Latitude { get; set; }
        public double Longitude { get; set; }

        public LatLng(double latitude, double longitude)
        {
            Latitude = latitude;
            Longitude = longitude;
        }

        public bool Equals(LatLng other)
        {
            return Latitude.Equals(other.Latitude) && Longitude.Equals(other.Longitude);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            return obj is LatLng && Equals((LatLng) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (Latitude.GetHashCode()*397) ^ Longitude.GetHashCode();
            }
        }

        private sealed class LatitudeLongitudeEqualityComparer : IEqualityComparer<LatLng>
        {
            public bool Equals(LatLng x, LatLng y)
            {
                return x.Latitude.Equals(y.Latitude) && x.Longitude.Equals(y.Longitude);
            }

            public int GetHashCode(LatLng obj)
            {
                unchecked
                {
                    return (obj.Latitude.GetHashCode()*397) ^ obj.Longitude.GetHashCode();
                }
            }
        }

        private static readonly IEqualityComparer<LatLng> LatitudeLongitudeComparerInstance = new LatitudeLongitudeEqualityComparer();

        public static IEqualityComparer<LatLng> LatitudeLongitudeComparer
        {
            get { return LatitudeLongitudeComparerInstance; }
        }

        public override string ToString()
        {
            return $"Latitude: {Latitude}, Longitude: {Longitude}";
        }

        public static bool operator <(LatLng one, LatLng two)
        {
            return one.Latitude < two.Latitude && one.Longitude < two.Longitude;
        }

        public static bool operator >(LatLng one, LatLng two)
        {
            return one.Latitude > two.Latitude && one.Longitude > two.Longitude;
        }
    }
}
