using FileMonitoring.Common.ApiClient;
using FileMonitoring.Common.Models;
using FileMonitoring.DataAccess.AppContext;
using FileMonitoring.DataAccess.Services;
using FluentScheduler;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using Prometheus;

namespace FileMonitoring.Scheduler.Jobs
{
    public class ApiIntegrationJob: IJob
    {
        public void Execute()
        {
            try
            {
                using (var dbContext = new FileMonitoringContext(MainSheduler.AppConfiguration.GetConnectionString(Constants.DefaultConnectionName)))
                {
                    var dataService = new FileMonitoringDataService(dbContext);
                    var tasks = dataService.GetNewSendingTasks();
                    if (tasks.Count <= 0) return;

                    var apiClient = new ApiClient(MainSheduler.MonitoringConfig, MainSheduler.Logger, MainSheduler.MetricServer);

                    foreach (var task in tasks)
                    {
                        var responseStatus = apiClient.SendFileInfoRequest(task, task.Recipient, task.Name);
                        switch (responseStatus)
                        {
                            case ApiResponseStatus.SuccessfullyProcessed:
                                dataService.UpdateProcessedTask(task, TaskStatus.Completed);
                                break;
                            case ApiResponseStatus.ServerIsNotAvailable:
                                break;
                            case ApiResponseStatus.BusConnectException:
                                break;
                            case ApiResponseStatus.InternalBusError:
                                dataService.UpdateProcessedTask(task, TaskStatus.Error);
                                break;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MainSheduler.CounterException.WithLabels("Ошибка сохранения информации о отправленных файлах").Set(1);
                MainSheduler.Logger.LogError(ex, "Ошибка сохранения информации о отправленных файлах");
            }
        }
    }
}