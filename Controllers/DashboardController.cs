using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MiniATS.Services;
using MiniATS.ViewModels;

namespace MiniATS.Controllers
{
    [Authorize]
    public class DashboardController : Controller
    {
        private readonly SupabaseService _supabaseService;

        public DashboardController(SupabaseService supabaseService)
        {
            _supabaseService = supabaseService;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> Kanban(string jobId, string searchTerm)
        {
            var userId = Guid.Parse(User.FindFirst("UserId")?.Value);
            var isAdmin = User.IsInRole("admin");

            var viewModel = new KanbanViewModel
            {
                SelectedJobId = jobId,
                SearchTerm = searchTerm,
                Columns = new Dictionary<string, List<KanbanItem>>()
            };

            // Get user's jobs
            var allJobs = await _supabaseService.GetAllJobs();
            if (!isAdmin)
            {
                viewModel.Jobs = allJobs.Where(j => j.CreatedBy == userId).ToList();
            }
            else
            {
                viewModel.Jobs = allJobs;
            }

            // Get candidates based on filters
            var allCandidates = await _supabaseService.GetAllCandidates();
            var userCandidates = !isAdmin
                ? allCandidates.Where(c => c.CreatedBy == userId).ToList()
                : allCandidates;

            if (!string.IsNullOrEmpty(searchTerm))
            {
                viewModel.Candidates = userCandidates
                    .Where(c => c.Name.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                               (c.Email != null && c.Email.Contains(searchTerm, StringComparison.OrdinalIgnoreCase)))
                    .ToList();
            }
            else
            {
                viewModel.Candidates = userCandidates;
            }

            // Get all applications
            var allApplications = await _supabaseService.GetAllApplications();

            // Filter applications to only include user's jobs/candidates
            var userJobIds = viewModel.Jobs.Select(j => j.Id).ToList();
            var userCandidateIds = viewModel.Candidates.Select(c => c.Id).ToList();

            viewModel.Applications = allApplications
                .Where(a => userJobIds.Contains(a.JobId) && userCandidateIds.Contains(a.CandidateId))
                .ToList();

            // Filter by job if specified
            if (!string.IsNullOrEmpty(jobId) && Guid.TryParse(jobId, out var parsedJobId))
            {
                viewModel.Applications = viewModel.Applications
                    .Where(a => a.JobId == parsedJobId)
                    .ToList();
            }

            // Group by stage
            var stages = new[] { "applied", "screening", "interview", "offer", "rejected" };

            foreach (var stage in stages)
            {
                var items = new List<KanbanItem>();

                var stageApplications = viewModel.Applications
                    .Where(a => a.Stage == stage)
                    .ToList();

                foreach (var app in stageApplications)
                {
                    var candidate = viewModel.Candidates.FirstOrDefault(c => c.Id == app.CandidateId);
                    var job = viewModel.Jobs.FirstOrDefault(j => j.Id == app.JobId);

                    if (candidate != null && job != null)
                    {
                        items.Add(new KanbanItem
                        {
                            Id = app.Id.ToString(),
                            CandidateName = candidate.Name,
                            JobTitle = job.Title,
                            Stage = stage,
                            Rating = app.Rating,
                            LinkedInUrl = candidate.LinkedInUrl
                        });
                    }
                }

                viewModel.Columns[stage] = items;
            }

            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateApplicationStage([FromBody] UpdateStageModel model)
        {
            try
            {
                if (model == null || string.IsNullOrEmpty(model.ApplicationId) || string.IsNullOrEmpty(model.NewStage))
                {
                    return Json(new { success = false, message = "Invalid request data" });
                }

                if (!Guid.TryParse(model.ApplicationId, out var applicationId))
                {
                    return Json(new { success = false, message = "Invalid application ID format" });
                }

                var application = await _supabaseService.GetApplicationById(applicationId);

                if (application != null)
                {
                    var oldStage = application.Stage;
                    application.Stage = model.NewStage;
                    application.LastUpdated = DateTime.UtcNow;

                    var updated = await _supabaseService.UpdateApplication(application);

                    if (updated != null)
                    {
                        return Json(new { success = true, message = $"Moved from {oldStage} to {model.NewStage}" });
                    }
                    else
                    {
                        return Json(new { success = false, message = "Failed to update application" });
                    }
                }

                return Json(new { success = false, message = "Application not found" });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in UpdateApplicationStage: {ex.Message}");
                return Json(new { success = false, message = ex.Message });
            }
        }
    }
}