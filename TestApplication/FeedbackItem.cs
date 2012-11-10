﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TestApplication
{
    public class FeedbackItem : TestApplication.IFeedbackItem
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
        public string Text { get; set; }
        public DateTime Created { get; set; }
        public bool Closed { get; set; }
        public int VoteCount { get; set; }
    }
}