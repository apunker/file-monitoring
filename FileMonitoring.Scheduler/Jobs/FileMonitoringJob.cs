using FileMonitoring.Common.Models;
using FileMonitoring.DataAccess.AppContext;
using FileMonitoring.DataAccess.Services;
using FluentScheduler;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Linq;
using FileMonitoring.Common.FileManager;
using FileInfo = System.IO.FileInfo;

namespace FileMonitoring.Scheduler.Jobs
{
    public class FileMonitoringJob : IJob
    {
        public void Execute()
        {
            try
            {
                foreach (var folder in MainSheduler.MonitoringConfig.Folders)
                {
                    if (Directory.Exists(folder.Path))
                    {
                        var files = Directory.GetFiles(folder.Path);
                        if (files.Length < 1) continue;
                        if (folder.ShareFolder != null)
                        {
                            using (var dbContext = new FileMonitoringContext(
                                MainSheduler.AppConfiguration.GetConnectionString(Constants.DefaultConnectionName)))
                            {
                                var dataService = new FileMonitoringDataService(dbContext);
                                files.ToList().ForEach(file =>
                                {
                                    var fileInfo = new FileInfo(file);
                                    SendingFile sendingFile = new SendingFile();
  
                                    sendingFile.Path = file;
                                    sendingFile.Status = TaskStatus.New;
                                    sendingFile.CreateDate = fileInfo.CreationTime;
                                    sendingFile.FileName = fileInfo.Name;
                                    dataService.SaveSendingFile(sendingFile);
                                });
                            }

                            using (var dbContext = new FileMonitoringContext(
                                MainSheduler.AppConfiguration.GetConnectionString(Constants.DefaultConnectionName)))
                            {
                                var dataService = new FileMonitoringDataService(dbContext);
                                var newSendingFilesFiles = dataService.GetNewSendingFiles();
                                FileManager fileManager = new FileManager(MainSheduler.MonitoringConfig, MainSheduler.Logger, folder.ShareFolder, MainSheduler.MetricServer);
                                foreach (var file in newSendingFilesFiles)
                                {
                                    var taskStatus = fileManager.SendFile(file.Path);
                                    dataService.UpdateProcessedTask(file, taskStatus);
                                }
                            }

                           
                        }
                        else
                        {
                            var tasks = from file in files select CreateSendingTask(file, folder);   
                            using (var dbContext = new FileMonitoringContext(
                                MainSheduler.AppConfiguration.GetConnectionString(Constants.DefaultConnectionName)))
                            {
                                var dataService = new FileMonitoringDataService(dbContext);
                                dataService.SaveSendingTasks(tasks);
                            }
                        } 
                    }
                }
            }
            catch (IOException ex)
            {
                MainSheduler.CounterException.WithLabels("Ошибка при опросе папок").Set(1);
                MainSheduler.Logger.LogError(ex, "Ошибка при опросе папок");
            }
            catch (Exception ex)
            {
                MainSheduler.CounterException.WithLabels("Ошибка при сохранении информации по опрошенным папкам").Set(1);
                MainSheduler.Logger.LogError(ex, "Ошибка при сохранении информации по опрошенным папкам");
            }
        }

        public static SendingTask CreateSendingTask(string file, Folder folder)
        {
            var fileInfo = new System.IO.FileInfo(file);
            
            string recipient;
            if (fileInfo.Name.Substring(0, 1).Contains("Р") || fileInfo.Name.Substring(0, 1).Contains("P"))
            {
                var regionCode = string.Empty;
                if (fileInfo.Name.ToLower().Contains("uprmes"))
                {
                    regionCode = fileInfo.Name.Substring(3, 2);
                }
                else
                {
                    if (fileInfo.Name.ToLower().Contains("uprak"))
                    {
                        regionCode = fileInfo.Name.Substring(1, 2); 
                    }
                }

                var fomsDataService = new FomsDataService(MainSheduler.MonitoringConfig.FomsDbConnectionString,
                    MainSheduler.Logger);
                recipient = fomsDataService.GetOkatoByInstitutionCode(regionCode);
            }
            else
            {
                recipient = (string.IsNullOrEmpty(folder.Recipient))
                    // Получатель первые 5 символов в названии файла если не указан в настройках для папки
                    // Пример для файла 57000-10756-04.uprmes это 57000
                    ? fileInfo.Name.Substring(0, 5)
                    : folder.Recipient;
            }

            return new SendingTask()
            {
                Id = Guid.NewGuid(),
                Status = TaskStatus.New,
                Path = file,
                Recipient = recipient,
                Name = fileInfo.Name,
                CreateDate = fileInfo.CreationTime
            };
        }
        
    }
}