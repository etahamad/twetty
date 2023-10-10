namespace twetty.DTOs
{
    public class ReplyDto
    {
        public int UserId { get; set; }
        public int TweetId { get; set; }
        public string Content { get; set; }
        public DateTime CreatedAt { get; set; }
    }

}
