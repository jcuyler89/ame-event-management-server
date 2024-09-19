namespace EventManagementServer.Models
{
    public class CourseDto
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string CourseId { get; set; }
        public string SessionId { get; set; }
        public string GroupId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string Instructor { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Duration { get; set; }
        public List<string> Categories { get; set; }
    }
}
