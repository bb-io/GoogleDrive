using Blackbird.Applications.Sdk.Common;

namespace Apps.GoogleDrive.Models.Storage.Responses
{
    public class GetFileCommentsResponse
    {
        public List<FileComment> Comments { get; set; } = new();
    }

    public class FileComment
    {
        [Display("Comment ID")]
        public string ID { get; set; }
        public string Content { get; set; }
        public string Author { get; set; }

        public string? Anchor { get; set; }

        [Display("Quoted content")]
        public string? QuotedContent { get; set; }

        [Display("Quoted content MIME type")]
        public string? QuotedContentMimeType { get; set; }

        [Display("Created time")]
        public DateTime? CreatedTime { get; set; }

        public List<CommentReply> Replies { get; set; } = new();
    }

    public class CommentReply
    {
        [Display("Reply ID")]
        public string ID { get; set; }
        public string Content { get; set; }
        public string Author { get; set; }

        [Display("Created time")]
        public DateTime? CreatedTime { get; set; }
    }
}
