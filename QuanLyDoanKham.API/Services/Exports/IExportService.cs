namespace QuanLyDoanKham.API.Services.Exports
{
    public interface IExportService
    {
        Task<byte[]> ExportPatientsByGroupAsync(int groupId);
        Task<byte[]> ExportGroupPnlAsync(int groupId);
        Task<byte[]> ExportPayrollAsync(int month, int year);
    }
}
