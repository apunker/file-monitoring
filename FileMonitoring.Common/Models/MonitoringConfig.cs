using System.Collections.Generic;

namespace FileMonitoring.Common.Models
{
    public class MonitoringConfig
    {
        /// <summary>
        /// Список опрашиваемых папок
        /// </summary>
        public List<Folder> Folders { get; set; }

        /// <summary>
        /// Таймаут опроса папок(секунды)
        /// </summary>
        public int FileMonitoringTimeout { get; set; }

        /// <summary>
        /// Таймаут отправки данных на апи (секунды)
        /// </summary>
        public int ApiIntegrationTimeout { get; set; }
        
        /// <summary>
        /// Строка подключения к базе данных Foms(Gateway)
        /// </summary>
        /// <returns></returns>
        public string FomsDbConnectionString { get; set; }

        /// <summary>
        /// Service Integration Url
        /// </summary>
        public string ServiceIntegrationUrl { get; set; }

        /// <summary>
        /// Метод(Код сервиса)
        /// </summary>
        public string Method { get; set; }

        /// <summary>
        /// Таймаут ответа апи (миллисекунды)
        /// </summary>
        public int ApiTimeout { get; set; } 
        
        /// <summary>
        /// Путь для перемещенных файлов, если не указано удаляем
        /// </summary>
        public string PathToWasteFolder { get; set; }
        /// <summary>
        /// Env code для шины
        /// </summary>
        public string FileMonitoringEnvCode { get; set; }
    }
}