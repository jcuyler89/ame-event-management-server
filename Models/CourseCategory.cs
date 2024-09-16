using System.Text.Json.Serialization;

namespace EventManagementServer.Models
{
    public class CourseCategory
    {
        public Guid CourseId { get; set; }
        [JsonIgnore]
        public Course Course { get; set; }

        public Guid CategoryId { get; set; }
        [JsonIgnore]
        public Category Category { get; set; }
    }
}
