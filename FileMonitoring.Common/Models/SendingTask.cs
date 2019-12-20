using System;

namespace FileMonitoring.Common.Models
{
    public class SendingTask
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Path { get; set; }
		public string Recipient { get; set; }
        public DateTime CreateDate { get; set; }
        public DateTime? ProcessDate { get; set; }
        public TaskStatus Status { get; set; }
        public Guid? Uid { get; set; }
    }
}