using FileMonitoring.Common.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using Microsoft.Extensions.Logging;
using Prometheus;

namespace FileMonitoring.Common.ApiClient
{
    public class ApiClient
    {
        private static MonitoringConfig _config;
        private static ILogger _logger;
        private static IMetricServer _metricServer;
        private static readonly Gauge FailedApiIntegration = Metrics
            .CreateGauge("ApiIntegration_jobs_exception_total", "Number of API integration operations that failed.",
                new GaugeConfiguration()
                { 
                    LabelNames = new [] {"ExceptionMessage"}
                });

        public ApiClient(MonitoringConfig config, ILogger logger, IMetricServer metricServer)
        {
            _metricServer = metricServer;
            _config = config;
            _logger = logger;
        }

        private static IntegrationModel CastSendingTasksListToIntegrationModel(SendingTask task, string recipient,
            string method, string uid)
        {
            var model = new IntegrationModel()
            {
                dataModel = new Models.FileInfo()
                {
                    name = task.Name,
                    path = task.Path
                },
                recipient = recipient,
                uid = uid.ToString(),
                method = method,
                envCode = _config.FileMonitoringEnvCode
            };
                

            return model;
        }

        public ApiResponseStatus SendFileInfoRequest(SendingTask task, string recipient, string uid)
        {
            var model = CastSendingTasksListToIntegrationModel(task, recipient, _config.Method, uid);
            var jsonBody = JsonHelper.ConvertIntegrationModelToJson(model);

            try
            {
                var request = WebRequest.Create(_config.ServiceIntegrationUrl);
                request.Method = "POST";
                var byteArray = Encoding.UTF8.GetBytes(jsonBody);
                request.ContentType = "application/json";
                request.ContentLength = byteArray.Length;
                request.Timeout = _config.ApiTimeout;

                using (var dataStream = request.GetRequestStream())
                {
                    dataStream.Write(byteArray, 0, byteArray.Length);
                }


                var address = $"Адрес шины. :\n {_config.ServiceIntegrationUrl}";
                _logger.LogInformation($"Запрос пытается отправиться на шину. JSON body:\n {jsonBody}");
                _logger.LogInformation(address);

                if (!GetResponse(request))
                { 
                    FailedApiIntegration.WithLabels($"JSON body:\n {jsonBody}").Set(1);
                    throw new ApplicationException($"Ошибка при отправке запроса. JSON body:\n {jsonBody}");
                }
                    
                _logger.LogInformation($"Запрос успешно отправлен на шину. JSON body:\n {jsonBody}");

                return ApiResponseStatus.SuccessfullyProcessed;
            }
            catch (WebException webException)
            {
                try
                {
                    using (var webResponse = webException.Response)
                    {
                        if (webResponse != null)
                        {
                            using (var receiveStream = webResponse.GetResponseStream())
                            {
                                var encode = Encoding.GetEncoding("utf-8");
                                using (var readStream = new StreamReader(receiveStream, encode))
                                {
                                    //длина строки в консоли
                                    var readBytes = new char[256];
                                    var errorMessageBuilder = new StringBuilder();

                                    var count = readStream.Read(readBytes, 0, 256);
                                    while (count > 0)
                                    {
                                        errorMessageBuilder.Append(new string(readBytes, 0, count));
                                        count = readStream.Read(readBytes, 0, 256);
                                    }

                                    readStream.Close();
                                    webResponse.Close();
                                    var rawResponse = errorMessageBuilder.ToString();
                                    FailedApiIntegration.WithLabels(webException.Message + $"Response: {rawResponse}\n Request Body: {jsonBody}").Set(1);
                                    _logger.LogError(webException, $"Ошибка при отправке запроса.Response: {rawResponse}\n Request Body: {jsonBody}");

                                    return rawResponse.Contains(Constants.ConnectExceptionMessage)
                                        ? ApiResponseStatus.BusConnectException
                                        : ApiResponseStatus.InternalBusError;
                                }
                            }
                        }
                        else
                        {
                            FailedApiIntegration.WithLabels(webException.Message + $"{_config.ServiceIntegrationUrl}, Request Body: {jsonBody}").Set(1);
                            _logger.LogError(webException, $"Сервер не отвечает по адресу {_config.ServiceIntegrationUrl}, Request Body: {jsonBody}");                           
                            return ApiResponseStatus.ServerIsNotAvailable;
                        }
                    }
                }
                catch (Exception ex)
                {
                    FailedApiIntegration.WithLabels($"Request Body: {jsonBody}. {webException.Message}").Set(1);
                    throw new ApplicationException(
                        $"Ошибка обработки ошибки с сервера, Request Body: {jsonBody}. Ошибка сервера {webException.ToString()}", ex);
                }
            }
        }

        private static bool GetResponse(WebRequest request)
        {
            
            using (var response = (HttpWebResponse)request.GetResponse())
            {
                var result = response.StatusCode == HttpStatusCode.OK ||
                             response.StatusCode == HttpStatusCode.Created;

                return result;
            }
        }
    }
}
