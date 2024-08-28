using AdvisorAPI.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AdvisorAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AdvisorsController : ControllerBase
    {
        private readonly AdvisorDbContext _context;

        public AdvisorsController(AdvisorDbContext context)
        {
            _context = context;
        }

        [HttpPost]
        public async Task<ActionResult<Advisor>> CreateAdvisor(Advisor advisor)
        {

            if (_context.Advisors.Any(a => a.Id == advisor.Id))
            {
                return BadRequest($"An advisor with Id {advisor.Id} already exists.");
            }

            if (string.IsNullOrWhiteSpace(advisor.Name))
            {
                return BadRequest("Name is required.");
            }

            if (advisor.SIN.Length != 9)
            {
                return BadRequest("SIN must be exactly 9 characters long.");
            }

            if (_context.Advisors.Any(a => a.SIN == advisor.SIN))
            {
                return BadRequest("SIN must be unique.");
            }

            if (advisor.Phone.Length != 8)
            {
                return BadRequest("Phone number must be exactly 8 characters long.");
            }
         


            // Generate a random health status
            var random = new Random();
            int healthStatus = random.Next(1, 6);

            advisor.HealthStatus = healthStatus switch
            {
                <= 3 => "Green",
                4 => "Yellow",
                _ => "Red",
            };

            _context.Advisors.Add(advisor);
            await _context.SaveChangesAsync();

            var maskedAdvisor = new
            {
                advisor.Id,
                advisor.Name,
                SIN = MaskSIN(advisor.SIN),
                advisor.Address,
                Phone = MaskPhone(advisor.Phone),
                advisor.HealthStatus
            };

            //  return CreatedAtAction(nameof(GetAdvisor), new { id = advisor.Id }, advisor);
            return CreatedAtAction(nameof(GetAdvisor), new { id = advisor.Id }, maskedAdvisor);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult> GetAdvisor(int id)
        {
            var advisor = await _context.Advisors.FindAsync(id);

            if (advisor == null)
            {
                return NotFound();
            }

            var maskedAdvisor = new
            {
                advisor.Id,
                advisor.Name,
                SIN = MaskSIN(advisor.SIN),
                advisor.Address,
                Phone = MaskPhone(advisor.Phone),
                advisor.HealthStatus
            };
            return Ok(maskedAdvisor);
        }
        private string MaskSIN(string sin)
            {
                if (sin.Length != 9)
                    return sin;

                return "*****" + sin.Substring(5);
            }

            private string MaskPhone(string phone)
            {
                if (phone.Length != 8)
                    return phone;

                return "****" + phone.Substring(4);
            }
      

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Advisor>>> GetAdvisors()
        {

            var advisors = await _context.Advisors.ToListAsync();
            if (advisors == null || !advisors.Any())
            {
                return Ok(new List<dynamic>());  // Return an empty list instead of null
            }
            var maskedAdvisors = advisors.Select(a => new
            {
                a.Id,
                a.Name,
                SIN = MaskedSIN(a.SIN),
                a.Address,
                Phone = MaskedPhone(a.Phone),
                a.HealthStatus
            }).ToList();
            return Ok(maskedAdvisors);
        }

        private string MaskedSIN(string sin)
        {
            if (sin.Length != 9)
                return sin;

            return "*****" + sin.Substring(5);
        }

        private string MaskedPhone(string phone)
        {
            if (phone.Length != 8)
                return phone;

            return "****" + phone.Substring(4);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateAdvisor(int id, Advisor advisor)
        {
            if (id != advisor.Id)
            {
                return BadRequest();
            }

            var existingAdvisor = await _context.Advisors.FindAsync(id);

            if (existingAdvisor != null)
            {
                _context.Entry(existingAdvisor).State = EntityState.Detached; // Detach existing entity
            }

            _context.Entry(advisor).State = EntityState.Modified;
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Advisors.Any(e => e.Id == id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

                return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAdvisor(int id)
        {
            var advisor = await _context.Advisors.FindAsync(id);
            if (advisor == null)
            {
                return NotFound();
            }

            _context.Advisors.Remove(advisor);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
