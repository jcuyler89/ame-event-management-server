using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace EventManagementServer.Models
{
    public class Category
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Name { get; set; }

        [JsonIgnore]
        public List<CourseCategory> CourseCategories { get; set; } = new List<CourseCategory>();
    }
}
