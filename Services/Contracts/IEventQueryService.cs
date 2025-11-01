using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DiaplesWeb.Models;

namespace DiaplesWeb.Services.Contracts
{
    public interface IEventQueryService
    {
        Task<List<EventItem>> GetAllOrderedAsync();
        Task<EventItem?> FindAsync(int id);

        Task<(List<EventItem> Items, int TotalCount)> GetPagedAsync(int page, int pageSize);

        Task<List<CalendarItemDto>> GetCalendarAsync(
            DateTime from, DateTime to, string userId, Func<int, string> linkBuilder);
    }
}