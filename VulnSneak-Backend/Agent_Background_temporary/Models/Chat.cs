namespace Agent_Background_temporary.Models
{
    public class Chat
    {
        public int Id { get; set; }
        public string? ChatName { get; set; }
        public required string ChatFolderPath { get; set; }
        public int UserId { get; set; }
        public DateTime CreatedAt { get; set; }
        public User? User { get; set; }
        public ICollection<ScanSession> ScanSessions { get; set; } = new List<ScanSession>();

    }

}