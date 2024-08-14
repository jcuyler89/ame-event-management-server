using System.Text.Json.Serialization;

namespace EventManagementServer.Models
{
    public class Registration
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public int CostCenter { get; set; }
        public int SourceId { get; set; }
        public string Department { get; set; }
        public string Position { get; set; }
        public string TeamMemberManager { get; set; }
        public string ManagerEmail { get; set; }
        public int CourseId { get; set; }

        [JsonIgnore]
        public Course Course { get; set; }

        public string SubmittedBy { get; set; }
        public string RegistrationStatus { get; set; }
    }
}
