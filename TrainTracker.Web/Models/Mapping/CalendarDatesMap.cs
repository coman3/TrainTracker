using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using TrainTracker.Models;

namespace TrainTracker.Web.Models.Mapping
{
    public class CalendarDatesMap : EntityTypeConfiguration<CalendarDate>
    {
        public CalendarDatesMap()
        {
            // Primary Key
            HasKey(t => t.Id);

            // Properties
            Property(t => t.Id)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.None);

            Property(t => t.ServiceId)
                .IsRequired()
                .HasMaxLength(255);

            Property(t => t.Date)
                .IsRequired()
                .HasMaxLength(255);

            // Table & Column Mappings
            ToTable("Calendar_dates");
            Property(t => t.Id).HasColumnName("id");
            Property(t => t.ServiceId).HasColumnName("service_id");
            Property(t => t.Date).HasColumnName("date");
            Property(t => t.ExceptionType).HasColumnName("exception_type");
        }
    }
}
