using System.Text.Json.Serialization;

namespace EventManagementServer.Models
{
    public class Course
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Title { get; set; }
        public string Description { get; set; }
        public string Instructor { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Duration { get; set; }

        [JsonIgnore]
        public List<CourseCategory> CourseCategories { get; set; } = new List<CourseCategory>();

        [JsonIgnore]
        public List<Registration> Registrations { get; set; } = new List<Registration>();
    }
}
