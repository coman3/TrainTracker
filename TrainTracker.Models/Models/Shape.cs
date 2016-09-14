namespace TrainTracker.Models
{
    public class Shape
    {
        public int Id { get; set; }
        public string ShapeId { get; set; }
        public double ShapePtLat { get; set; }
        public double ShapePtLon { get; set; }
        public int ShapePtSequence { get; set; }
        public double? ShapeDistTraveled { get; set; }
    }
}
