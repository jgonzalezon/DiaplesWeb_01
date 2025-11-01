using System.Collections.Generic;
using DiaplesWeb.Models;

namespace DiaplesWeb.Models.ViewModels
{
    public class PagedEventsViewModel
    {
        public List<EventItem> Events { get; set; } = new();
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalCount { get; set; }
        public int TotalPages => (int)System.Math.Ceiling((double)TotalCount / PageSize);
    }
}