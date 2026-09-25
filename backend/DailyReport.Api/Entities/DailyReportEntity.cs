using SqlSugar;

namespace DailyReport.Api.Entities
{
    [SugarTable("daily_report")]
    public class DailyReportEntity
    {
        [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
        public int Id { get; set; }

        public DateTime ReportDate { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime UpdatedAt { get; set; }
    }
}
