using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights.Extensibility;

namespace TrainTracker.Web.Helpers
{
    public class TelemetryProcessor : ITelemetryProcessor
    {
        private ITelemetryProcessor Next { get; set; }

        // Link processors to each other in a chain.
        public TelemetryProcessor(ITelemetryProcessor next)
        {
            Next = next;
        }
        public void Process(ITelemetry item)
        {
            if (IsSQLDependency(item))
                return; 
            Next.Process(item);
        }

        private bool IsSQLDependency(ITelemetry item)
        {
            var dependency = item as DependencyTelemetry;
            if (dependency?.DependencyTypeName == "SQL")
            {
                return true;
            }
            return false;
        }
    }
}