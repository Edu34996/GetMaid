namespace Core.Concretes.Enums
{
    public enum VerificationStatus
    {
        Unverified = 1,     // Has not uploaded anything
        PendingReview = 2,  // Uploaded ID, waiting for Admin approval
        Verified = 3,       // Admin approved
        Rejected = 4        // Admin rejected (e.g., blurry image, expired ID)
    }
}