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
        public async Task<ActionResult<DailyReportDetailResponse>> GetById(
            int id
        )
        {
            var report = await _database
                .Queryable<DailyReportEntity>()
                .FirstAsync(report => report.Id == id);

            if (report is null)
            {
                return NotFound(new
                {
                    Message = "日报不存在"
                });
            }

            var items = await _database
                .Queryable<DailyReportItemEntity>()
                .Where(item => item.DailyReportId == id)
                .OrderBy(
                    item => item.SortOrder,
                    OrderByType.Asc
                )
                .ToListAsync();

            var response = new DailyReportDetailResponse
            {
                Id = report.Id,
                ReportDate = DateOnly.FromDateTime(
                    report.ReportDate
                ),
                CreatedAt = report.CreatedAt,
                UpdatedAt = report.UpdatedAt,
                TotalHours = items.Sum(item => item.Hours),
                Items = items
                    .Select(ToItemResponse)
                    .ToList()
            };

            return Ok(response);
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

        [HttpPost("{reportId:int}/items")]
        public async Task<ActionResult<DailyReportItemResponse>> CreateItem(
            int reportId,
            CreateDailyReportItemRequest request
        )
        {
            var report = await _database
                .Queryable<DailyReportEntity>()
                .FirstAsync(report => report.Id == reportId);

            if (report is null)
            {
                return NotFound(new
                {
                    Message = "日报不存在"
                });
            }

            var description = (
                request.Description ?? string.Empty
            ).Trim();

            if (string.IsNullOrWhiteSpace(description))
            {
                return BadRequest(new
                {
                    Message = "工作内容不能为空"
                });
            }

            if (description.Length > 500)
            {
                return BadRequest(new
                {
                    Message = "工作内容不能超过 500 个字符"
                });
            }

            if (
                request.Hours < 0.5m ||
                request.Hours > 8m ||
                request.Hours % 0.5m != 0
            )
            {
                return BadRequest(new
                {
                    Message = "工时必须是 0.5 到 8 之间的 0.5 倍数"
                });
            }

            var lastItem = await _database
                .Queryable<DailyReportItemEntity>()
                .Where(item => item.DailyReportId == reportId)
                .OrderBy(
                    item => item.SortOrder,
                    OrderByType.Desc
                )
                .FirstAsync();

            var nextSortOrder =
                (lastItem?.SortOrder ?? 0) + 1;

            var now = DateTime.Now;

            var item = new DailyReportItemEntity
            {
                DailyReportId = reportId,
                Description = description,
                Hours = request.Hours,
                SortOrder = nextSortOrder,
                CreatedAt = now,
                UpdatedAt = now
            };

            item.Id = await _database
                .Insertable(item)
                .ExecuteReturnIdentityAsync();

            report.UpdatedAt = now;

            await _database
                .Updateable(report)
                .ExecuteCommandAsync();

            var response = ToItemResponse(item);

            return CreatedAtAction(
                nameof(GetById),
                new { id = reportId },
                response
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

        private static DailyReportItemResponse ToItemResponse(
            DailyReportItemEntity item
        )
        {
            return new DailyReportItemResponse
            {
                Id = item.Id,
                DailyReportId = item.DailyReportId,
                Description = item.Description,
                Hours = item.Hours,
                SortOrder = item.SortOrder,
                CreatedAt = item.CreatedAt,
                UpdatedAt = item.UpdatedAt
            };
        }
    }
}
