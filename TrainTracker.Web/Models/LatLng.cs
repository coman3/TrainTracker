namespace TrainTracker.Web.Models
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
    }
}