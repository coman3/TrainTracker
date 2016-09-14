using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using TrainTracker.Models;

namespace TrainTracker.Web.Models.Mapping
{
    public class RouteMap : EntityTypeConfiguration<Route>
    {
        public RouteMap()
        {
            // Primary Key
            HasKey(t => t.RouteId);

            // Properties
            Property(t => t.RouteId)
                .IsRequired()
                .HasMaxLength(255);

            Property(t => t.AgencyId)
                .HasMaxLength(255);

            Property(t => t.RouteShortName)
                .IsRequired()
                .HasMaxLength(255);

            Property(t => t.RouteLongName)
                .IsRequired()
                .HasMaxLength(255);

            // Table & Column Mappings
            ToTable("Routes");
            Property(t => t.RouteId).HasColumnName("route_id");
            Property(t => t.AgencyId).HasColumnName("agency_id");
            Property(t => t.RouteShortName).HasColumnName("route_short_name");
            Property(t => t.RouteLongName).HasColumnName("route_long_name");
            Property(t => t.RouteType).HasColumnName("route_type");
        }
    }
}
