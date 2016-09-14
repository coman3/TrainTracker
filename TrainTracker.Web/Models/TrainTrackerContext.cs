using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using TrainTracker.Models;
using TrainTracker.Web.Models.Mapping;

namespace TrainTracker.Web.Models
{
    public partial class TrainTrackerContext : DbContext
    {
        static TrainTrackerContext()
        {
            Database.SetInitializer<TrainTrackerContext>(null);
        }

        public TrainTrackerContext()
            : base("Name=TrainTrackerContext")
        {
        }

        public DbSet<Calendar> Calendars { get; set; }
        public DbSet<CalendarDate> CalendarDates { get; set; }
        public DbSet<Route> Routes { get; set; }
        public DbSet<Shape> Shapes { get; set; }
        public DbSet<StopTime> StopTimes { get; set; }
        public DbSet<Stop> Stops { get; set; }
        public DbSet<Trip> Trips { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Configurations.Add(new CalendarMap());
            modelBuilder.Configurations.Add(new CalendarDatesMap());
            modelBuilder.Configurations.Add(new RouteMap());
            modelBuilder.Configurations.Add(new ShapeMap());
            modelBuilder.Configurations.Add(new StopTimesMap());
            modelBuilder.Configurations.Add(new StopMap());
            modelBuilder.Configurations.Add(new TripMap());
        }
    }
}
