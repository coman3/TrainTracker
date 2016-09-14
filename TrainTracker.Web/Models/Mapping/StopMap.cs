using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using TrainTracker.Models;

namespace TrainTracker.Web.Models.Mapping
{
    public class StopMap : EntityTypeConfiguration<Stop>
    {
        public StopMap()
        {
            // Primary Key
            HasKey(t => t.StopId);

            // Properties
            Property(t => t.StopId)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.None);

            Property(t => t.StopName)
                .IsRequired()
                .HasMaxLength(255);

            // Table & Column Mappings
            ToTable("Stops");
            Property(t => t.StopId).HasColumnName("stop_id");
            Property(t => t.StopName).HasColumnName("stop_name");
            Property(t => t.StopLat).HasColumnName("stop_lat");
            Property(t => t.StopLon).HasColumnName("stop_lon");
        }
    }
}
