using FileMonitoring.Common.Models;
using FileMonitoring.DataAccess.AppContext;
using FileMonitoring.Scheduler;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;
using System;
using System.ComponentModel;
using Prometheus;

namespace FileMonitoring
{
    class Program
    {
        private static readonly Counter FailedMigrateDb  = Metrics
            .CreateCounter("FileMonitoring_jobs_processed_total", "Number of import operations that failed.",
                new CounterConfiguration
                {
                    LabelNames = new [] {"ExceptionMessage"}
                });

        private static readonly Gauge JobsGaugeException = Metrics.CreateGauge("FileMonitoring_jobs","1=bug 0=fixed", new GaugeConfiguration
        {
            LabelNames = new[] {"ExceptionMessage"}
        });
        
        static void Main(string[] args)
        {
            var configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json")
                .Build();           
           
            Log.Logger = new LoggerConfiguration()
                .ReadFrom.Configuration(configuration)
                .CreateLogger();

            var serviceCollection = new ServiceCollection();
            ConfigureServices(serviceCollection);
            
            var serviceProvider = serviceCollection.BuildServiceProvider();
            var logger = serviceProvider.GetService<ILogger<Program>>();

            var monitoringConfig = new MonitoringConfig();
            configuration.GetSection("MonitoringConfig").Bind(monitoringConfig);

            var prometheus = new PrometheusConfig();
            configuration.GetSection(Constants.DefaultMetricsServerName).Bind(prometheus);

            MetricPusher metricServer = null; 
            if (!string.IsNullOrEmpty(prometheus.Instance) && !string.IsNullOrEmpty(prometheus.Endpoint))
            {
                metricServer = new MetricPusher(endpoint:prometheus.Endpoint,
                    job:"FileMonitoring", instance:prometheus.Instance);
                metricServer.Start();
            }
            
            logger.LogInformation("Приложение запущено");
            if (FailedMigrateDb.CountExceptions(() => MigrateDb(configuration, logger)))
            {
                //запуск планировщика задач
                var mainSheduler = new MainSheduler(monitoringConfig, logger, configuration, JobsGaugeException, metricServer);
                logger.LogInformation("Для завершения работы нажмите клавишу Q");
                var stopKey = new ConsoleKeyInfo();
                while (stopKey.Key != ConsoleKey.Q)
                {
                    stopKey = Console.ReadKey();
                }
                mainSheduler.Dispose();
                metricServer?.Stop();
            }
            logger.LogInformation("Приложение завершило работу");
        }
        
        private static void ConfigureServices(IServiceCollection services)
        {
            services.AddEntityFrameworkNpgsql().AddDbContext<FileMonitoringContext>().BuildServiceProvider();
            services.AddLogging(configure => configure.AddSerilog());
            services.AddSingleton(typeof(ILogger<>), typeof(Logger<>));
        }

        private static bool MigrateDb(IConfiguration configuration, Microsoft.Extensions.Logging.ILogger logger)
        {
            try
            {
                logger.LogInformation("Запуск мигратора");
                using (var dbContext = new FileMonitoringContext(configuration.GetConnectionString(Constants.DefaultConnectionName)))
                {
                    dbContext.Database.Migrate();
                }
                logger.LogInformation("Мигратор успешно отработал");
                return true;
            }
            catch (Exception ex)
            {
                FailedMigrateDb.WithLabels(ex.Message).Inc();
                logger.LogError(ex, "Произошла ошибка в работе мигратора");
                return false;
            }
        }
    }
}