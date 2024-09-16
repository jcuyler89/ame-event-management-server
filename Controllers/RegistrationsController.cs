using Microsoft.AspNetCore.Mvc;
using EventManagementServer.Data;
using EventManagementServer.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;

namespace EventManagementServer.Controllers
{
    [Route("/[controller]")]
    [ApiController]
    [Authorize]
    public class RegistrationsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public RegistrationsController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Registration>>> GetAllRegistrations()
        {
            var registrations = await _context.Registrations
                .Include(r => r.Course)
                .ToListAsync();
            return Ok(registrations);
        }

        [HttpPost]
        public async Task<ActionResult<Registration>> CreateRegistration(Registration registration)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Custom validation for courseId
            if (registration.CourseId <= Guid.Empty)
            {
                return BadRequest(new { message = "Invalid CourseId" });
            }

            // Set registrationStatus to PENDING
            registration.RegistrationStatus = "PENDING";

            // Additional custom validation can be added here

            try
            {
                _context.Registrations.Add(registration);
                await _context.SaveChangesAsync();
                return CreatedAtAction(nameof(GetRegistrationById), new { id = registration.Id }, registration);
            }
            catch (Exception)
            {
                // Log the exception (not shown here for brevity)
                return StatusCode(500, new { message = "An error occurred while creating the registration." });
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Registration>> GetRegistrationById(Guid id)
        {
            var registration = await _context.Registrations
                .Include(r => r.Course)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (registration == null)
            {
                return NotFound();
            }

            return Ok(registration);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateRegistration(Guid id, Registration registration)
        {
            if (id != registration.Id)
            {
                return BadRequest();
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            _context.Entry(registration).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!RegistrationExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // Get Team Mmebers who have registered for a course
        [HttpGet("course/{courseId}")]
        public async Task<ActionResult<IEnumerable<Registration>>> GetRegistrationsByCourse(Guid courseId)
        {
            if (courseId <= Guid.Empty)
            {
                return BadRequest(new { message = "Invalid CourseId" });
            }

            var registrations = await _context.Registrations
                .Where(r => r.CourseId == courseId)
                .ToListAsync();

            if (registrations == null || registrations.Count == 0)
            {
                return NotFound(new { message = "No registrations found for the given CourseId" });
            }

            return Ok(registrations);
        }


        private bool RegistrationExists(Guid id)
        {
            return _context.Registrations.Any(e => e.Id == id);
        }
    }
}
