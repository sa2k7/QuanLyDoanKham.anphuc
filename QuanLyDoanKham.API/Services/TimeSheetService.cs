namespace QuanLyDoanKham.API.Services;

public class TimeSheetService
{
    public decimal ComputeWorkUnits(DateTime checkIn, DateTime checkOut, decimal halfDayHours = 4m)
    {
        var hours = (decimal)(checkOut - checkIn).TotalHours;
        if (hours <= 0) return 0;
        if (hours < halfDayHours) return 0.5m;
        return 1.0m;
    }
}
