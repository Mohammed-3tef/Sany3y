using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Sany3y.Infrastructure.Models;
using Sany3y.API.Services.CountryServices;
using System.Threading.Tasks;

namespace Sany3y.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CountryServicesController : ControllerBase
    {
        private readonly ProvinceServices _provinceServices;
        private readonly GovernorateServices _governorateServices;
        private readonly CityServices _cityServices;

        public CountryServicesController(ProvinceServices provinceServices, GovernorateServices governorateServices, CityServices cityServices)
        {
            _provinceServices = provinceServices;
            _governorateServices = governorateServices;
            _cityServices = cityServices;
        }


        [HttpGet("GetAllProvinces")]
        public async Task<IActionResult> GetAllProvinces()
        {
            var provinces = await _provinceServices.GetAll();
            return Ok(provinces);
        }

        [HttpGet("GetAllGovernorates")]
        public async Task<IActionResult> GetAllGovernorates(int provinceId)
        {
            var governorates = await _governorateServices.GetAll();
            return Ok(governorates);
        }

        [HttpGet("GetAllCities")]
        public async Task<IActionResult> GetAllCities()
        {
            var cities = await _cityServices.GetAll();
            return Ok(cities);
        }

        [HttpGet("GetGovernoratesByProvinceId/{provinceId}")]
        public async Task<IActionResult> GetGovernoratesByProvinceId(int provinceId)
        {
            var governorates = await _provinceServices.GetGovernoratesByProvinceId(provinceId);
            return Ok(governorates);
        }

        [HttpGet("GetCitiesByProvinceId/{provinceId}")]
        public async Task<IActionResult> GetCitiesByProvinceId(int provinceId)
        {
            var cities = await _provinceServices.GetCitiesByProvinceId(provinceId);
            return Ok(cities);
        }

        [HttpGet("GetCitiesByGovernorateId/{governorateId}")]
        public async Task<IActionResult> GetCitiesByGovernorateId(int governorateId)
        {
            var cities = await _governorateServices.GetCitiesByGovernorateId(governorateId);
            return Ok(cities);
        }

        [HttpGet("GetProvinceById/{provinceId}")]
        public async Task<IActionResult> GetProvinceById(int provinceId)
        {
            var province = await _provinceServices.GetByID(provinceId);
            return Ok(province);
        }

        [HttpGet("GetGovernorateById/{governorateId}")]
        public async Task<IActionResult> GetGovernorateById(int governorateId)
        {
            var governorate = await _governorateServices.GetByID(governorateId);
            return Ok(governorate);
        }

        [HttpGet("GetCityById/{cityId}")]
        public async Task<IActionResult> GetCityById(int cityId)
        {
            var city = await _cityServices.GetByID(cityId);
            return Ok(city);
        }
    }
}
