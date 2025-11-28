using Microsoft.EntityFrameworkCore;
using Sany3y.Infrastructure.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sany3y.API.Services.CountryServices
{
    public class CityServices
    {
        AppDbContext _context;

        public CityServices(AppDbContext context) { 
            _context = context;
        }

        public async Task<List<City>> GetAll()
        {
            return await _context.Cities.AsNoTracking().ToListAsync();
        }

        public async Task<City> GetByID(int cityId)
        {
            return await _context.Cities.AsNoTracking().FirstOrDefaultAsync(c => c.Id == cityId);
        }
    }
}
