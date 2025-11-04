using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Sany3y.Infrastructure.Models;
using Sany3y.Infrastructure.Repositories;

namespace Sany3y.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PaymentMethodController : ControllerBase
    {
        IRepository<PaymentMethod> _paymentMethodRepository;

        public PaymentMethodController(IRepository<PaymentMethod> repository)
        {
            _paymentMethodRepository = repository;
        }

        [HttpGet("GetAll")]
        public async Task<IActionResult> GetAll()
        {
            var paymentMethods = await _paymentMethodRepository.GetAll();
            return Ok(paymentMethods);
        }

        [HttpGet("GetByID/{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var paymentMethod = await _paymentMethodRepository.GetById(id);
            if (paymentMethod == null)
                return NotFound();
            return Ok(paymentMethod);
        }

        [HttpPost("Create")]
        public async Task<IActionResult> Create(PaymentMethod paymentMethod)
        {
            await _paymentMethodRepository.Add(paymentMethod);
            return CreatedAtAction(nameof(GetById), new { id = paymentMethod.Id }, paymentMethod);
        }

        [HttpPut("Update/{id}")]
        public async Task<IActionResult> Update(int id, PaymentMethod paymentMethod)
        {
            if (id != paymentMethod.Id)
                return BadRequest();

            var existingPaymentMethod = await _paymentMethodRepository.GetById(id);
            if (existingPaymentMethod == null)
                return NotFound();

            await _paymentMethodRepository.Update(paymentMethod);
            return NoContent();
        }

        [HttpDelete("Delete/{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var paymentMethod = await _paymentMethodRepository.GetById(id);
            if (paymentMethod == null)
                return NotFound();

            await _paymentMethodRepository.Delete(paymentMethod);
            return NoContent();
        }
    }
}
