namespace DailyReport.Api.Contracts
{
    public class UpdateDailyReportItemRequest
    {
        public string Description { get; set; } = string.Empty;

        public decimal Hours { get; set; }
    }
}
