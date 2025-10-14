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
    public class CategoryRepository : IRepository<Category>
    {
        AppDbContext dbContext;

        public CategoryRepository(AppDbContext _dbContext)
        {
            dbContext = _dbContext;
        }

        async Task IRepository<Category>.Add(Category entity)
        {
            await dbContext.Categories.AddAsync(entity);
            await dbContext.SaveChangesAsync();
            return;
        }

        async Task IRepository<Category>.Delete(Category entity)
        {
            dbContext.Categories.Remove(entity);
            await dbContext.SaveChangesAsync();
            return;
        }

        async Task<List<Category>> IRepository<Category>.GetAll()
        {
            return await dbContext.Categories.AsNoTracking().ToListAsync();
        }

        async Task<Category?> IRepository<Category>.GetById(long id)
        {
            return await dbContext.Categories.FirstOrDefaultAsync(c => c.Id == id);
        }

        async Task IRepository<Category>.Update(Category entity)
        {
            var entityToUpdate = await ((IRepository<Category>)this).GetById(entity.Id);

            if (entityToUpdate == null)
                return;

            entityToUpdate.Name = entity.Name;
            entityToUpdate.Description = entity.Description;

            dbContext.Categories.Update(entity);
            await dbContext.SaveChangesAsync();
        }
    }
}
