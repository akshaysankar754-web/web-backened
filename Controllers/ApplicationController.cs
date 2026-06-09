using InternshipPortalApi.Data;
using InternshipPortalApi.DTOs;
using InternshipPortalApi.Models;
using InternshipPortalApi.Services;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using System.Security.Claims;

namespace InternshipPortalApi.Controllers
{
[ApiController]
[Route("api/[controller]")]
public class ApplicationController : ControllerBase
{
private readonly ApplicationDbContext _context;
private readonly IEmailService _emailService;

    public ApplicationController(
        ApplicationDbContext context,
        IEmailService emailService
    )
    {
        _context = context;
        _emailService = emailService;
    }

    // APPLY INTERNSHIP

    [Authorize]
    [HttpPost("apply/{internshipId}")]
    public async Task<IActionResult> ApplyInternship(int internshipId)
    {
        var userId =
        int.Parse(
            User.FindFirstValue(
                ClaimTypes.NameIdentifier
            )
        );

        var user =
        await _context.Users
        .FirstOrDefaultAsync(
            x => x.Id == userId
        );

        var internship =
        await _context.Internships
        .Include(x => x.Applications)
        .FirstOrDefaultAsync(
            x => x.Id == internshipId
        );

        if (internship == null)
        {
            return NotFound(
                "Internship not found"
            );
        }

        if (user.CGPA < internship.MinimumCGPA)
        {
            return BadRequest(
                "Not eligible based on CGPA"
            );
        }

        var alreadyApplied =
        await _context.Applications
        .AnyAsync(x =>
            x.UserId == userId &&
            x.InternshipId == internshipId
        );

        if (alreadyApplied)
        {
            return BadRequest(
                "Already applied"
            );
        }

        var application =
        new Application
        {
            UserId = userId,
            InternshipId = internshipId,
            AppliedAt = DateTime.Now,
            Status = "Pending"
        };

        _context.Applications.Add(
            application
        );

        await _context.SaveChangesAsync();

        return Ok(new
        {
            message = "Applied successfully"
        });
    }

    // MY APPLICATIONS

    [Authorize]
    [HttpGet("myapplications")]
    public async Task<IActionResult> GetMyApplications()
    {
        var userId =
        int.Parse(
            User.FindFirstValue(
                ClaimTypes.NameIdentifier
            )
        );

        Console.WriteLine($"Logged UserId = {userId}");

        var applications =
        await _context.Applications

        .Include(x => x.Internship)

        .Where(x => x.UserId == userId)

        .Select(x => new
        {
            x.Id,
            x.InternshipId,
            InternshipTitle = x.Internship.Title,
            CompanyName = x.Internship.CompanyName,
            Deadline = x.Internship.Deadline,
            x.Status,
            x.AppliedAt
        })

        .ToListAsync();

        Console.WriteLine($"Applications Count = {applications.Count}");

        return Ok(applications);
    }

    // MY COURSE

    [Authorize]
    [HttpGet("mycourse")]
    public async Task<IActionResult> GetMyCourse()
    {
        var userId =
        int.Parse(
            User.FindFirstValue(
                ClaimTypes.NameIdentifier
            )
        );

        var courses =
        await _context.Applications

        .Include(x => x.Internship)

        .Where(x =>
            x.UserId == userId &&
            x.Status == "Accepted"
        )

        .Select(x => new
        {
            x.Id,
            InternshipTitle = x.Internship.Title,
            CompanyName = x.Internship.CompanyName,
            Description = x.Internship.Description,
            Deadline = x.Internship.Deadline
        })

        .ToListAsync();

        return Ok(courses);
    }

    // UPDATE STATUS

    [Authorize(Roles = "Admin")]
    [HttpPut("{applicationId}/status")]
    public async Task<IActionResult> UpdateApplicationStatus(
        int applicationId,
        [FromBody] UpdateApplicationStatusDto dto
    )
    {
        var application =
        await _context.Applications
            .Include(x => x.User)
            .Include(x => x.Internship)
            .FirstOrDefaultAsync(x => x.Id == applicationId);

            if (application == null)
            {
                return NotFound(
                    "Application not found"
                );
            }

            application.Status = dto.Status;

            await _context.SaveChangesAsync();

            if (application.Status == "Accepted")
            {
                try
                {
                    var subject = $"Your application for {application.Internship.Title} has been accepted";
                    var body = $"Hello {application.User.Name},\n\n" +
                        $"Congratulations! Your application for the internship '{application.Internship.Title}' at {application.Internship.CompanyName} has been accepted.\n\n" +
                        "Please follow up with the company if you need any further details.\n\n" +
                        "Best regards,\nInternship Portal";

                    await _emailService.SendEmailAsync(
                        application.User.Email,
                        subject,
                        body
                    );
                }
                catch
                {
                    // Log only; do not block status update.
                }
            }

            return Ok(application);
        }

    // VIEW APPLICANTS

    [Authorize(Roles = "Admin")]
    [HttpGet("internship/{internshipId}")]
    public async Task<IActionResult> GetApplicants(
        int internshipId
    )
    {
        var applicants =
        await _context.Applications

        .Include(x => x.User)

        .Where(x =>
            x.InternshipId == internshipId
        )

        .Select(x => new
        {
            x.Id,
            x.User.Name,
            x.User.Email,
            x.User.CGPA,
            x.User.CollegeName,
            x.Status,
            x.AppliedAt
        })

        .ToListAsync();

        return Ok(applicants);
    }

    [Authorize(Roles = "Admin")]
    [HttpGet("all")]
    public async Task<IActionResult> GetAllApplications()
    {
        var applications =
        await _context.Applications

        .Include(x => x.User)
        .Include(x => x.Internship)

        .Select(x => new
        {
            x.Id,
            ApplicantName = x.User.Name,
            ApplicantEmail = x.User.Email,
            x.User.CGPA,
            x.User.CollegeName,
            InternshipTitle = x.Internship.Title,
            InternshipId = x.InternshipId,
            x.Status,
            x.AppliedAt
        })
        .OrderByDescending(x => x.AppliedAt)
        .ToListAsync();

        return Ok(applications);
    }
}


}