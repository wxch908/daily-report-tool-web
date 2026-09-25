using SqlSugar;

namespace DailyReport.Api.Entities
{
    [SugarTable("daily_report")]
    [SugarIndex(
        "ux_daily_report_report_date",
        nameof(ReportDate),
        OrderByType.Asc,
        true
    )]
    public class DailyReportEntity
    {
        [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
        public int Id { get; set; }

        public DateTime ReportDate { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime UpdatedAt { get; set; }
    }
}
