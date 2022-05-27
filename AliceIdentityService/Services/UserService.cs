using AliceIdentityService.Models;

namespace AliceIdentityService.Services
{
    public class UserService
    {
        private readonly AppDbContext _db;

        public UserService(AppDbContext db)
        {
            _db = db;
        }

        public List<User> GetUsers()
        {
            return _db.Users.OrderBy(u => u.FirstName).ThenBy(u => u.LastName).ToList();
        }

        public User GetUser(string id)
        {
            return _db.Users.Find(id);
        }

        public void SaveChanges() => _db.SaveChanges();
    }
}
