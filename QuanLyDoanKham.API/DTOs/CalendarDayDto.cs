namespace QuanLyDoanKham.API.DTOs
{
    public class CalendarDayDto
    {
        public DateTime Date { get; set; }
        public List<CalendarGroupDto> Groups { get; set; } = new List<CalendarGroupDto>();
    }

    public class CalendarGroupDto
    {
        public int GroupId { get; set; }
        public string GroupName { get; set; } = null!;
        public string? CompanyName { get; set; }
        public string Status { get; set; } = null!;
        public int AssignedStaffCount { get; set; }
    }

    public class MyScheduleDto
    {
        public int GroupId { get; set; }
        public string GroupName { get; set; } = null!;
        public DateTime ExamDate { get; set; }
        public string? PositionName { get; set; }
        public string? CompanyName { get; set; }
    }
}
