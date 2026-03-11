using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using MiniATS.Models;
using MiniATS.Services;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

namespace MiniATS.Controllers
{
    public class AccountController : Controller
    {
        private readonly SupabaseService _supabaseService;

        public AccountController(SupabaseService supabaseService)
        {
            _supabaseService = supabaseService;
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(string email, string password)
        {
            try
            {
                Console.WriteLine($"Attempting login for email: {email}");
                var session = await _supabaseService.Client.Auth.SignIn(email, password);

                if (session != null)
                {
                    // Get the user ID from the session - it's a string that needs to be parsed
                    Console.WriteLine($"Session user ID: {session.User.Id}");

                    // Parse the user ID from string to Guid
                    if (!Guid.TryParse(session.User.Id, out var userId))
                    {
                        ModelState.AddModelError("", "Invalid user ID format from authentication");
                        return View();
                    }

                    // Get user role from custom users table
                    var user = await _supabaseService.GetUserByEmail(email);

                    if (user == null)
                    {
                        Console.WriteLine("User not found in custom table, creating...");

                        // Create user record if it doesn't exist
                        user = new User
                        {
                            Id = userId,  // Use the parsed Guid from auth session
                            Email = email,
                            FullName = email.Split('@')[0], // Default name from email
                            Role = "customer", // Default role
                            CreatedAt = DateTime.UtcNow
                        };

                        try
                        {
                            await _supabaseService.InsertUser(user);
                            Console.WriteLine("User created successfully in custom table");
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Error inserting user: {ex.Message}");
                            ModelState.AddModelError("", "Error creating user profile");
                            return View();
                        }
                    }

                    var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.FullName),
                new Claim(ClaimTypes.Email, email),
                new Claim(ClaimTypes.Role, user.Role),
                new Claim("UserId", user.Id.ToString())
            };

                    var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                    var authProperties = new AuthenticationProperties
                    {
                        IsPersistent = true
                    };

                    await HttpContext.SignInAsync(
                        CookieAuthenticationDefaults.AuthenticationScheme,
                        new ClaimsPrincipal(claimsIdentity),
                        authProperties);

                    return RedirectToAction("Index", "Dashboard");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Login error: {ex.Message}");
                ModelState.AddModelError("", "Invalid login attempt: " + ex.Message);
            }

            return View();
        }
        [HttpGet]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            await _supabaseService.Client.Auth.SignOut();
            return RedirectToAction("Login");
        }

        [HttpGet]
        public IActionResult CreateAccount()
        {
            if (!User.IsInRole("admin"))
                return Forbid();

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> CreateAccount(string email, string password, string fullName, string role)
        {
            if (!User.IsInRole("admin"))
                return Forbid();

            try
            {
                Console.WriteLine($"Creating account for email: {email}");

                // Create auth user in Supabase
                var user = await _supabaseService.Client.Auth.SignUp(email, password);

                if (user == null || user.User == null)
                {
                    ModelState.AddModelError("", "Failed to create authentication user");
                    return View();
                }

                Console.WriteLine($"Auth user created with ID: {user.User.Id}");

                // Parse the user ID from string to Guid
                if (!Guid.TryParse(user.User.Id, out var userId))
                {
                    ModelState.AddModelError("", "Invalid user ID format returned from Supabase");
                    return View();
                }

                // Add to custom users table
                var newUser = new User
                {
                    Id = userId,
                    Email = email,
                    FullName = fullName,
                    Role = role,
                    CreatedAt = DateTime.UtcNow
                };

                try
                {
                    await _supabaseService.InsertUser(newUser);
                    Console.WriteLine("User inserted successfully into custom table");
                    TempData["Success"] = "Account created successfully";
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error inserting user into custom table: {ex.Message}");

                    // Since we can't delete the auth user via Admin API easily,
                    // we'll just log the error and inform the admin
                    ModelState.AddModelError("", "User created in Auth but failed to create profile. Please check the users table.");
                    return View();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error creating account: {ex.Message}");
                ModelState.AddModelError("", "Error creating account: " + ex.Message);
                return View();
            }

            return RedirectToAction("Index", "Dashboard");
        }
    }
}