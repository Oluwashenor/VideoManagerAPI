using System.ComponentModel.DataAnnotations;

namespace VideoManagerAPI.Models
{
    public class Video
    {
        public Video()
        {
            Id = Guid.NewGuid().ToString();
            Created = DateTime.Now;
        }
        public string Id { get; set; }
        public string? Url { get; set; }
        public DateTime Created { get; set; }
        public List<Transcript>? Transcripts { get; set; }
    }
}
