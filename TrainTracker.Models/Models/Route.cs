namespace TrainTracker.Models
{
    public class Route
    {
        public string RouteId { get; set; }
        public string AgencyId { get; set; }
        public string RouteShortName { get; set; }
        public string RouteLongName { get; set; }
        public byte RouteType { get; set; }
    }
}
