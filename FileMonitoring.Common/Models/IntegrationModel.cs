using System.Collections.Generic;

namespace FileMonitoring.Common.Models
{
    public class IntegrationModel
    {
        public FileInfo dataModel { get; set; }
        public string recipient { get; set; }
        public string method { get; set; }
        public string uid { get; set; }

        public string envCode
        {
            get; set;
        }
    }

    public class FileInfo
    {
       // public string mime { get; set; }
        public string name { get; set; }
        public string path { get; set; }
    }
}