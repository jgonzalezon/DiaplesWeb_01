using DiaplesWeb.Models;

namespace DiaplesWeb.Services.Contracts
{
    public record CalendarItemDto(int Id, string Title, DateTime Date, string? Location,
                                  AttendanceStatus Status, string[] ClassNames, object ExtendedProps);
}