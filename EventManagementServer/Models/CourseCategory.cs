using System.Text.Json.Serialization;

namespace EventManagementServer.Models
{
    public class CourseCategory
    {
        public int CourseId { get; set; }
        [JsonIgnore]
        public Course Course { get; set; }

        public int CategoryId { get; set; }
        [JsonIgnore]
        public Category Category { get; set; }
    }
}
