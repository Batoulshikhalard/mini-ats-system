using MiniATS.Models;

namespace MiniATS.Services;
public class DatabaseSeeder
{
    private readonly SupabaseService _supabaseService;

    public DatabaseSeeder(SupabaseService supabaseService)
    {
        _supabaseService = supabaseService;
    }

    public async Task SeedAdminUser()
    {
        try
        {
            // Check if any admin exists
            var users = await _supabaseService.GetAllUsers();
            var adminExists = users.Any(u => u.Role == "admin");

            if (!adminExists)
            {
                // Create admin user in Supabase Auth
                var email = "admin@ats.com";
                var password = "Admin123!"; // Change this in production!

                try
                {
                    var newUser = await _supabaseService.Client.Auth.SignUp(email, password);

                    if (newUser != null && Guid.TryParse(newUser.User.Id, out var userId))
                    {
                        // Create user record
                        var adminUser = new User
                        {
                            Id = userId,
                            Email = email,
                            FullName = "System Administrator",
                            Role = "admin",
                            CreatedAt = DateTime.UtcNow
                        };

                        await _supabaseService.InsertUser(adminUser);
                        Console.WriteLine("Admin user created successfully");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error creating admin user: {ex.Message}");
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in database seeding: {ex.Message}");
        }
    }
}