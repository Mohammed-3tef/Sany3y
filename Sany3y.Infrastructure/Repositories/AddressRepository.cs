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
    public class AddressRepository : IRepository<Address>
    {
        AppDbContext context;

        public AddressRepository(AppDbContext _context)
        {
            this.context = _context;
        }

        async Task IRepository<Address>.Add(Address entity)
        {
            await context.AddAsync(entity);
            await context.SaveChangesAsync();
        }

        async Task IRepository<Address>.Delete(Address entity)
        {
            context.Remove(entity);
            await context.SaveChangesAsync();
        }

        async Task<List<Address>> IRepository<Address>.GetAll()
        {
            List<Address> addresses = await context.Addresses.ToListAsync();
            return addresses;
        }

        async Task<Address?> IRepository<Address>.GetById(long id)
        {
            Address? address = await context.Addresses.FirstOrDefaultAsync(a => a.Id == id);
            return address;
        }

        async Task IRepository<Address>.Update(Address entity)
        {
            Address? address = await ((IRepository<Address>)this).GetById(entity.Id);
            if (address == null)
                return;

            address.Street = entity.Street;
            address.City = entity.City;
            context.Update(address);
            await context.SaveChangesAsync();
        }
    }
}
