using Microsoft.EntityFrameworkCore;
using TVOnline.Data;
using TVOnline.Models;

namespace TVOnline.Repository.Posts
{
    public class PostRepository(AppDbContext context) : IPostRepository
    {
        private readonly AppDbContext _context = context;

        public async Task<Post?> FindPostById(string id) => await _context.Posts.Include(p => p.Employer).Include(p => p.City).FirstOrDefaultAsync(p => p.PostId == id);

        public async Task<List<Post>> GetAllPosts() => await _context.Posts
            .Include(p => p.Employer)
            .Include(p => p.City).ToListAsync();

        public async Task<List<Post>> GetSeveralPosts(int quantity) => await _context.Posts
            .Include(p => p.Employer)
            .Include(p => p.City)
            .OrderByDescending(p => p.CreatedAt)
            .Take(quantity)
            .ToListAsync();

        public async Task<SavedJob?> FindSavedJob(string postId, string userId) =>
            await _context.SavedJobs.FirstOrDefaultAsync(
                s => s.PostId == postId && s.UserId == userId);

        public async Task<SavedJob> AddSavedJob(SavedJob savedJob)
        {
            await _context.SavedJobs.AddAsync(savedJob);
            await _context.SaveChangesAsync();
            return savedJob;
        }
        public async Task<bool> DeleteSavedJob(SavedJob savedJob)
        {
            _context.SavedJobs.Remove(savedJob);
            var result = await _context.SaveChangesAsync();
            return result > 0;
        }

        public async Task<List<SavedJob>> GetSavedPostsForJobSeeker(string jobSeekerId) // Corrected return type!
        {
            return await _context.SavedJobs
                .Where(s => s.UserId == jobSeekerId)
                .Include(s => s.Post) // Eager load SavedJob.Post navigation property
                .ThenInclude(p => p.Employer) // Then eager load Post.Employer navigation property
                .Include(s => s.Post) // Include Post again to eager load City
                .ThenInclude(p => p.City) // Then eager load Post.City navigation property
                .ToListAsync(); // Now returning List<SavedJob> directly!
        }

        public async Task<List<Post>> SearchPosts(string keyword, int? cityId, int page, int pageSize)
        {
            IQueryable<Post> query = _context.Posts
                .Include(p => p.Employer)
                .Include(p => p.City)
                .Where(p => p.IsActive);

            if (!string.IsNullOrEmpty(keyword))
            {
                keyword = keyword.ToLower();
                query = query.Where(p =>
                    p.Title.ToLower().Contains(keyword) ||
                    p.Description.ToLower().Contains(keyword) ||
                    p.Requirements.ToLower().Contains(keyword) ||
                    p.Employer.CompanyName.ToLower().Contains(keyword));
            }

            if (cityId.HasValue && cityId.Value > 0)
            {
                query = query.Where(p => p.CityId == cityId.Value);
            }

            return await query
                .OrderByDescending(p => p.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<int> CountSearchPosts(string keyword, int? cityId)
        {
            IQueryable<Post> query = _context.Posts.Where(p => p.IsActive);

            if (!string.IsNullOrEmpty(keyword))
            {
                keyword = keyword.ToLower();
                query = query.Where(p =>
                    p.Title.ToLower().Contains(keyword) ||
                    p.Description.ToLower().Contains(keyword) ||
                    p.Requirements.ToLower().Contains(keyword) ||
                    p.Employer.CompanyName.ToLower().Contains(keyword));
            }

            if (cityId.HasValue && cityId.Value > 0)
            {
                query = query.Where(p => p.CityId == cityId.Value);
            }

            return await query.CountAsync();
        }

        public async Task<List<Post>> FilterPosts(string keyword, int? cityId, decimal? minSalary, decimal? maxSalary, int? minExperience, int? maxExperience, int page, int pageSize)
        {
            IQueryable<Post> query = _context.Posts
                .Include(p => p.Employer)
                .Include(p => p.City)
                .Where(p => p.IsActive);

            // Apply keyword filter
            if (!string.IsNullOrEmpty(keyword))
            {
                keyword = keyword.ToLower();
                query = query.Where(p =>
                    p.Title.ToLower().Contains(keyword) ||
                    p.Description.ToLower().Contains(keyword) ||
                    p.Requirements.ToLower().Contains(keyword) ||
                    p.Employer.CompanyName.ToLower().Contains(keyword));
            }

            // Apply city filter
            if (cityId.HasValue && cityId.Value > 0)
            {
                query = query.Where(p => p.CityId == cityId.Value);
            }

            // Apply salary filters
            if (minSalary.HasValue)
            {
                query = query.Where(p => p.Salary >= minSalary.Value);
            }

            if (maxSalary.HasValue)
            {
                query = query.Where(p => p.Salary <= maxSalary.Value);
            }

            // Apply experience filters by parsing the Experience field
            if (minExperience.HasValue || maxExperience.HasValue)
            {
                // Get all posts first, we'll filter in-memory
                var posts = await query.ToListAsync();

                // Filter the posts based on experience
                posts = posts.Where(p =>
                {
                    // Get numbers from experience string
                    var experienceText = p.Experience.ToLower();
                    var numbers = System.Text.RegularExpressions.Regex.Matches(experienceText, @"\d+")
                        .Cast<System.Text.RegularExpressions.Match>()
                        .Select(m => int.Parse(m.Value))
                        .ToList();

                    if (numbers.Count == 0)
                        return false; // No numbers found in the experience string

                    // Handle case 1: "3 năm kinh nghiệm" - single value
                    if (numbers.Count == 1)
                    {
                        int experienceValue = numbers[0];

                        if (minExperience.HasValue && maxExperience.HasValue)
                        {
                            // Check if the single value is within the range
                            return experienceValue >= minExperience.Value &&
                                   experienceValue <= maxExperience.Value;
                        }
                        else if (minExperience.HasValue)
                        {
                            // Check if the single value is greater than or equal to min
                            return experienceValue >= minExperience.Value;
                        }
                        else if (maxExperience.HasValue)
                        {
                            // Check if the single value is less than or equal to max
                            return experienceValue <= maxExperience.Value;
                        }
                    }
                    // Handle case 2: "2-4 năm kinh nghiệm" - range value
                    else if (numbers.Count >= 2)
                    {
                        int minExp = numbers[0];
                        int maxExp = numbers[1];

                        if (minExperience.HasValue && maxExperience.HasValue)
                        {
                            // Check if there's any overlap between the ranges
                            return !(maxExperience.Value < minExp || minExperience.Value > maxExp);
                        }
                        else if (minExperience.HasValue)
                        {
                            // Check if max of range is greater than or equal to min
                            return maxExp >= minExperience.Value;
                        }
                        else if (maxExperience.HasValue)
                        {
                            // Check if min of range is less than or equal to max
                            return minExp <= maxExperience.Value;
                        }
                    }

                    return true; // Default to include if no filters applied
                }).ToList();

                // Apply pagination to the filtered posts
                return posts
                    .OrderByDescending(p => p.CreatedAt)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToList();
            }

            // If no experience filter, apply regular pagination through EF
            return await query
                .OrderByDescending(p => p.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<int> CountFilteredPosts(string keyword, int? cityId, decimal? minSalary, decimal? maxSalary, int? minExperience, int? maxExperience)
        {
            IQueryable<Post> query = _context.Posts.Where(p => p.IsActive);

            // Apply keyword filter
            if (!string.IsNullOrEmpty(keyword))
            {
                keyword = keyword.ToLower();
                query = query.Where(p =>
                    p.Title.ToLower().Contains(keyword) ||
                    p.Description.ToLower().Contains(keyword) ||
                    p.Requirements.ToLower().Contains(keyword) ||
                    p.Employer.CompanyName.ToLower().Contains(keyword));
            }

            // Apply city filter
            if (cityId.HasValue && cityId.Value > 0)
            {
                query = query.Where(p => p.CityId == cityId.Value);
            }

            // Apply salary filters
            if (minSalary.HasValue)
            {
                query = query.Where(p => p.Salary >= minSalary.Value);
            }

            if (maxSalary.HasValue)
            {
                query = query.Where(p => p.Salary <= maxSalary.Value);
            }

            // Apply experience filters by parsing the Experience field
            if (minExperience.HasValue || maxExperience.HasValue)
            {
                // Get all posts to filter in-memory
                var posts = await query.ToListAsync();

                // Filter the posts based on experience
                return posts.Count(p =>
                {
                    // Get numbers from experience string
                    var experienceText = p.Experience.ToLower();
                    var numbers = System.Text.RegularExpressions.Regex.Matches(experienceText, @"\d+")
                        .Cast<System.Text.RegularExpressions.Match>()
                        .Select(m => int.Parse(m.Value))
                        .ToList();

                    if (numbers.Count == 0)
                        return false; // No numbers found in the experience string

                    // Handle case 1: "3 năm kinh nghiệm" - single value
                    if (numbers.Count == 1)
                    {
                        int experienceValue = numbers[0];

                        if (minExperience.HasValue && maxExperience.HasValue)
                        {
                            // Check if the single value is within the range
                            return experienceValue >= minExperience.Value &&
                                   experienceValue <= maxExperience.Value;
                        }
                        else if (minExperience.HasValue)
                        {
                            // Check if the single value is greater than or equal to min
                            return experienceValue >= minExperience.Value;
                        }
                        else if (maxExperience.HasValue)
                        {
                            // Check if the single value is less than or equal to max
                            return experienceValue <= maxExperience.Value;
                        }
                    }
                    // Handle case 2: "2-4 năm kinh nghiệm" - range value
                    else if (numbers.Count >= 2)
                    {
                        int minExp = numbers[0];
                        int maxExp = numbers[1];

                        if (minExperience.HasValue && maxExperience.HasValue)
                        {
                            // Check if there's any overlap between the ranges
                            return !(maxExperience.Value < minExp || minExperience.Value > maxExp);
                        }
                        else if (minExperience.HasValue)
                        {
                            // Check if max of range is greater than or equal to min
                            return maxExp >= minExperience.Value;
                        }
                        else if (maxExperience.HasValue)
                        {
                            // Check if min of range is less than or equal to max
                            return minExp <= maxExperience.Value;
                        }
                    }

                    return true; // Default to include if no filters applied
                });
            }

            // If no experience filter, use EF Count
            return await query.CountAsync();
        }

        public async Task<List<Post>> GetPostsByEmployerId(string employerId)
        {
            return await _context.Posts
                .Where(p => p.EmployerId == employerId && p.IsActive)
                .Include(p => p.Employer)
                .Include(p => p.City)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();
        }
    }
}
