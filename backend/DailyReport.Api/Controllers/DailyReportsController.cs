using Microsoft.AspNetCore.Mvc;
using DailyReport.Api.Entities;
using SqlSugar;

namespace DailyReport.Api.Controllers
{
    [ApiController]
    [Route("api/daily-reports")]
    public class DailyReportsController : ControllerBase
    {
        private readonly ISqlSugarClient _database;

        public DailyReportsController(ISqlSugarClient database)
        {
            _database = database;
        }

        [HttpGet]
        public async Task<ActionResult<List<DailyReportEntity>>> GetAll()
        {
            var reports = await _database
                .Queryable<DailyReportEntity>()
                .OrderBy(
                    report => report.ReportDate,
                    OrderByType.Desc
                )
                .ToListAsync();

            return Ok(reports);
        }

        [HttpGet("status")]
        public IActionResult GetStatus()
        {
            return Ok(new
            {
                Message = "日报 API 连接成功",
                ServerTime = DateTimeOffset.Now
            });
        }
    }
}
