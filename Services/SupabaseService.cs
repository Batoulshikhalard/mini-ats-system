using Supabase;
using MiniATS.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MiniATS.Services
{
    public class SupabaseService
    {
        private readonly Client _supabaseClient;

        public SupabaseService(IConfiguration configuration)
        {
            var supabaseUrl = configuration["Supabase:Url"];
            var supabaseKey = configuration["Supabase:Key"];

            var options = new SupabaseOptions
            {
                AutoConnectRealtime = true
            };

            _supabaseClient = new Client(supabaseUrl, supabaseKey, options);
            _supabaseClient.InitializeAsync().Wait();
        }

        public Client Client => _supabaseClient;

        // Simplified helper methods - removed problematic generic ones
        public async Task<List<Job>> GetAllJobs()
        {
            var response = await _supabaseClient.From<Job>().Get();
            return response.Models;
        }

        public async Task<Job> GetJobById(Guid id)
        {
            var response = await _supabaseClient.From<Job>()
                .Where(x => x.Id == id)
                .Get();
            return response.Models.FirstOrDefault();
        }

        public async Task<Job> InsertJob(Job job)
        {
            var response = await _supabaseClient.From<Job>().Insert(job);
            return response.Models.FirstOrDefault();
        }

        public async Task<Job> UpdateJob(Job job)
        {
            var response = await _supabaseClient.From<Job>().Update(job);
            return response.Models.FirstOrDefault();
        }

        public async Task DeleteJob(Guid id)
        {
            await _supabaseClient.From<Job>()
                .Where(x => x.Id == id)
                .Delete();
        }

        // Candidate methods
        public async Task<List<Candidate>> GetAllCandidates()
        {
            var response = await _supabaseClient.From<Candidate>().Get();
            return response.Models;
        }

        public async Task<Candidate> GetCandidateById(Guid id)
        {
            var response = await _supabaseClient.From<Candidate>()
                .Where(x => x.Id == id)
                .Get();
            return response.Models.FirstOrDefault();
        }

        public async Task<Candidate> InsertCandidate(Candidate candidate)
        {
            try
            {
                Console.WriteLine($"InsertCandidate called with ID: {candidate.Id}");

                var response = await _supabaseClient
                    .From<Candidate>()
                    .Insert(candidate);

                var inserted = response.Models.FirstOrDefault();

                if (inserted == null)
                {
                    Console.WriteLine("InsertCandidate returned null");
                    throw new Exception("Failed to insert candidate - no response");
                }

                Console.WriteLine($"InsertCandidate successful, returned ID: {inserted.Id}");
                return inserted;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"InsertCandidate error: {ex.Message}");
                throw;
            }
        }

        public async Task<Candidate> UpdateCandidate(Candidate candidate)
        {
            var response = await _supabaseClient.From<Candidate>().Update(candidate);
            return response.Models.FirstOrDefault();
        }

        public async Task DeleteCandidate(Guid id)
        {
            await _supabaseClient.From<Candidate>()
                .Where(x => x.Id == id)
                .Delete();
        }

        // JobApplication methods
        public async Task<List<JobApplication>> GetAllApplications()
        {
            var response = await _supabaseClient.From<JobApplication>().Get();
            return response.Models;
        }

        public async Task<JobApplication> GetApplicationById(Guid id)
        {
            var response = await _supabaseClient.From<JobApplication>()
                .Where(x => x.Id == id)
                .Get();
            return response.Models.FirstOrDefault();
        }

        public async Task<JobApplication> InsertApplication(JobApplication application)
        {
            try
            {
                Console.WriteLine($"InsertApplication called for Candidate: {application.CandidateId}, Job: {application.JobId}");

                // Verify candidate exists before inserting
                var candidate = await GetCandidateById(application.CandidateId);
                if (candidate == null)
                {
                    throw new Exception($"Candidate with ID {application.CandidateId} does not exist");
                }

                var response = await _supabaseClient
                    .From<JobApplication>()
                    .Insert(application);

                var inserted = response.Models.FirstOrDefault();

                if (inserted == null)
                {
                    Console.WriteLine("InsertApplication returned null");
                    throw new Exception("Failed to insert application - no response");
                }

                Console.WriteLine($"InsertApplication successful, returned ID: {inserted.Id}");
                return inserted;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"InsertApplication error: {ex.Message}");
                throw;
            }
        }

        public async Task<JobApplication> UpdateApplication(JobApplication application)
        {
            var response = await _supabaseClient.From<JobApplication>().Update(application);
            return response.Models.FirstOrDefault();
        }

        public async Task DeleteApplication(Guid id)
        {
            await _supabaseClient.From<JobApplication>()
                .Where(x => x.Id == id)
                .Delete();
        }

        // User methods
        public async Task<List<User>> GetAllUsers()
        {
            var response = await _supabaseClient.From<User>().Get();
            return response.Models;
        }

        public async Task<User> GetUserByEmail(string email)
        {
            var response = await _supabaseClient.From<User>()
                .Where(x => x.Email == email)
                .Get();
            return response.Models.FirstOrDefault();
        }

        public async Task<User> GetUserById(Guid id)
        {
            var response = await _supabaseClient.From<User>()
                .Where(x => x.Id == id)
                .Get();
            return response.Models.FirstOrDefault();
        }

        public async Task<User> InsertUser(User user)
        {
            var response = await _supabaseClient.From<User>().Insert(user);
            return response.Models.FirstOrDefault();
        }

        public async Task<User> UpdateUser(User user)
        {
            var response = await _supabaseClient.From<User>().Update(user);
            return response.Models.FirstOrDefault();
        }

        public async Task DeleteUser(Guid id)
        {
            await _supabaseClient.From<User>()
                .Where(x => x.Id == id)
                .Delete();
        }
    }
}