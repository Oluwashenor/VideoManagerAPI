using System.ComponentModel.DataAnnotations;

namespace VideoManagerAPI.Models
{
    public class Video
    {
        public string Id { get; set; }
        public string? Url { get; set; }
        public DateTime Created { get; set; }
        public List<Transcript>? Transcripts { get; set; }
    }
}
