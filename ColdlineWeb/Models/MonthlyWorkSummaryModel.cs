namespace ColdlineWeb.Models
{
    public class MonthlyWorkSummaryModel
    {
        public DateTime Day { get; set; }
        public string TotalHours { get; set; } = "00:00:00";
        public int ProcessCount { get; set; }
    }
}
