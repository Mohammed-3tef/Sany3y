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
    public class MessageRepository : IRepository<Message>
    {
        AppDbContext context;

        public MessageRepository(AppDbContext _context)
        {
            this.context = _context;
        }

        async Task IRepository<Message>.Add(Message entity)
        {
            await context.AddAsync(entity);
            await context.SaveChangesAsync();
        }

        async Task IRepository<Message>.Delete(Message entity)
        {
            context.Remove(entity);
            await context.SaveChangesAsync();
        }

        async Task<List<Message>> IRepository<Message>.GetAll()
        {
            List<Message> messages = await context.Messages.ToListAsync();
            return messages;
        }

        async Task<Message?> IRepository<Message>.GetById(long id)
        {
            Message? message = await context.Messages.FirstOrDefaultAsync(a => a.Id == id);
            return message;
        }

        async Task IRepository<Message>.Update(Message entity)
        {
            Message? message = await ((IRepository<Message>)this).GetById(entity.Id);
            if (message == null)
                return;

            message.Content = entity.Content;
            message.SentAt = entity.SentAt;
            context.Update(message);
            await context.SaveChangesAsync();
        }
    }
}
