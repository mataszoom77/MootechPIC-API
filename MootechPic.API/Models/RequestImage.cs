namespace MootechPic.API.Models
{
    public class RequestImage
    {
        public Guid Id { get; set; }
        public Guid RequestId { get; set; }
        public Request Request { get; set; } = null!;
        public string Url { get; set; } = null!;
    }
}
