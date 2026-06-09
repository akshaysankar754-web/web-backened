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

    public class FeedbackController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public FeedbackController(
            ApplicationDbContext context
        )
        {
            _context = context;
        }


        // ================= SUBMIT FEEDBACK =================

        [Authorize]

        [HttpPost]

        public async Task<IActionResult>
        SubmitFeedback(CreateFeedbackDto dto)
        {
            var setting = await _context.FeedbackSettings.FirstOrDefaultAsync();

            if (setting == null)
            {
                setting = new FeedbackSetting { IsFeedbackOpen = true };
                _context.FeedbackSettings.Add(setting);
                await _context.SaveChangesAsync();
            }

            if (!setting.IsFeedbackOpen)
            {
                return BadRequest("Feedback is currently closed by the admin.");
            }

            var userId =
            int.Parse(
                User.FindFirstValue(
                    ClaimTypes.NameIdentifier
                )
            );

            var feedbackText = string.Join("\n", dto.Answers.Select((answer, index) => $"Q{index + 1}: {answer.Trim()}"));

            var feedback = new Feedback
            {
                Message = feedbackText,

                UserId = userId,

                CreatedAt = DateTime.Now
            };

            _context.Feedbacks.Add(
                feedback
            );

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message =
                "Feedback submitted successfully"
            });

        }


        // ================= GET FEEDBACKS =================

        [Authorize(Roles = "Admin")]

        [HttpGet]

        public async Task<IActionResult>
        GetFeedbacks()
        {

            var feedbacks =
            await _context.Feedbacks
            .Include(x => x.User)
            .OrderByDescending(
                x => x.CreatedAt
            )
            .Select(x => new
            {
                x.Id,
                x.Message,
                x.CreatedAt,
                x.UserId,
                UserName = x.User != null ? x.User.Name : "Unknown Student",
                UserEmail = x.User != null ? x.User.Email : "Unknown Email"
            })
            .ToListAsync();

            return Ok(feedbacks);

        }

        [HttpGet("status")]
        public async Task<IActionResult> GetFeedbackStatus()
        {
            var setting = await _context.FeedbackSettings.FirstOrDefaultAsync();

            if (setting == null)
            {
                setting = new FeedbackSetting { IsFeedbackOpen = true };
                _context.FeedbackSettings.Add(setting);
                await _context.SaveChangesAsync();
            }

            return Ok(new { isFeedbackOpen = setting.IsFeedbackOpen });
        }

        [Authorize(Roles = "Admin")]
        [HttpPut("status")]
        public async Task<IActionResult> SetFeedbackStatus([FromBody] FeedbackSetting setting)
        {
            var current = await _context.FeedbackSettings.FirstOrDefaultAsync();

            if (current == null)
            {
                current = new FeedbackSetting();
                _context.FeedbackSettings.Add(current);
            }

            current.IsFeedbackOpen = setting.IsFeedbackOpen;
            await _context.SaveChangesAsync();

            return Ok(new { isFeedbackOpen = current.IsFeedbackOpen });
        }

    }
}