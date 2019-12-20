using System;
using System.IO;
using FileMonitoring.Common.Models;
using Microsoft.Extensions.Logging;
using Prometheus;
using SharpCifs.Smb;
using FileInfo = System.IO.FileInfo;


namespace FileMonitoring.Common.FileManager
{
    public class FileManager
    {
        private static MonitoringConfig _config;
        private static ILogger _logger;
        private static ShareFolder _shareFolder;
        private static IMetricServer _metricServer;
        private static readonly Gauge FailedCopyingFiles= Metrics
            .CreateGauge("CopyingFiles_jobs_exception_total", "Number of Copying Files operations that failed.",
                new GaugeConfiguration()
                { 
                    LabelNames = new [] {"ExceptionMessage"}
                });

        
        public FileManager(MonitoringConfig config, ILogger logger, ShareFolder shareFolder, IMetricServer metricServer)
        {
            _shareFolder = shareFolder;
            _config = config;
            _logger = logger;
            _metricServer = metricServer;
        }

        public TaskStatus SendFile(string file)
        {
            TaskStatus status = TaskStatus.Completed;
            _logger.LogInformation("Начинаю Копировать");
            if (_shareFolder.Type == (int)OperationType.Cmb)
                status = SendFileSmb(file);
            if (_shareFolder.Type == (int) OperationType.Copy)
                status = CopyFile(file);
            return status;
        }

        private TaskStatus CopyFile(string file)
        {
            try
            {
                if (File.Exists(file))
                {
                    File.Copy(file, Path.Combine(_shareFolder.Address, Path.GetFileName(file)), true);
                    File.Delete(file);
                    _logger.LogInformation("Успешно");
                    return TaskStatus.Completed;
                }
                FailedCopyingFiles.WithLabels("Файл недоступен").Set(1);
                _logger.LogInformation("Файл недоступен");
                return TaskStatus.Error;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                FailedCopyingFiles.WithLabels(e.Message).Set(1);
                return TaskStatus.Error;
            }
        }

        private TaskStatus SendFileSmb(string file)
        {
            SharpCifs.Config.SetProperty("jcifs.smb.client.lport", "8137");
            var auth = new NtlmPasswordAuthentication(_shareFolder.Domen, _shareFolder.UserName, _shareFolder.Password);
            var address = Path.Combine(@"smb://", _shareFolder.Address, Path.GetFileName(file));
            var smb = new SmbFile(address, auth);
            try
            {
                if(smb.Exists())
                    smb.Delete();
                smb.CreateNewFile();
                using (var writeStream = smb.GetOutputStream())
                {
                    writeStream.Write(GetCopiedFile(file));
                }
                _logger.LogInformation("Успешно");
                return TaskStatus.Completed;
            }
            catch (SmbException smbException)
            {
                FailedCopyingFiles.WithLabels("Ошибка при подключении").Set(1);
                _logger.LogError(smbException, "Ошибка при подключении");
                return TaskStatus.Error;
            }
            catch (Exception ex)
            {
                FailedCopyingFiles.WithLabels("Ошибка при копировании файла").Set(1);
                _logger.LogError(ex, "Ошибка при копировании файла");
                return TaskStatus.Error;
            }
        }
        
        private static byte[] GetCopiedFile(string file)
        {
            if (File.Exists(file))
            {
                var tempDir = Directory.CreateDirectory(Path.Combine(Path.GetTempPath() + "Gisoms"));
                File.Copy(file, Path.Combine(tempDir.FullName, Path.GetFileName(file)));
                var tempFile = File.ReadAllBytes(Path.Combine(tempDir.FullName, Path.GetFileName(file)));
                DeleteTempDir(tempDir.FullName);
                var wasteFolder = _config.PathToWasteFolder;
                if (string.IsNullOrEmpty(wasteFolder))
                    MoveToWasteFolder(_config.PathToWasteFolder, file);
                else
                {
                    File.Delete(file);
                }
                return tempFile;
            }
            return null;
        }

        private static void MoveToWasteFolder(string pathToWasteFolder, string file)
        {
            DirectoryInfo directoryInfo = new DirectoryInfo(pathToWasteFolder);
            if (!directoryInfo.Exists)
            {
                Directory.CreateDirectory(pathToWasteFolder);
            }
            if(File.Exists(file))
                File.Move(file, Path.Combine(pathToWasteFolder, Path.GetFileName(file)));
        }

        private static void DeleteTempDir(string pathToTempDirectory)
        {
            DirectoryInfo di = new DirectoryInfo(pathToTempDirectory);
           
            foreach (FileInfo file in di.GetFiles())
            {
                file.Delete(); 
            }
            foreach (DirectoryInfo dir in di.GetDirectories())
            {
                dir.Delete(true); 
            }
        }
    }
}