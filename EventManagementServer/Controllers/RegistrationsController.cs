using Microsoft.AspNetCore.Mvc;
using EventManagementServer.Data;
using EventManagementServer.Models;
using Microsoft.EntityFrameworkCore;

namespace EventManagementServer.Controllers
{
    [Route("/[controller]")]
    [ApiController]
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
            if (registration.CourseId <= 0)
            {
                return BadRequest(new { message = "Invalid CourseId" });
            }

            // Additional custom validation can be added here

            try
            {
                _context.Registrations.Add(registration);
                await _context.SaveChangesAsync();
                return CreatedAtAction(nameof(GetRegistrationById), new { id = registration.Id }, registration);
            }
            catch (Exception ex)
            {
                // Log the exception (not shown here for brevity)
                return StatusCode(500, new { message = "An error occurred while creating the registration." });
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Registration>> GetRegistrationById(int id)
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
        public async Task<IActionResult> UpdateRegistration(int id, Registration registration)
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

        private bool RegistrationExists(int id)
        {
            return _context.Registrations.Any(e => e.Id == id);
        }
    }
}
