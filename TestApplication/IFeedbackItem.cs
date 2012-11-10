using System;
namespace TestApplication
{
    public interface IFeedbackItem
    {
        bool Closed { get; set; }
        DateTime Created { get; set; }
        Guid Id { get; set; }
        string Title { get; set; }
    }
}
