using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MiniATS.Models;
using MiniATS.Services;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace MiniATS.Controllers
{
    [Authorize]
    public class JobsController : Controller
    {
        private readonly SupabaseService _supabaseService;

        public JobsController(SupabaseService supabaseService)
        {
            _supabaseService = supabaseService;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var userId = Guid.Parse(User.FindFirst("UserId")?.Value);
            var isAdmin = User.IsInRole("admin");

            var allJobs = await _supabaseService.GetAllJobs();

            if (!isAdmin)
            {
                allJobs = allJobs.Where(j => j.CreatedBy == userId).ToList();
            }

            return View(allJobs);
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(Job job)
        {
            if (ModelState.IsValid)
            {
                job.Id = Guid.NewGuid();
                job.CreatedBy = Guid.Parse(User.FindFirst("UserId")?.Value);
                job.CreatedAt = DateTime.UtcNow;
                job.Status = "active";

                await _supabaseService.InsertJob(job);

                return RedirectToAction("Index");
            }

            return View(job);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(Guid id)
        {
            var job = await _supabaseService.GetJobById(id);
            if (job == null)
                return NotFound();

            return View(job);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(Job job)
        {
            if (ModelState.IsValid)
            {
                await _supabaseService.UpdateJob(job);
                return RedirectToAction("Index");
            }

            return View(job);
        }

        [HttpPost]
        public async Task<IActionResult> Delete(string id)
        {
            if (Guid.TryParse(id, out var guidId))
            {
                await _supabaseService.DeleteJob(guidId);
            }
            return RedirectToAction("Index");
        }
    }
}