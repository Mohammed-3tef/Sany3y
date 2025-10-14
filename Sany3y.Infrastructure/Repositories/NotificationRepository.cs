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
    public class NotificationRepository : IRepository<Notification>
    {
        AppDbContext context;

        public NotificationRepository(AppDbContext _context)
        {
            this.context = _context;
        }

        async Task IRepository<Notification>.Add(Notification entity)
        {
            await context.AddAsync(entity);
            await context.SaveChangesAsync();
        }

        async Task IRepository<Notification>.Delete(Notification entity)
        {
            context.Remove(entity);
            await context.SaveChangesAsync();
        }

        async Task<List<Notification>> IRepository<Notification>.GetAll()
        {
            List<Notification> notifications = await context.Notifications.ToListAsync();
            return notifications;
        }

        async Task<Notification?> IRepository<Notification>.GetById(long id)
        {
            Notification? notification = await context.Notifications.FirstOrDefaultAsync(a => a.Id == id);
            return notification;
        }

        async Task IRepository<Notification>.Update(Notification entity)
        {
            Notification? notification = await ((IRepository<Notification>)this).GetById(entity.Id);
            if (notification == null)
                return;

            notification.Title = entity.Title;
            notification.Message = entity.Message;
            notification.CreatedAt = entity.CreatedAt;
            notification.IsRead = entity.IsRead;

            context.Update(notification);
            await context.SaveChangesAsync();
        }
    }
}
