using InternshipPortalApi.Data;
using InternshipPortalApi.DTOs;
using InternshipPortalApi.Models;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using System.Security.Claims;

namespace InternshipPortalApi.Controllers
{
    [ApiController]

    [Route("api/[controller]")]

    public class InternshipController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public InternshipController(
            ApplicationDbContext context
        )
        {
            _context = context;
        }


        // CREATE INTERNSHIP

        [Authorize(Roles = "Admin")]

        [HttpPost("create")]

        public async Task<IActionResult>
        CreateInternship(
            CreateInternshipDto dto
        )
        {
            var internship =
            new Internship
            {
                Title = dto.Title,

                Description = dto.Description,

                MinimumCGPA = dto.MinimumCGPA,

                MaximumBacklogs = dto.MaximumBacklogs,

                Department = dto.Department,

                AllowBacklogs = dto.MaximumBacklogs > 0,

                Deadline = dto.Deadline.Date.AddDays(1).AddTicks(-1),

                CompanyName = dto.CompanyName,

                NumberOfSeats = dto.NumberOfSeats,

                CreatedAt = DateTime.Now
            };

            _context.Internships.Add(
                internship
            );

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message =
                "Internship created successfully"
            });
        }


        // ONLY ELIGIBLE INTERNSHIPS

        [AllowAnonymous]

        [HttpGet]

        public async Task<IActionResult>
        GetInternships()
        {
            var internships =
            await _context.Internships

            .Where(x => x.Deadline >= DateTime.Now.Date)

            .OrderByDescending(
                x => x.CreatedAt
            )

            .Select(x => new
            {
                x.Id,

                x.Title,

                x.Description,

                x.MinimumCGPA,

                x.MaximumBacklogs,

                x.Department,

                x.AllowBacklogs,

                x.Deadline,

                x.CompanyName,

                x.NumberOfSeats,

                x.CreatedAt
            })

            .ToListAsync();

            return Ok(internships);
        }


        // ADMIN INTERNSHIPS

        [Authorize(Roles = "Admin")]

        [HttpGet("admin")]

        public async Task<IActionResult>
        GetAdminInternships()
        {
            var internships =
            await _context.Internships

            .Include(x => x.Applications)

            .Select(x => new
            {
                x.Id,

                x.Title,

                x.CompanyName,

                x.MinimumCGPA,

                x.MaximumBacklogs,

                x.Department,

                x.NumberOfSeats,

                FilledSeats =
                x.Applications.Count,

                RemainingSeats =
                x.NumberOfSeats -
                x.Applications.Count,

                x.Deadline
            })

            .OrderByDescending(
                x => x.Id
            )

            .ToListAsync();

            return Ok(internships);
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("stats")]
        public async Task<IActionResult> GetAdminStats()
        {
            var totalStudents =
            await _context.Users.CountAsync();

            var totalInternships =
            await _context.Internships.CountAsync();

            var totalApplications =
            await _context.Applications.CountAsync();

            var acceptedApplications =
            await _context.Applications
            .CountAsync(x => x.Status == "Accepted");

            return Ok(new
            {
                totalStudents,
                totalInternships,
                totalApplications,
                acceptedApplications
            });
        }

        // DELETE INTERNSHIP

        [Authorize(Roles = "Admin")]

        [HttpDelete("{id}")]

        public async Task<IActionResult>
        DeleteInternship(int id)
        {
            var internship =
            await _context.Internships
            .FirstOrDefaultAsync(
                x => x.Id == id
            );

            if(internship == null)
            {
                return NotFound(
                    "Internship not found"
                );
            }

            _context.Internships.Remove(
                internship
            );

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message =
                "Internship deleted successfully"
            });
        }
    }
}