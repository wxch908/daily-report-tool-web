using DailyReport.Api.Contracts;
using DailyReport.Api.Entities;
using Microsoft.AspNetCore.Mvc;
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

        [HttpGet("{id:int}")]
        public async Task<ActionResult<DailyReportEntity>> GetById(int id)
        {
            var report = await _database
                .Queryable<DailyReportEntity>()
                .FirstAsync(report => report.Id == id);

            if (report is null)
            {
                return NotFound();
            }

            return Ok(report);
        }

        [HttpPost]
        public async Task<ActionResult<DailyReportEntity>> Create(
            CreateDailyReportRequest request
        )
        {
            if (request.ReportDate == default)
            {
                return BadRequest(new
                {
                    Message = "请选择日报日期"
                });
            }

            var reportDate = request.ReportDate.ToDateTime(
                TimeOnly.MinValue
            );

            var existingReport = await _database
                .Queryable<DailyReportEntity>()
                .FirstAsync(report => report.ReportDate == reportDate);

            if (existingReport is not null)
            {
                return Conflict(new
                {
                    Message = "该日期的日报已经存在"
                });
            }

            var now = DateTime.Now;

            var report = new DailyReportEntity
            {
                ReportDate = reportDate,
                CreatedAt = now,
                UpdatedAt = now
            };

            report.Id = await _database
                .Insertable(report)
                .ExecuteReturnIdentityAsync();

            return CreatedAtAction(
                nameof(GetById),
                new { id = report.Id },
                report
            );
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
