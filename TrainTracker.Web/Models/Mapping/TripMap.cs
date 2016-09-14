using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using TrainTracker.Models;

namespace TrainTracker.Web.Models.Mapping
{
    public class TripMap : EntityTypeConfiguration<Trip>
    {
        public TripMap()
        {
            // Primary Key
            HasKey(t => t.TripId);

            // Properties
            Property(t => t.RouteId)
                .IsRequired()
                .HasMaxLength(255);

            Property(t => t.ServiceId)
                .IsRequired()
                .HasMaxLength(255);

            Property(t => t.TripId)
                .IsRequired()
                .HasMaxLength(255);

            Property(t => t.ShapeId)
                .HasMaxLength(255);

            Property(t => t.TripHeadsign)
                .HasMaxLength(255);

            // Table & Column Mappings
            ToTable("Trips");
            Property(t => t.RouteId).HasColumnName("﻿route_id");
            Property(t => t.ServiceId).HasColumnName("service_id");
            Property(t => t.TripId).HasColumnName("trip_id");
            Property(t => t.ShapeId).HasColumnName("shape_id");
            Property(t => t.TripHeadsign).HasColumnName("trip_headsign");
            Property(t => t.DirectionId).HasColumnName("direction_id");
        }
    }
}
