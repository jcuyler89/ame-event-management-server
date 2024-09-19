using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using EventManagementServer.Data;
using EventManagementServer.Models;
using Microsoft.EntityFrameworkCore;

namespace EventManagementServer.Controllers
{
    [Route("[controller]")] // Removed the leading slash
    [ApiController]
    // [Authorize]
    public class CoursesController : ControllerBase
    {
        private readonly AppDbContext _context;

        public CoursesController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<CourseDto>>> GetCourses()
        {
            var courses = await _context.Courses
                .Include(c => c.CourseCategories)
                    .ThenInclude(cc => cc.Category)
                .ToListAsync();

            var result = courses.Select(course => new CourseDto
            {
                Id = course.Id,
                CourseId = course.CourseId, // Assuming this is intentional
                SessionId = course.SessionId,
                GroupId = course.GroupId,
                Title = course.Title,
                Description = course.Description,
                Instructor = course.Instructor,
                StartDate = course.StartDate,
                EndDate = course.EndDate,
                Duration = course.Duration,
                Categories = course.CourseCategories.Select(cc => cc.Category.Name).ToList()
            });

            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<CourseDto>> GetCourseById(Guid id)
        {
            var course = await _context.Courses
                .Include(c => c.CourseCategories)
                    .ThenInclude(cc => cc.Category)
                .SingleOrDefaultAsync(c => c.Id == id);

            if (course == null)
            {
                return NotFound();
            }

            var result = new CourseDto
            {
                Id = course.Id,
                CourseId = course.CourseId,
                SessionId = course.SessionId,
                GroupId = course.GroupId,
                Title = course.Title,
                Description = course.Description,
                Instructor = course.Instructor,
                StartDate = course.StartDate,
                EndDate = course.EndDate,
                Duration = course.Duration,
                Categories = course.CourseCategories.Select(cc => cc.Category.Name).ToList()
            };

            return Ok(result);
        }

        [HttpPost]
        public async Task<ActionResult<Course>> CreateCourse(CourseDto courseDto)
        {
            if (courseDto.Categories == null || !courseDto.Categories.Any())
            {
                return BadRequest(new { message = "Categories cannot be empty." });
            }

            var existingCategories = await _context.Categories
                .Where(c => courseDto.Categories.Contains(c.Name.ToLower()))
                .ToListAsync();

            var newCategories = courseDto.Categories
                .Where(name => !existingCategories.Any(c => c.Name == name.ToLower()))
                .Select(name => new Category { Name = name.ToLower() })
                .ToList();

            _context.Categories.AddRange(newCategories);
            await _context.SaveChangesAsync();

            var categoryConnections = existingCategories.Concat(newCategories).ToList();

            var course = new Course
            {
                CourseId = courseDto.CourseId,
                SessionId = courseDto.SessionId,
                GroupId = courseDto.GroupId,
                Title = courseDto.Title,
                Description = courseDto.Description,
                Instructor = courseDto.Instructor,
                StartDate = DateTime.SpecifyKind(courseDto.StartDate, DateTimeKind.Utc),
                EndDate = DateTime.SpecifyKind(courseDto.EndDate, DateTimeKind.Utc),
                Duration = courseDto.Duration,
                CourseCategories = categoryConnections.Select(category => new CourseCategory
                {
                    Category = category
                }).ToList()
            };

            _context.Courses.Add(course);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetCourseById), new { id = course.Id }, course); // Changed to GetCourseById
        }

        [HttpPost("MyCourses")]
        public async Task<ActionResult<IEnumerable<CourseDto>>> GetCoursesByUserEmail([FromBody] UserEmailDto userEmailDto)
        {
            if (string.IsNullOrWhiteSpace(userEmailDto.Email))
            {
                return BadRequest(new { message = "Email is required" });
            }

            var emailLower = userEmailDto.Email.ToLower();

            var registrations = await _context.Registrations
                .Include(r => r.Course)
                    .ThenInclude(c => c.CourseCategories)
                        .ThenInclude(cc => cc.Category)
                .Where(r => r.Email.ToLower() == emailLower)
                .ToListAsync();

            if (registrations == null || registrations.Count == 0)
            {
                return NotFound(new { message = "No registrations found for the given email" });
            }

            var courses = registrations.Select(r => r.Course).Distinct().ToList();

            var result = courses.Select(course => new CourseDto
            {
                Id = course.Id,
                CourseId = course.CourseId,
                SessionId = course.SessionId,
                GroupId = course.GroupId,
                Title = course.Title,
                Description = course.Description,
                Instructor = course.Instructor,
                StartDate = course.StartDate,
                EndDate = course.EndDate,
                Duration = course.Duration,
                Categories = course.CourseCategories.Select(cc => cc.Category.Name).ToList()
            });

            return Ok(result);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<Course>> UpdateCourse(Guid id, CourseDto courseDto)
        {
            var course = await _context.Courses
                .Include(c => c.CourseCategories)
                .SingleOrDefaultAsync(c => c.Id == id);

            if (course == null)
            {
                return NotFound();
            }

            if (courseDto.Categories == null || !courseDto.Categories.Any())
            {
                return BadRequest(new { message = "Categories cannot be empty." });
            }

            var existingCategories = await _context.Categories
                .Where(c => courseDto.Categories.Contains(c.Name.ToLower()))
                .ToListAsync();

            var newCategories = courseDto.Categories
                .Where(name => !existingCategories.Any(c => c.Name == name.ToLower()))
                .Select(name => new Category { Name = name.ToLower() })
                .ToList();

            _context.Categories.AddRange(newCategories);
            await _context.SaveChangesAsync();

            var categoryConnections = existingCategories.Concat(newCategories).ToList();

            course.CourseId = courseDto.CourseId;
            course.SessionId = courseDto.SessionId;
            course.GroupId = courseDto.GroupId;
            course.Title = courseDto.Title;
            course.Description = courseDto.Description;
            course.Instructor = courseDto.Instructor;
            course.StartDate = DateTime.SpecifyKind(courseDto.StartDate, DateTimeKind.Utc);
            course.EndDate = DateTime.SpecifyKind(courseDto.EndDate, DateTimeKind.Utc);
            course.Duration = courseDto.Duration;

            course.CourseCategories.Clear();
            foreach (var category in categoryConnections)
            {
                course.CourseCategories.Add(new CourseCategory
                {
                    CourseId = course.Id,
                    CategoryId = category.Id
                });
            }

            await _context.SaveChangesAsync();

            return Ok(course);
        }
    }
}
