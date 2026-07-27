namespace PoliticalLeaderPortal.ViewModels
{
    public class HomeCitizenConnectVM
    {
        public HomeCitizenConnectVM()
        {
            Contact = CreateRequest("Contact", "Contact Office");
            Suggestion = CreateRequest("Suggestion", "Development Suggestion");
            Volunteer = CreateRequest("Volunteer", "Volunteer Application");
        }

        public CitizenConnectVM Contact { get; set; }
        public CitizenConnectVM Suggestion { get; set; }
        public CitizenConnectVM Volunteer { get; set; }

        private static CitizenConnectVM CreateRequest(string requestType, string subject)
        {
            return new CitizenConnectVM
            {
                RequestType = requestType,
                Subject = subject,
                Status = "New"
            };
        }
    }
}
