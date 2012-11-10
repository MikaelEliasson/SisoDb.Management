using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TestApplication
{
    public class Comment : TestApplication.IComment
    {
        public Guid Id { get; set; }
        public Guid FeedbackItemId { get; set; }
        public string FeedbackItemTitle { get; set; }
        public string Text { get; set; }
        public DateTime CommentDate { get; set; }
        public bool Deleted { get; set; }
        public string Author { get; set; }
    }
}
