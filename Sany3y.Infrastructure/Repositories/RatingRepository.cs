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
    public class RatingRepository : IRepository<Rating>
    {
        AppDbContext context;

        public RatingRepository(AppDbContext _context)
        {
            this.context = _context;
        }

        async Task IRepository<Rating>.Add(Rating entity)
        {
            await context.AddAsync(entity);
            await context.SaveChangesAsync();
        }

        async Task IRepository<Rating>.Delete(Rating entity)
        {
            context.Remove(entity);
            await context.SaveChangesAsync();
        }

        async Task<List<Rating>> IRepository<Rating>.GetAll()
        {
            List<Rating> ratings = await context.Ratings.ToListAsync();
            return ratings;
        }

        async Task<Rating?> IRepository<Rating>.GetById(long id)
        {
            Rating? rating = await context.Ratings.FirstOrDefaultAsync(a => a.Id == id);
            return rating;
        }

        async Task IRepository<Rating>.Update(Rating entity)
        {
            Rating? rating = await ((IRepository<Rating>)this).GetById(entity.Id);
            if (rating == null)
                return;

            rating.Comment = entity.Comment;
            rating.Score = entity.Score;
            rating.CreatedAt = entity.CreatedAt;

            context.Update(rating);
            await context.SaveChangesAsync();
        }
    }
}
