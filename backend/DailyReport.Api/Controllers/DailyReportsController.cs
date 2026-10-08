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
        public async Task<ActionResult<List<DailyReportEntity>>> GetAll(
            [FromQuery] DateOnly? startDate = null,
            [FromQuery] DateOnly? endDate = null
        )
        {
            if (
                startDate.HasValue &&
                endDate.HasValue &&
                startDate.Value > endDate.Value
            )
            {
                return BadRequest(new
                {
                    Message = "开始日期不能晚于结束日期"
                });
            }

            var query = _database
                .Queryable<DailyReportEntity>();

            if (startDate.HasValue)
            {
                var start = startDate.Value.ToDateTime(
                    TimeOnly.MinValue
                );

                query = query.Where(
                    report => report.ReportDate >= start
                );
            }

            if (endDate.HasValue)
            {
                var end = endDate.Value.ToDateTime(
                    TimeOnly.MinValue
                );

                query = query.Where(
                    report => report.ReportDate <= end
                );
            }

            var reports = await query
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

            var validationError = ValidateItem(
            description,
            request.Hours
);

            if (validationError is not null)
            {
                return BadRequest(new
                {
                    Message = validationError
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

        [HttpPut("{reportId:int}/items/{itemId:int}")]
        public async Task<ActionResult<DailyReportItemResponse>> UpdateItem(
        int reportId,
        int itemId,
        UpdateDailyReportItemRequest request
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

            var item = await _database
                .Queryable<DailyReportItemEntity>()
                .FirstAsync(item =>
                    item.Id == itemId &&
                    item.DailyReportId == reportId
                );

            if (item is null)
            {
                return NotFound(new
                {
                    Message = "工作项不存在"
                });
            }

            var description = (
                request.Description ?? string.Empty
            ).Trim();

            var validationError = ValidateItem(
                description,
                request.Hours
            );

            if (validationError is not null)
            {
                return BadRequest(new
                {
                    Message = validationError
                });
            }

            var now = DateTime.Now;

            item.Description = description;
            item.Hours = request.Hours;
            item.UpdatedAt = now;

            report.UpdatedAt = now;

            await _database.Ado.BeginTranAsync();

            try
            {
                await _database
                    .Updateable(item)
                    .ExecuteCommandAsync();

                await _database
                    .Updateable(report)
                    .ExecuteCommandAsync();

                await _database.Ado.CommitTranAsync();
            }
            catch
            {
                await _database.Ado.RollbackTranAsync();
                throw;
            }

            return Ok(ToItemResponse(item));
        }

        [HttpDelete("{reportId:int}/items/{itemId:int}")]
        public async Task<IActionResult> DeleteItem(
        int reportId,
        int itemId
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

            var item = await _database
                .Queryable<DailyReportItemEntity>()
                .FirstAsync(item =>
                    item.Id == itemId &&
                    item.DailyReportId == reportId
                );

            if (item is null)
            {
                return NotFound(new
                {
                    Message = "工作项不存在"
                });
            }

            var now = DateTime.Now;

            await _database.Ado.BeginTranAsync();

            try
            {
                await _database
                    .Deleteable<DailyReportItemEntity>()
                    .Where(item =>
                        item.Id == itemId &&
                        item.DailyReportId == reportId
                    )
                    .ExecuteCommandAsync();

                var remainingItems = await _database
                    .Queryable<DailyReportItemEntity>()
                    .Where(item =>
                        item.DailyReportId == reportId
                    )
                    .OrderBy(
                        item => item.SortOrder,
                        OrderByType.Asc
                    )
                    .ToListAsync();

                for (
                    var index = 0;
                    index < remainingItems.Count;
                    index++
                )
                {
                    var remainingItem = remainingItems[index];

                    remainingItem.SortOrder = index + 1;
                    remainingItem.UpdatedAt = now;

                    await _database
                        .Updateable(remainingItem)
                        .ExecuteCommandAsync();
                }

                report.UpdatedAt = now;

                await _database
                    .Updateable(report)
                    .ExecuteCommandAsync();

                await _database.Ado.CommitTranAsync();
            }
            catch
            {
                await _database.Ado.RollbackTranAsync();
                throw;
            }

            return NoContent();
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
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

            await _database.Ado.BeginTranAsync();

            try
            {
                await _database
                    .Deleteable<DailyReportItemEntity>()
                    .Where(item => item.DailyReportId == id)
                    .ExecuteCommandAsync();

                await _database
                    .Deleteable<DailyReportEntity>()
                    .Where(report => report.Id == id)
                    .ExecuteCommandAsync();

                await _database.Ado.CommitTranAsync();
            }
            catch
            {
                await _database.Ado.RollbackTranAsync();
                throw;
            }

            return NoContent();
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

        private static string? ValidateItem(
        string description,
        decimal hours
)
        {
            if (string.IsNullOrWhiteSpace(description))
            {
                return "工作内容不能为空";
            }

            if (description.Length > 500)
            {
                return "工作内容不能超过 500 个字符";
            }

            if (
                hours < 0.5m ||
                hours > 8m ||
                hours % 0.5m != 0
            )
            {
                return "工时必须是 0.5 到 8 之间的 0.5 倍数";
            }

            return null;
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
