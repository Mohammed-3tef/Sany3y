using Microsoft.EntityFrameworkCore;
using Sany3y.Infrastructure.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sany3y.API.Services.CountryServices
{
    public class GovernorateServices
    {
        AppDbContext _context;

        public GovernorateServices(AppDbContext context) { 
            _context = context;
        }

        public async Task<List<Governorate>> GetAll()
        {
            return await _context.Governorates.AsNoTracking().ToListAsync();
        }

        public async Task<Governorate> GetByID(int governorateId)
        {
            return await _context.Governorates.AsNoTracking().FirstOrDefaultAsync(c => c.Id == governorateId);
        }

        public async Task<List<City>> GetCitiesByGovernorateId(int governorateId)
        {
            return await _context.Cities 
                .Where(c => c.GovernorateId == governorateId)
                .ToListAsync();
        }
    }
}
