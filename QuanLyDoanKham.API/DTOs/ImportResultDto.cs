namespace QuanLyDoanKham.API.DTOs
{
    public class ImportResultDto
    {
        public int TotalRows { get; set; }
        public int ImportedCount { get; set; }
        public int SkippedCount { get; set; }
        public int ErrorCount { get; set; }
        public List<string> Errors { get; set; } = new List<string>();
    }
}
