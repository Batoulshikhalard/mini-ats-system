using MiniATS.Models;

namespace MiniATS.ViewModels;

public class KanbanViewModel
{
    public List<Job> Jobs { get; set; }
    public List<Candidate> Candidates { get; set; }
    public List<JobApplication> Applications { get; set; }
    public string? SelectedJobId { get; set; }
    public string? SearchTerm { get; set; }

    public Dictionary<string, List<KanbanItem>> Columns { get; set; }
}

public class KanbanItem
{
    public string Id { get; set; }
    public string? CandidateName { get; set; }
    public string? JobTitle { get; set; }
    public string? Stage { get; set; }
    public int? Rating { get; set; }
    public string? LinkedInUrl { get; set; }
}
public class UpdateStageModel
{
    public string? ApplicationId { get; set; }
    public string? NewStage { get; set; }
}
