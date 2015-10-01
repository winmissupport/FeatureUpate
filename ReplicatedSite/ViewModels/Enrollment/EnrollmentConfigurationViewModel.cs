using Common;

namespace ReplicatedSite.ViewModels
{
    public class EnrollmentConfigurationViewModel
    {
        public int EnrollerID { get; set; }
        public MarketName MarketName { get; set; }
        public EnrollmentType SelectedEnrollmentType { get; set; }
    }
}