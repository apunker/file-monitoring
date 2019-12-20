using System;
using FileMonitoring.Common.Models;
using FileMonitoring.DataAccess.AppContext;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using Microsoft.EntityFrameworkCore.Scaffolding.Internal;

namespace FileMonitoring.DataAccess.Services
{
    public class FileMonitoringDataService
    {
        private readonly FileMonitoringContext _dbContext;
        private object _lockDb = new object();

        public FileMonitoringDataService(FileMonitoringContext fileMonitoringContext)
        {
            _dbContext = fileMonitoringContext;
        }

        public void SaveSendingTasks(IEnumerable<SendingTask> tasks)
        {
            lock (_lockDb)
            {
                {
                    var sendingNewTasks = tasks.Where(x => !_dbContext.SendingTasks.Any(z => z.Path == x.Path &&
                    z.Name == x.Name && z.CreateDate == x.CreateDate));
                    _dbContext.AddRange(sendingNewTasks);
                    _dbContext.SaveChanges();
                }
            }
        }
        public List<SendingTask> GetNewSendingTasks()
        {
            lock (_lockDb)
            {
                return _dbContext.SendingTasks.Where(x => x.Status == TaskStatus.New).ToList();
            }
        }

        public void UpdateProcessedTask(SendingTask task, TaskStatus status)
        {
            lock (_lockDb)
            {
                var taskInDb = _dbContext.SendingTasks.Single(x => x.Id == task.Id);
                taskInDb.Status = status;
                _dbContext.SaveChanges();
            }
        }
        
        public void UpdateProcessedTask(SendingFile task, TaskStatus status)
        {
            lock (_lockDb)
            {
                var taskInDb = _dbContext.SendingFiles.Single(x => x.Id == task.Id);
                taskInDb.Status = status;
                _dbContext.SaveChanges();
            }
        }
        
        
        public List<SendingFile> GetNewSendingFiles()
        {
            lock (_lockDb)
            {
                return _dbContext.SendingFiles.Where(x => x.Status == TaskStatus.New).ToList();
            }
        }
        
        public void SaveSendingFile(SendingFile file)
        {
            if (!VerifyFile(file)) return;
            lock (_lockDb)
            {
                file.Id = Guid.NewGuid();
                _dbContext.Add(file);
                _dbContext.SaveChanges();
            }
        }

        private bool VerifyFile(SendingFile file)
        {
            lock (_lockDb)
            {
                var sendingFiles = _dbContext.SendingFiles.Where(x =>
                    x.FileName == file.FileName && x.CreateDate == file.CreateDate && x.Path == file.Path &&file.Id == Guid.Empty);
                return !sendingFiles.Any();
            }
        }
    }
}