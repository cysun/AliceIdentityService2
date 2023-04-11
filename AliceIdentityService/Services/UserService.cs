using AliceIdentityService.Models;
using Microsoft.EntityFrameworkCore;

namespace AliceIdentityService.Services;

public class UserService
{
    public enum CountType
    {
        Total,      // total # of the users
        Recent,     // # of users who registered/added in the last 30 days
        Unconfirmed // # of users whose emails are not confirmed
    }

    private readonly AppDbContext _db;

    public UserService(AppDbContext db)
    {
        _db = db;
    }

    public List<User> GetUsers() => _db.Users.AsNoTracking()
        .OrderBy(u => u.FirstName).ThenBy(u => u.LastName)
        .ToList();

    public List<User> GetRecentUsers(int days = 30) => _db.Users.AsNoTracking()
        .Where(u => u.CreationTime > DateTime.UtcNow.AddDays(-days))
        .OrderByDescending(u => u.CreationTime)
        .ToList();

    public List<User> GetUnconfirmedUsers() => _db.Users.AsNoTracking()
        .Where(u => !u.EmailConfirmed)
        .OrderBy(u => u.FirstName).ThenBy(u => u.LastName)
        .ToList();

    // maxResults=null for unlimited results
    public List<User> SearchUsersByPrefix(string prefix, int? maxResults = 100)
    {
        prefix = prefix?.Trim();
        if (prefix == null || prefix.Length < 2) return new List<User>();

        return _db.Users.FromSqlRaw("SELECT * FROM \"SearchUsers\"({0}, {1})",
            $"{prefix}%".ToLower(), maxResults).AsNoTracking().ToList();
    }

    public User GetUser(string id)
    {
        return _db.Users.Find(id);
    }

    public Dictionary<CountType, int> GetCounts() => new Dictionary<CountType, int>
        {
            { CountType.Total, _db.Users.Count() },
            { CountType.Recent, _db.Users.Where(u => u.CreationTime > DateTime.UtcNow.AddDays(-30)).Count() },
            { CountType.Unconfirmed, _db.Users.Where(u => !u.EmailConfirmed).Count() }
        };

    public void SaveChanges() => _db.SaveChanges();
}
