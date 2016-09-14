using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using TrainTracker.Models;

namespace TrainTracker.Web.Models.Mapping
{
    public class ShapeMap : EntityTypeConfiguration<Shape>
    {
        public ShapeMap()
        {
            // Primary Key
            HasKey(t => t.Id);

            // Properties
            Property(t => t.ShapeId)
                .HasMaxLength(255);

            // Table & Column Mappings
            ToTable("Shapes");
            Property(t => t.Id).HasColumnName("ID");
            Property(t => t.ShapeId).HasColumnName("shape_id");
            Property(t => t.ShapePtLat).HasColumnName("shape_pt_lat");
            Property(t => t.ShapePtLon).HasColumnName("shape_pt_lon");
            Property(t => t.ShapePtSequence).HasColumnName("shape_pt_sequence");
            Property(t => t.ShapeDistTraveled).HasColumnName("shape_dist_traveled");
        }
    }
}
