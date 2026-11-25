using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;

namespace Agent_Background_temporary.Models
{
    public class VulnResult
    {
        public int Id { get; set; }

        public int VulnTypeId { get; set; }
        public VulnType? VulnType { get; set; }

        public int ScanId { get; set; }
        public ScanSession? ScanSession { get; set; }

        public int StartLine { get; set; }
        public int EndLine { get; set; }
        public string? CodeSnippet { get; set; }
        public double Confidence { get; set; }
        public string? RepairedCode { get; set; }
        public string? Explanation { get; set; }
        public string? ModelUsed { get; set; }
        public bool RepairSuccess { get; set; }
        public string? RepairError { get; set; }
        public double ElapsedSecs { get; set; }

        public string? VulnLinesJson { get; set; } // "[37, 45, 52, 61]"

        [NotMapped]
        public List<int> VulnLines
        {
            get => string.IsNullOrEmpty(VulnLinesJson)
                ? new List<int>()
                : JsonSerializer.Deserialize<List<int>>(VulnLinesJson) ?? new List<int>();
            set => VulnLinesJson = JsonSerializer.Serialize(value);
        }
    }
}
