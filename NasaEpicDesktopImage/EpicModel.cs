using System.Text.Json.Serialization;

namespace NasaEpicDesktopImage
{
    public class EpicModel
    {
        [JsonPropertyName("identifier")]
        public string Identifier { get; set; }

        [JsonPropertyName("caption")]
        public string Caption { get; set; }

        [JsonPropertyName("image")]
        public string Image { get; set; }

        [JsonPropertyName("version")]
        public string Version { get; set; }

        [JsonPropertyName("date")]
        public string Date { get; set; }
    }
}
