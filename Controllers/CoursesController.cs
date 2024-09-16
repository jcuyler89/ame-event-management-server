using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using EventManagementServer.Data;
using EventManagementServer.Models;
using Microsoft.EntityFrameworkCore;

namespace EventManagementServer.Controllers
{
    [Route("/[controller]")]
    [ApiController]
    [Authorize]
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
            var categoryConnections = new List<Category>();

            foreach (var name in courseDto.Categories)
            {
                var category = await _context.Categories.SingleOrDefaultAsync(c => c.Name == name.ToLower()); // Convert category name to lowercase if necessary
                if (category == null)
                {
                    category = new Category { Name = name.ToLower() }; // Store category name as lowercase if necessary
                    _context.Categories.Add(category);
                    await _context.SaveChangesAsync();
                }
                categoryConnections.Add(category);
            }

            var course = new Course
            {
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

            return CreatedAtAction(nameof(GetCourses), new { id = course.Id }, course);
        }

        [HttpPost("MyCourses")]
        public async Task<ActionResult<IEnumerable<CourseDto>>> GetCoursesByUserEmail([FromBody] UserEmailDto userEmailDto)
        {
            if (string.IsNullOrWhiteSpace(userEmailDto.Email))
            {
                return BadRequest(new { message = "Email is required" });
            }

            // Convert input email to lowercase
            var emailLower = userEmailDto.Email.ToLower();

            // Find the registrations by user email (case-insensitive)
            var registrations = await _context.Registrations
                .Include(r => r.Course)
                    .ThenInclude(c => c.CourseCategories)
                        .ThenInclude(cc => cc.Category)
                .Where(r => r.Email.ToLower() == emailLower)  // Convert database email to lowercase
                .ToListAsync();

            if (registrations == null || registrations.Count == 0)
            {
                return NotFound(new { message = "No registrations found for the given email" });
            }

            // Get the list of courses the user is registered for
            var courses = registrations.Select(r => r.Course).Distinct().ToList();

            // Convert to CourseDto
            var result = courses.Select(course => new CourseDto
            {
                Id = course.Id,
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

            var categoryConnections = new List<Category>();

            foreach (var name in courseDto.Categories)
            {
                var category = await _context.Categories.SingleOrDefaultAsync(c => c.Name == name.ToLower()); // Convert category name to lowercase if necessary
                if (category == null)
                {
                    category = new Category { Name = name.ToLower() }; // Store category name as lowercase if necessary
                    _context.Categories.Add(category);
                    await _context.SaveChangesAsync();
                }
                categoryConnections.Add(category);
            }

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
