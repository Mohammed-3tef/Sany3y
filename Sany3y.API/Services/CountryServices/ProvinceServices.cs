using Microsoft.EntityFrameworkCore;
using Sany3y.Infrastructure.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sany3y.API.Services.CountryServices
{
    public class ProvinceServices
    {
        AppDbContext _context;

        public ProvinceServices(AppDbContext context) { 
            _context = context;
        }

        public async Task<List<Province>> GetAll()
        {
            return await _context.Provinces.AsNoTracking().ToListAsync();
        }

        public async Task<Province> GetByID(int provinceId)
        {
            return await _context.Provinces.AsNoTracking().FirstOrDefaultAsync(c => c.Id == provinceId);
        }

        public async Task<List<Governorate>> GetGovernoratesByProvinceId(int provinceId)
        {
            return await _context.Governorates
                .Where(g => g.ProvinceId == provinceId)
                .ToListAsync();
        }

        public async Task<List<City>> GetCitiesByProvinceId(int provinceId)
        {
            var governorateIds = await GetGovernoratesByProvinceId(provinceId)
                .ContinueWith(t => t.Result.Select(g => g.Id).ToList());

            return await _context.Cities 
                .Where(c => governorateIds.Contains(c.GovernorateId))
                .ToListAsync();
        }
    }
}
