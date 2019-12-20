namespace FileMonitoring.Common.Models
{
    public enum ApiResponseStatus
    {
        SuccessfullyProcessed,
        ServerIsNotAvailable,
        InternalBusError,
        BusConnectException
    }
}