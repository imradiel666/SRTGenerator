using System.Text.Json.Serialization;

namespace SRTGenerator.JSON
{
    public class VoskPartialResult
    {
        [JsonPropertyName("partial")]
        public string Partial { get; set; }
    }

    public class VoskResult
    {
        [JsonPropertyName("result")]
        public VoskResultWord[] Words { get; set; }
        [JsonPropertyName("text")]
        public string Text { get; set; }
    }

    public class VoskResultWord
    {
        [JsonPropertyName("conf")]
        public double? Conf { get; set; }
        [JsonPropertyName("end")]
        public double? End { get; set; }
        [JsonPropertyName("start")]
        public double? Start { get; set; }
        [JsonPropertyName("word")]
        public string Word { get; set; }
    }
}
