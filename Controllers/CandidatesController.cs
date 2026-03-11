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
    public class CandidatesController : Controller
    {
        private readonly SupabaseService _supabaseService;

        public CandidatesController(SupabaseService supabaseService)
        {
            _supabaseService = supabaseService;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var userId = Guid.Parse(User.FindFirst("UserId")?.Value);
            var isAdmin = User.IsInRole("admin");

            var allCandidates = await _supabaseService.GetAllCandidates();

            if (!isAdmin)
            {
                allCandidates = allCandidates.Where(c => c.CreatedBy == userId).ToList();
            }

            return View(allCandidates);
        }

        [HttpGet]
        public async Task<IActionResult> Create(Guid? jobId)
        {
            ViewBag.JobId = jobId;
            ViewBag.Jobs = await GetUserJobs();
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(Candidate candidate, Guid? jobId)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    candidate.Id = Guid.NewGuid();
                    candidate.CreatedBy = Guid.Parse(User.FindFirst("UserId")?.Value);
                    candidate.CreatedAt = DateTime.UtcNow;

                    Console.WriteLine($"Inserting candidate with ID: {candidate.Id}");

                    // Insert the candidate first
                    var insertedCandidate = await _supabaseService.InsertCandidate(candidate);

                    if (insertedCandidate == null)
                    {
                        ModelState.AddModelError("", "Failed to create candidate");
                        ViewBag.Jobs = await GetUserJobs();
                        return View(candidate);
                    }

                    Console.WriteLine($"Candidate inserted successfully with ID: {insertedCandidate.Id}");

                    // If jobId provided, create job application
                    if (jobId.HasValue && jobId.Value != Guid.Empty)
                    {
                        // Verify the job exists
                        var job = await _supabaseService.GetJobById(jobId.Value);
                        if (job == null)
                        {
                            TempData["Warning"] = "Candidate created but job not found. Application not created.";
                            return RedirectToAction("Index");
                        }

                        var application = new JobApplication
                        {
                            Id = Guid.NewGuid(),
                            JobId = jobId.Value,
                            CandidateId = insertedCandidate.Id, // Use the inserted candidate's ID
                            Stage = "applied",
                            AppliedDate = DateTime.UtcNow,
                            LastUpdated = DateTime.UtcNow,
                            Notes = "New candidate application"
                        };

                        Console.WriteLine($"Creating application for Candidate: {application.CandidateId}, Job: {application.JobId}");

                        var insertedApplication = await _supabaseService.InsertApplication(application);

                        if (insertedApplication != null)
                        {
                            TempData["Success"] = "Candidate added and application created successfully";
                            return RedirectToAction("Kanban", "Dashboard", new { jobId = jobId });
                        }
                        else
                        {
                            TempData["Warning"] = "Candidate created but application failed. Please try adding to job manually.";
                            return RedirectToAction("Index");
                        }
                    }

                    TempData["Success"] = "Candidate created successfully";
                    return RedirectToAction("Index");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error in Create: {ex.Message}");
                    Console.WriteLine($"Stack trace: {ex.StackTrace}");
                    ModelState.AddModelError("", $"Error creating candidate: {ex.Message}");
                }
            }

            ViewBag.Jobs = await GetUserJobs();
            return View(candidate);
        }

        [HttpPost]
        public async Task<IActionResult> Delete(string id)
        {
            if (Guid.TryParse(id, out var guidId))
            {
                await _supabaseService.DeleteCandidate(guidId);
            }
            return RedirectToAction("Index");
        }

        private async Task<List<Job>> GetUserJobs()
        {
            var userId = Guid.Parse(User.FindFirst("UserId")?.Value);
            var isAdmin = User.IsInRole("admin");

            var allJobs = await _supabaseService.GetAllJobs();

            if (!isAdmin)
            {
                allJobs = allJobs.Where(j => j.CreatedBy == userId).ToList();
            }

            return allJobs;
        }
    }
}