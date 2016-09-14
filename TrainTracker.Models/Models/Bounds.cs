namespace TrainTracker.Models
{
    public struct Bounds
    {
        public static readonly Bounds Invalid = new Bounds(LatLng.Invalid, LatLng.Invalid);
        public LatLng NorthEast { get; set; }
        public LatLng SouthWest { get; set; }

        public Bounds(LatLng northWest, LatLng southEast)
        {
            NorthEast = northWest;
            SouthWest = southEast;
        }

        public bool ContainsPoint(LatLng point)
        {
            return (point.Longitude < NorthEast.Longitude && point.Longitude > SouthWest.Longitude) && (point.Latitude > NorthEast.Latitude && point.Latitude < SouthWest.Latitude);
        }
    }
}