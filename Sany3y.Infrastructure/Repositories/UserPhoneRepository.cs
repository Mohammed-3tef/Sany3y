using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Sany3y.Infrastructure.Models;
using Task = System.Threading.Tasks.Task;

namespace Sany3y.Infrastructure.Repositories
{
    public class UserPhoneRepository : IRepository<UserPhone>
    {
        AppDbContext context;

        public UserPhoneRepository(AppDbContext _context)
        {
            this.context = _context;
        }

        async Task IRepository<UserPhone>.Add(UserPhone entity)
        {
            await context.AddAsync(entity);
            await context.SaveChangesAsync();
        }

        async Task IRepository<UserPhone>.Delete(UserPhone entity)
        {
            context.Remove(entity);
            await context.SaveChangesAsync();
        }

        async Task<List<UserPhone>> IRepository<UserPhone>.GetAll()
        {
            List<UserPhone> phones = await context.UserPhones.ToListAsync();
            return phones;
        }

        async Task<UserPhone?> IRepository<UserPhone>.GetById(long id)
        {
            UserPhone? phone = await context.UserPhones.FirstOrDefaultAsync(a => a.Id == id);
            return phone;
        }

        async Task IRepository<UserPhone>.Update(UserPhone entity)
        {
            UserPhone? phone = await ((IRepository<UserPhone>)this).GetById(entity.Id);
            if (phone == null)
                return;

            phone.PhoneNumber = entity.PhoneNumber;
            context.Update(phone);
            await context.SaveChangesAsync();
        }
    }
}
