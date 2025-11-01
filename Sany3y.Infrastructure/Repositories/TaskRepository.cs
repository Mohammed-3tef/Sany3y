using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Sany3y.Infrastructure.Models;
using Task = Sany3y.Infrastructure.Models.Task;

namespace Sany3y.Infrastructure.Repositories
{
    public class TaskRepository : IRepository<Task>
    {
        AppDbContext context;

        public TaskRepository(AppDbContext _context)
        {
            this.context = _context;
        }

        public async System.Threading.Tasks.Task Add(Task entity)
        {
            await context.AddAsync(entity);
            await context.SaveChangesAsync();
        }

        public async System.Threading.Tasks.Task Delete(Task entity)
        {
            context.Remove(entity);
            await context.SaveChangesAsync();
        }

        public async System.Threading.Tasks.Task<List<Task>> GetAll()
        {
            List<Task> tasks = await context.Tasks.ToListAsync();
            return tasks;
        }

        public System.Threading.Tasks.Task<Task?> GetById(long id)
        {
            Task? task =  context.Tasks.FirstOrDefault(a => a.Id == id);
            return System.Threading.Tasks.Task.FromResult(task);
        }

        public System.Threading.Tasks.Task Update(Task entity)
        {
            Task? task =  context.Tasks.FirstOrDefault(a => a.Id == entity.Id);
            if (task == null)
                return System.Threading.Tasks.Task.CompletedTask;

            task.Title = entity.Title;
            task.Description = entity.Description;
            task.Status = entity.Status;

            context.Update(task);
            return context.SaveChangesAsync();
        }
    }
}
