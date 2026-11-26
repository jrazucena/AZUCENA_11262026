using System.Text.Json.Serialization;

namespace AZUCENA_11262026.Models
{
    [JsonConverter(typeof(JsonStringEnumConverter<ProcessingStatus>))]
    public enum ProcessingStatus
    {
        Success,
        Failed
    }
}
