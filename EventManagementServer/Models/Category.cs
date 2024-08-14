using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace EventManagementServer.Models
{
    public class Category
    {
        public int Id { get; set; }
        public string Name { get; set; }

        [JsonIgnore]
        public List<CourseCategory> CourseCategories { get; set; } = new List<CourseCategory>();
    }
}
