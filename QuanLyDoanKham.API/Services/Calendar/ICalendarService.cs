using QuanLyDoanKham.API.DTOs;

namespace QuanLyDoanKham.API.Services.Calendar
{
    public interface ICalendarService
    {
        Task<List<CalendarDayDto>> GetCalendarAsync(int month, int year);
        Task<List<MyScheduleDto>> GetMyScheduleAsync(int userId, int month, int year);
    }
}
