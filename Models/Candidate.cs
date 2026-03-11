using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace MiniATS.Models;

[Table("candidates")]
public class Candidate : BaseModel
{
    [PrimaryKey("id")]
    public Guid Id { get; set; }

    [Column("name")]
    public string? Name { get; set; }

    [Column("email")]
    public string? Email { get; set; }

    [Column("phone")]
    public string? Phone { get; set; }

    [Column("linkedin_url")]
    public string? LinkedInUrl { get; set; }

    [Column("current_company")]
    public string? CurrentCompany { get; set; }

    [Column("notes")]
    public string? Notes { get; set; }

    [Column("created_by")]
    public Guid CreatedBy { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; }
}