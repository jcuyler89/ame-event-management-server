namespace EventManagementServer.DTOs
{
    public class AppliedUsersRequestDto
    {
        public string Keyword { get; set; }
    }

    public class AppliedUserDto
    {
        public int CostCenter { get; set; }
        public string Department { get; set; }
        public string Position { get; set; }
        public string Source { get; set; }
        public int SourceId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public string Country { get; set; }
        public bool IsActive { get; set; }
        public int ManagerId { get; set; }
        public DateTime? EffectiveTo { get; set; }
        public DateTime EffectiveFrom { get; set; }
    }

    public class AppliedUsersResponseDto
    {
        public int CurrentPage { get; set; }
        public int FilteredCount { get; set; }
        public int PageSize { get; set; }
        public List<AppliedUserDto> Results { get; set; }
        public int TotalCount { get; set; }
        public int TotalPages { get; set; }
    }
}
