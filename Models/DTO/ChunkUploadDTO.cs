namespace VideoManagerAPI.Models.DTO
{
    public class ChunkUploadStringDTO
    {
        public string? ChunkString { get; set; }
        public string? Id { get; set; }
        public DateTime Created { get; set; } = DateTime.Now;
    }

    public class ChunkUploadByteDTO
    {
        public byte[]? Chunk { get; set; }
        public string? Id { get; set; }
        public DateTime Created { get; set; } = DateTime.Now;
    }
}
