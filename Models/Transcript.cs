namespace VideoManagerAPI.Models
{
    public class Transcript
    {
        public string Text { get; set; }
        public TimeSpan Start { get; set; }
        public TimeSpan End { get; set; }
    }
}
