using System;
namespace TestApplication
{
    public interface IComment
    {
        DateTime CommentDate { get; set; }
        bool Deleted { get; set; }
        Guid FeedbackItemId { get; set; }
        Guid Id { get; set; }
        string Author { get; set; }
    }
}
