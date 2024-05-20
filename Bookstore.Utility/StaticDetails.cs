namespace Bookstore.Utility
{
	public static class StaticDetails
	{
		public const int RelatedBooksCount = 10;

		public const string Role_Customer = "Customer";
		public const string Role_Admin = "Admin";
		public const string Role_Employee = "Employee";
		public const string Role_Company = "Company";

		public const string SessionCart = "SessionShoppingCart";

		public const string LandingPage_Newest = "New Books";
		public const string LandingPage_Manga = "Unleash Imagination with Top Manga Picks";
		public const string LandingPage_Fantasy = "Journey Through Enchanting Fantasy Worlds";
		public const string LandingPage_ScienceFiction = "Venture into the Future with Sci-Fi Reads";
		public const string LandingPage_Romance = "Experience Heartfelt Moments in Romance Novels";

        public const string Status_Pending = "Pending";
        public const string Status_Approved = "Approved";
        public const string Status_InProcess = "Processing";
        public const string Status_Shipped = "Shipped";
        public const string Status_Cancelled = "Cancelled";
        public const string Status_Refunded = "Refunded";

        public const string PaymentStatus_Pending = "Pending";
        public const string PaymentStatus_Approved = "Approved";
        public const string PaymentStatus_DelayedPayment = "ApprovedForDelayedPayment";
        public const string PaymentStatus_Rejected = "Rejected";
    }
}