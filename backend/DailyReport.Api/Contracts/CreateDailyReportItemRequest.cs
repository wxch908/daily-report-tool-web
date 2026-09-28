namespace DailyReport.Api.Contracts
{
    public class CreateDailyReportItemRequest
    {
        public string Description { get; set; } = string.Empty;
        public decimal Hours { get; set; }
    }
}
