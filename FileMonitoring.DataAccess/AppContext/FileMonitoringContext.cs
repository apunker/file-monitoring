using FileMonitoring.Common.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System.IO;


namespace FileMonitoring.DataAccess.AppContext
{
    public class FileMonitoringContext : DbContext
    {
        protected string ConnectionString;

        public FileMonitoringContext()
        {
            var connectionStringConfig = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json").Build();
            ConnectionString = connectionStringConfig.GetConnectionString(Constants.DefaultConnectionName);
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseNpgsql(ConnectionString, x => x.MigrationsHistoryTable("__EFMigrationsHistory", Constants.DefaultSchemaName));
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasDefaultSchema(Constants.DefaultSchemaName);
            modelBuilder.Entity<SendingTask>().HasIndex(i => new { i.Id });
            modelBuilder.Entity<SendingTask>().HasIndex(i => new { i.Status });
            modelBuilder.Entity<SendingTask>().HasIndex(i => new { i.Path, i.Name, i.CreateDate });
            modelBuilder.Entity<SendingFile>().HasIndex(i => new { i.Id });
            modelBuilder.Entity<SendingFile>().HasIndex(i => new { i.Status });
            modelBuilder.Entity<SendingFile>().HasIndex(i => new { i.Path, i.FileName, i.CreateDate });
        }

        public FileMonitoringContext(string connectionString)
        {
            ConnectionString = connectionString;
        }

        public DbSet<SendingTask> SendingTasks { get; set; }
        
        public DbSet<SendingFile> SendingFiles { get; set; }
    }
}