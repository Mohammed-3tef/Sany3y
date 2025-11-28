using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Sany3y.Infrastructure.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Task = System.Threading.Tasks.Task;

namespace Sany3y.Infrastructure.Repositories
{
    public class UserRepository
    {
        AppDbContext context;
        private readonly UserManager<User> userManager;

        public UserRepository(AppDbContext _context, UserManager<User> _userManager)
        {
            this.context = _context;
            this.userManager = _userManager;
        }

        public async Task<IdentityResult> Add(User entity, string password)
        {
            return await userManager.CreateAsync(entity, password);
        }

        public async Task<IdentityResult> Update(User entity)
        {
            return await userManager.UpdateAsync(entity);
        }

        public async Task<IdentityResult> Delete(User entity)
        {
            return await userManager.DeleteAsync(entity);
        }

        public async Task<List<User>> GetAll()
        {
            List<User> users = await context.Users.AsNoTracking().ToListAsync();
            return users;
        }

        public async Task<User?> GetById(long id)
        {
            return await userManager.FindByIdAsync(id.ToString());
        }

        public async Task<User?> GetByNationalId(long nationalId)
        {
            User? user = await context.Users.FirstOrDefaultAsync(a => a.NationalId == nationalId);
            return user;
        }
        
        public async Task<User?> GetByEmail(string email)
        {
            return await userManager.FindByEmailAsync(email);
        }
        
        public async Task<User?> GetByUsername(string username)
        {
            return await userManager.FindByNameAsync(username);
        }

        public async Task<List<User>> GetUsersByRole(string roleName)
        {
            var usersInRole = await userManager.GetUsersInRoleAsync(roleName);
            var userIds = usersInRole.Select(u => u.Id).ToList();

            return await context.Users
                .Include(u => u.Address)
                .Include(u => u.ProfilePicture)
                .Where(u => userIds.Contains(u.Id))
                .ToListAsync();
        }
    }
}
