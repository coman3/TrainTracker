using System.Collections.Generic;

namespace TrainTracker.Web.Models
{
    public class PagedList<T>
    {
        public List<T> Data { get; set; }
        public int Returned { get; set; }
        public int PageNumber { get; set; }
        public string NextPageToken { get; set; }
    }
}