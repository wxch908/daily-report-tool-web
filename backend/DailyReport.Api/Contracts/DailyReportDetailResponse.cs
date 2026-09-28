namespace DailyReport.Api.Contracts
{
    public class DailyReportDetailResponse
    {
        public int Id { get; set; }

        public DateOnly ReportDate { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime UpdatedAt { get; set; }

        public decimal TotalHours { get; set; }

        public List<DailyReportItemResponse> Items { get; set; } = new();
    }
    public class DailyReportItemResponse
    {
        public int Id { get; set; }

        public int DailyReportId { get; set; }

        public string Description { get; set; } = string.Empty;

        public decimal Hours { get; set; }

        public int SortOrder { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime UpdatedAt { get; set; }
    }
}
