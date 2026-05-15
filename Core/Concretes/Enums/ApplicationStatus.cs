namespace Core.Concretes.Enums
{
    /// <summary>
    /// Represents the current state of a worker's application to a customer's job posting.
    /// </summary>
    public enum ApplicationStatus
    {
        Pending = 1,    // The worker has applied, but the customer has not taken action.
        Reviewing = 2,  // The customer is currently looking at the application/comparing.
        Accepted = 3,   // The customer has hired the worker for this job.
        Rejected = 4    // The customer has declined the application.
    }
}