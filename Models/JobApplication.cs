using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace MiniATS.Models;

[Table("job_applications")]
public class JobApplication : BaseModel
{
    [PrimaryKey("id")]
    public Guid Id { get; set; }

    [Column("job_id")]
    public Guid JobId { get; set; }

    [Column("candidate_id")]
    public Guid CandidateId { get; set; }

    [Column("stage")]
    public string? Stage { get; set; } // "applied", "screening", "interview", "offer", "rejected"

    [Column("applied_date")]
    public DateTime AppliedDate { get; set; }

    [Column("last_updated")]
    public DateTime LastUpdated { get; set; }

    [Column("rating")]
    public int? Rating { get; set; }

    [Column("notes")]
    public string? Notes { get; set; }
}