using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using TrainTracker.Models;

namespace TrainTracker.Web.Models.Mapping
{
    public class StopTimesMap : EntityTypeConfiguration<StopTime>
    {
        public StopTimesMap()
        {
            // Primary Key
            HasKey(t => t.Id);

            // Properties
            Property(t => t.TripId)
                .IsRequired()
                .HasMaxLength(255);

            Property(t => t.StopHeadsign)
                .HasMaxLength(255);

            // Table & Column Mappings
            ToTable("Stop_times");
            Property(t => t.Id).HasColumnName("ID");
            Property(t => t.TripId).HasColumnName("trip_id");
            Property(t => t.ArrivalTimeRaw).HasColumnName("arrival_time");
            Property(t => t.DepartureTimeRaw).HasColumnName("departure_time");
            Property(t => t.StopId).HasColumnName("stop_id");
            Property(t => t.StopSequence).HasColumnName("stop_sequence");
            Property(t => t.StopHeadsign).HasColumnName("stop_headsign");
            Property(t => t.PickupType).HasColumnName("pickup_type");
            Property(t => t.DropOffType).HasColumnName("drop_off_type");
            Property(t => t.ShapeDistTraveled).HasColumnName("shape_dist_traveled");
        }
    }
}
