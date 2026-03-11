using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace MiniATS.Models;

[Table("jobs")]
public class Job : BaseModel
{
    [PrimaryKey("id")]
    public Guid Id { get; set; }

    [Column("title")]
    public string? Title { get; set; }

    [Column("description")]
    public string? Description { get; set; }

    [Column("department")]
    public string? Department { get; set; }

    [Column("location")]
    public string? Location { get; set; }

    [Column("created_by")]
    public Guid CreatedBy { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; }

    [Column("status")]
    public string? Status { get; set; } // "active", "closed"
}
