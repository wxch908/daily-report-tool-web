using SqlSugar;
using DailyReport.Api.Entities;

namespace DailyReport.Api
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            var dataDirectory = Path.Combine(
                builder.Environment.ContentRootPath,
                "Data"
            );
            Directory.CreateDirectory(dataDirectory);
            var databasePath = Path.Combine(
                dataDirectory,
                "daily-report.db"
            );
            builder.Services.AddSingleton<ISqlSugarClient>(_ =>
            {
                return new SqlSugarScope(new ConnectionConfig
                {
                    ConnectionString = $"DataSource={databasePath}",
                    DbType = DbType.Sqlite,
                    IsAutoCloseConnection = true,
                    InitKeyType = InitKeyType.Attribute
                });
            });

            var app = builder.Build();

            var database = app.Services
                .GetRequiredService<ISqlSugarClient>();

            database.DbMaintenance.CreateDatabase();

            database.CodeFirst.InitTables<
                DailyReportEntity,
                DailyReportItemEntity
            >();
            var itemTableExists = database.DbMaintenance.IsAnyTable(
                "daily_report_item",
                false
            );
            Console.WriteLine(
                $"数据库初始化完成，工作项表存在：{itemTableExists}"
            );

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            if (!app.Environment.IsDevelopment())
            {
                app.UseHttpsRedirection();
            }

            app.UseAuthorization();
            app.MapControllers();

            app.Run();
        }
    }
}
