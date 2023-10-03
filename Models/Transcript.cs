using System.ComponentModel.DataAnnotations;

namespace VideoManagerAPI.Models
{
    public class Transcript
    {
        public int Id { get; set; }
        public string Text { get; set; }
        public TimeSpan Start { get; set; }
        public TimeSpan End { get; set; }
        public string VideoId { get; set; }
    }
}
