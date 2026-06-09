using InternshipPortalApi.Data;
using InternshipPortalApi.Models;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace InternshipPortalApi.Controllers
{
    [ApiController]

    [Route("api/[controller]")]

    public class MaterialController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public MaterialController(
            ApplicationDbContext context
        )
        {
            _context = context;
        }

        [Authorize(Roles = "Admin")]

        [HttpPost]
        public async Task<IActionResult> CreateMaterial(
            StudyMaterial material
        )
        {
            material.UploadedAt =
            DateTime.Now;

            _context.StudyMaterials.Add(
                material
            );

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message =
                "Material uploaded successfully"
            });
        }

        [Authorize]

        [HttpGet]
        public async Task<IActionResult> GetMaterials()
        {
            var materials =
            await _context.StudyMaterials

            .OrderByDescending(
                x => x.UploadedAt
            )

            .ToListAsync();

            return Ok(materials);
        }
    }
}