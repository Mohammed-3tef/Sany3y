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
    public class ProfilePictureRepository : IRepository<ProfilePicture>
    {
        AppDbContext context;

        public ProfilePictureRepository(AppDbContext _context)
        {
            this.context = _context;
        }

        async Task IRepository<ProfilePicture>.Add(ProfilePicture entity)
        {
            await context.AddAsync(entity);
            await context.SaveChangesAsync();
        }

        async Task IRepository<ProfilePicture>.Delete(ProfilePicture entity)
        {
            context.Remove(entity);
            await context.SaveChangesAsync();
        }

        async Task<List<ProfilePicture>> IRepository<ProfilePicture>.GetAll()
        {
            List<ProfilePicture> pictures = await context.ProfilePictures.ToListAsync();
            return pictures;
        }

        async Task<ProfilePicture?> IRepository<ProfilePicture>.GetById(long id)
        {
            ProfilePicture? picture = await context.ProfilePictures.FirstOrDefaultAsync(a => a.Id == id);
            return picture;
        }

        async Task IRepository<ProfilePicture>.Update(ProfilePicture entity)
        {
            ProfilePicture? picture = await ((IRepository<ProfilePicture>)this).GetById(entity.Id);
            if (picture == null)
                return;

            picture.Path = entity.Path;
            context.Update(picture);
            await context.SaveChangesAsync();
        }
    }
}
