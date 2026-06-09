using BCrypt.Net;

using InternshipPortalApi.Data;
using InternshipPortalApi.DTOs;
using InternshipPortalApi.Models;
using InternshipPortalApi.Services;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Security.Cryptography;

namespace InternshipPortalApi.Controllers
{
    [ApiController]

    [Route("api/[controller]")]

    public class AuthController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        private readonly JwtService _jwtService;

        private readonly IEmailService _emailService;

        private readonly ILogger<AuthController> _logger;

        public AuthController(
            ApplicationDbContext context,
            JwtService jwtService,
            IEmailService emailService,
            ILogger<AuthController> logger
        )
        {
            _context = context;

            _jwtService = jwtService;

            _emailService = emailService;

            _logger = logger;
        }

        // ================= REGISTER =================

        [HttpPost("register")]

        public async Task<IActionResult>
        Register(RegisterDto dto)
        {
            var existingUser =
            await _context.Users
            .FirstOrDefaultAsync(
                x => x.Email == dto.Email
            );

            User user;

            if(existingUser != null)
            {
                if(existingUser.IsVerified)
                {
                    return BadRequest(
                        new { message = "Email already exists" }
                    );
                }

                user = existingUser;
                user.Name = dto.Name;
                user.Password = BCrypt.Net.BCrypt.HashPassword(dto.Password);
                user.Role = "Student";
                user.CGPA = dto.CGPA;
                user.Backlogs = dto.Backlogs;
                user.Department = dto.Department;
                user.PassingYear = dto.PassingYear;
                user.HasBacklogs = dto.Backlogs > 0;
                user.CollegeName = dto.CollegeName;
                user.IsVerified = false;
                user.OtpCode = GenerateOtp();
                user.CreatedAt = DateTime.Now;
            }
            else
            {
                user = new User
                {
                    Name = dto.Name,
                    Email = dto.Email,
                    Password = BCrypt.Net.BCrypt.HashPassword(dto.Password),
                    Role = "Student",
                    CGPA = dto.CGPA,
                    Backlogs = dto.Backlogs,
                    Department = dto.Department,
                    PassingYear = dto.PassingYear,
                    HasBacklogs = dto.Backlogs > 0,
                    CollegeName = dto.CollegeName,
                    IsVerified = false,
                    OtpCode = GenerateOtp(),
                    CreatedAt = DateTime.Now
                };

                _context.Users.Add(user);
            }

            await _context.SaveChangesAsync();

            var emailSent = await _emailService.SendEmailAsync(
                user.Email,
                "Verify your email",
                $"Your verification code is: {user.OtpCode}"
            );

            if (!emailSent)
            {
                return Ok(new
                {
                    message = "Account created, but verification email could not be sent. Check server logs or configure SMTP.",
                    requiresVerification = true
                });
            }

            return Ok(new
            {
                message =
                "Verification email sent. Please check your email to verify your account.",
                requiresVerification = true
            });
        }


        // ================= LOGIN =================

        [HttpPost("login")]

        public async Task<IActionResult>
        Login(LoginDto dto)
        {
            var user =
            await _context.Users
            .FirstOrDefaultAsync(
                x => x.Email == dto.Email
            );

            if(user == null)
            {
                // Fallback admin shortcut for the default admin account
                if(
                    dto.Email == "akshay@gmail.com"
                    &&
                    dto.Password == "123456"
                )
                {
                    var adminUser = new User
                    {
                        Id = 999,
                        Name = "Admin",
                        Email = "akshay@gmail.com",
                        Role = "Admin"
                    };

                    var adminToken =
                    _jwtService.GenerateToken(
                        adminUser
                    );

                    return Ok(new
                    {
                        token = adminToken,
                        role = "Admin",
                        cgpa = 0,
                        backlogs = 0,
                        department = string.Empty,
                        passingYear = 0,
                        hasBacklogs = false,
                        collegeName = string.Empty,
                        name = "Admin"
                    });
                }

                return Unauthorized(
                    "Invalid credentials"
                );
            }

            if(user == null)
            {
                return Unauthorized(
                    "Invalid credentials"
                );
            }

            bool isPasswordCorrect = false;

            if(
                !string.IsNullOrWhiteSpace(user.Password)
                &&
                (
                    user.Password.StartsWith("$2a$")
                    ||
                    user.Password.StartsWith("$2b$")
                    ||
                    user.Password.StartsWith("$2y$")
                )
            )
            {
                isPasswordCorrect =
                BCrypt.Net.BCrypt.Verify(
                    dto.Password,
                    user.Password
                );
            }
            else
            {
                isPasswordCorrect =
                string.Equals(
                    dto.Password,
                    user.Password,
                    StringComparison.Ordinal
                );

                if(isPasswordCorrect)
                {
                    user.Password =
                    BCrypt.Net.BCrypt.HashPassword(
                        dto.Password
                    );

                    await _context.SaveChangesAsync();
                }
            }

            if(!user.IsVerified)
            {
                return Unauthorized(
                    "Email not verified"
                );
            }

            if(!isPasswordCorrect)
            {
                return Unauthorized(
                    "Invalid credentials"
                );
            }

            var token = _jwtService.GenerateToken(user);

            return Ok(new
            {
                token = token,
                role = user.Role,
                cgpa = user.CGPA,
                backlogs = user.Backlogs,
                department = user.Department,
                passingYear = user.PassingYear,
                hasBacklogs = user.HasBacklogs,
                collegeName = user.CollegeName,
                name = user.Name
            });
        }

        [HttpPost("verify-login")]
        [HttpPost("verify-otp")]
        public async Task<IActionResult> VerifyLogin(VerifyOtpDto dto)
        {
            var otpRecord = await _context.EmailOtps
                .Where(x => x.UserId == dto.UserId && x.OtpCode == dto.Otp && !x.IsUsed)
                .OrderByDescending(x => x.Id)
                .FirstOrDefaultAsync();

            if(otpRecord == null)
            {
                return BadRequest(
                    "Invalid OTP"
                );
            }

            if(otpRecord.ExpiresAt < DateTime.UtcNow)
            {
                return BadRequest(
                    "OTP expired"
                );
            }

            otpRecord.IsUsed = true;
            await _context.SaveChangesAsync();

            var user = await _context.Users.FindAsync(dto.UserId);
            if(user == null)
            {
                return NotFound(
                    "User not found"
                );
            }

            var token = _jwtService.GenerateToken(user);

            return Ok(new
            {
                token = token,
                role = user.Role,
                cgpa = user.CGPA,
                backlogs = user.Backlogs,
                department = user.Department,
                passingYear = user.PassingYear,
                hasBacklogs = user.HasBacklogs,
                collegeName = user.CollegeName,
                name = user.Name
            });
        }

        [HttpPost("verify-email")]
        public async Task<IActionResult> VerifyEmail(VerifyEmailDto dto)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(x => x.Email == dto.Email);

            if(user == null)
            {
                return NotFound(
                    "User not found"
                );
            }

            if(user.IsVerified)
            {
                return BadRequest(
                    "Email already verified"
                );
            }

            if(string.IsNullOrWhiteSpace(user.OtpCode)
               || user.OtpCode != dto.OtpCode)
            {
                return BadRequest(
                    "Invalid verification code"
                );
            }

            user.IsVerified = true;
            user.OtpCode = string.Empty;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message =
                "Email verified successfully"
            });
        }

        private static string GenerateOtp()
        {
            return RandomNumberGenerator
                .GetInt32(100000, 1000000)
                .ToString();
        }

        [Authorize]
        [HttpGet("profile")]
        public async Task<IActionResult> GetProfile()
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            var user = await _context.Users.FirstOrDefaultAsync(x => x.Id == userId);

            if (user == null)
            {
                return NotFound("User not found");
            }

            return Ok(new ProfileDto
            {
                Id = user.Id,
                Name = user.Name,
                Email = user.Email,
                CGPA = user.CGPA,
                Backlogs = user.Backlogs,
                Department = user.Department,
                PassingYear = user.PassingYear,
                CollegeName = user.CollegeName,
                HasBacklogs = user.HasBacklogs
            });
        }

        [Authorize]
        [HttpPut("profile")]
        public async Task<IActionResult> UpdateProfile(UpdateProfileDto dto)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            var user = await _context.Users.FirstOrDefaultAsync(x => x.Id == userId);

            if (user == null)
            {
                return NotFound("User not found");
            }

            user.Name = dto.Name;
            user.CGPA = dto.CGPA;
            user.Backlogs = dto.Backlogs;
            user.Department = dto.Department;
            user.PassingYear = dto.PassingYear;
            user.CollegeName = dto.CollegeName;
            user.HasBacklogs = dto.Backlogs > 0;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Profile updated successfully"
            });
        }
    }
}