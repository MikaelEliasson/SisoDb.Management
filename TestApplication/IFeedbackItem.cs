using System;
using System.Collections.Generic;
namespace TestApplication
{
    public interface IFeedbackItem
    {
        bool Closed { get; set; }
        DateTime Created { get; set; }
        Guid Id { get; set; }
        string Title { get; set; }
        DateTime? CloseDate { get; set; }
        IEnumerable<TakenAction> Actions { get; set; }
        TakenAction LatestAction { get; set; }
    }

    public class TakenAction
    {
        public string Name { get; set; }
        public DateTime Date { get; set; }
    }
}
