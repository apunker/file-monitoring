using System;

namespace FileMonitoring.Common.Models
{
    public class SendingFile
    {
        public Guid Id { get; set; }
        public string FileName { get; set; }
        public string Path { get; set; }
        public DateTime CreateDate { get; set; }
        public TaskStatus Status { get; set; }
    }
}