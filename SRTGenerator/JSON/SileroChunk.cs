using System.Text.Json.Serialization;

namespace SRTGenerator.JSON
{
    public class SileroChunk
    {
        [JsonPropertyName("start")]
        public double? Start { get; set; }
        [JsonPropertyName("end")]
        public double? End { get; set; }
        [JsonPropertyName("chunk_start")]
        public double? ChunStart { get; set; }
        [JsonPropertyName("chunk_end")]
        public double? ChunkEnd { get; set; }
        [JsonPropertyName("offset")]
        public double? Offset { get; set; }
    }
}
