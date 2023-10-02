namespace VideoManagerAPI.Models.DTO
{
    public class VideoResponse
    {
        public string Id { get; set; }
        public string Url { get; set; }
        public List<Transcript>? Transcripts { get; set; }
    }
}
