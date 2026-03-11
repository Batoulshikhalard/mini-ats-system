using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace MiniATS.Models;

[Table("users")]
public class User : BaseModel
{
    [PrimaryKey("id")]
    public Guid Id { get; set; }

    [Column("email")]
    public string? Email { get; set; }

    [Column("full_name")]
    public string? FullName { get; set; }

    [Column("role")]
    public string? Role { get; set; } // "admin" or "customer"

    [Column("created_at")]
    public DateTime CreatedAt { get; set; }
}