namespace FileMonitoring.Common.Models
{
    public class Folder
    {
        /// <summary>
        /// Путь к папке
        /// </summary>
        public string Path { get; set; }

        /// <summary>
        /// Получатель(Код системы)
        /// </summary>
        public string Recipient { get; set; }

        public ShareFolder ShareFolder { get; set; }
    }

    public class ShareFolder
    {
        public int Type { get; set; }
        public string Address { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public string Domen { get; set; }
    }
}