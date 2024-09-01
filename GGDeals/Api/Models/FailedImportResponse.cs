namespace GGDeals.Api.Models
{
    public class FailedImportResponse
    {
        public bool Success { get; set; }
        public FailedImportData Data { get; set; }
    }
}