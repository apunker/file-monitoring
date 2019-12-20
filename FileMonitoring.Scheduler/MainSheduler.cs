using System;
using FileMonitoring.Common.Models;
using FileMonitoring.Scheduler.Jobs;
using FluentScheduler;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Prometheus;

namespace FileMonitoring.Scheduler
{
    public class MainSheduler : IDisposable
    {
        internal static ILogger Logger;
        internal static MonitoringConfig MonitoringConfig;
        internal static IConfiguration AppConfiguration;
        internal static Gauge CounterException;
        internal static IMetricServer MetricServer;

        public MainSheduler(MonitoringConfig config, ILogger logger, IConfiguration configuration, Gauge counterException, IMetricServer metricServer = null)
        {
            Logger = logger;
            MetricServer = metricServer;
            CounterException = counterException;
            MonitoringConfig = config;
            AppConfiguration = configuration;
            var registry = new Registry();
            registry.Schedule<FileMonitoringJob>().NonReentrant().ToRunEvery(config.FileMonitoringTimeout).Seconds();
            registry.Schedule<ApiIntegrationJob>().NonReentrant().ToRunEvery(config.ApiIntegrationTimeout).Seconds();
            JobManager.Initialize(registry);
        }

        public void Start()
        {
            JobManager.Start();
        }

        public void Dispose()
        {
            JobManager.Stop();
        }
    }
}