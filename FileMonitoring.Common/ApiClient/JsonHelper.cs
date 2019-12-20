using FileMonitoring.Common.Models;
using Newtonsoft.Json;

namespace FileMonitoring.Common.ApiClient
{
    public static class JsonHelper
    {
        public static string ConvertIntegrationModelToJson(IntegrationModel model)
        {
            return JsonConvert.SerializeObject(model).ToString();
        }
    }
}
