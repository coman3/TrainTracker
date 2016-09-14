using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using TrainTracker.Models;

namespace TrainTracker.Web.Models.Mapping
{
    public class CalendarMap : EntityTypeConfiguration<Calendar>
    {
        public CalendarMap()
        {
            // Primary Key
            HasKey(t => t.ServiceId);

            // Properties
            Property(t => t.ServiceId)
                .IsRequired()
                .HasMaxLength(255);

            Property(t => t.StartDate)
                .IsRequired()
                .HasMaxLength(255);

            Property(t => t.EndDate)
                .IsRequired()
                .HasMaxLength(255);

            // Table & Column Mappings
            ToTable("Calendar");
            Property(t => t.ServiceId).HasColumnName("service_id");
            Property(t => t.Monday).HasColumnName("monday");
            Property(t => t.Tuesday).HasColumnName("tuesday");
            Property(t => t.Wednesday).HasColumnName("wednesday");
            Property(t => t.Thursday).HasColumnName("thursday");
            Property(t => t.Friday).HasColumnName("friday");
            Property(t => t.Saturday).HasColumnName("saturday");
            Property(t => t.Sunday).HasColumnName("sunday");
            Property(t => t.StartDate).HasColumnName("start_date");
            Property(t => t.EndDate).HasColumnName("end_date");
        }
    }
}
