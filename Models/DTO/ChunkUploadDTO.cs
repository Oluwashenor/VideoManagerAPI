namespace VideoManagerAPI.Models.DTO
{
    public class ChunkUploadDTO
    {
        public byte[]? Chunk { get; set; }
        public string? ChunkString { get; set; }
        public string? Id { get; set; }
        public DateTime Created { get; set; } = DateTime.Now;
    }
}
