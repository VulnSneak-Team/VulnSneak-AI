namespace Agent_Background_temporary.Models
{
    public class FileReport
    {
        public int ScanId { get; set; }
        public ScanSession? ScanSession { get; set; }
        public string FileReportName { get; set; } = null!;
        public string FileReportPath { get; set; } = null!;
    }
}
