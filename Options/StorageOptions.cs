namespace LogiTrack.WebApi.Options
{
    public class StorageOptions
    {
        public bool UseFileStorage { get; set; } = true;
        public string ShipmentsFilePath { get; set; } = "App_Data/shipments.json";
    }
}
