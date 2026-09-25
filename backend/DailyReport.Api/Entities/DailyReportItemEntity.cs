using SqlSugar;

namespace DailyReport.Api.Entities
{
    [SugarTable("daily_report_item")]
    [SugarIndex(
        "idx_daily_report_item_report_sort",
        nameof(DailyReportId),
        OrderByType.Asc,
        nameof(SortOrder),
        OrderByType.Asc
    )]
    public class DailyReportItemEntity
    {
        [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
        public int Id { get; set; }

        public int DailyReportId { get; set; }

        [SugarColumn(Length = 500)]
        public string Description { get; set; } = string.Empty;

        public decimal Hours { get; set; }

        public int SortOrder { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime UpdatedAt { get; set; }
    }
}
