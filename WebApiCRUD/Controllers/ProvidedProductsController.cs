using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApiCRUD.Data;
using WebApiCRUD.Models;

namespace WebApiCRUD.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProvidedProductsController : ControllerBase
    {
        private readonly MyDatabaseContext _context;

        public ProvidedProductsController(MyDatabaseContext context)
        {
            _context = context;
        }

        // GET: api/ProvidedProducts
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ProvidedProduct>>> GetProvidedProducts()
        {
            return await _context.ProvidedProducts.ToListAsync();
        }

        // GET: api/ProvidedProducts/5
        [HttpGet("{id}")]
        public async Task<ActionResult<ProvidedProduct>> GetProvidedProduct(int id)
        {
            var providedProduct = await _context.ProvidedProducts.FindAsync(id);

            if (providedProduct == null)
            {
                return NotFound();
            }

            return providedProduct;
        }

        // PUT: api/ProvidedProducts/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutProvidedProduct(int id, ProvidedProduct providedProduct)
        {
            if (id != providedProduct.Id)
            {
                return BadRequest();
            }

            _context.Entry(providedProduct).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ProvidedProductExists(id))
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

        // POST: api/ProvidedProducts
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<ProvidedProduct>> PostProvidedProduct(ProvidedProduct providedProduct)
        {
            _context.ProvidedProducts.Add(providedProduct);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetProvidedProduct", new { id = providedProduct.Id }, providedProduct);
        }

        // DELETE: api/ProvidedProducts/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProvidedProduct(int id)
        {
            var providedProduct = await _context.ProvidedProducts.FindAsync(id);
            if (providedProduct == null)
            {
                return NotFound();
            }

            _context.ProvidedProducts.Remove(providedProduct);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool ProvidedProductExists(int id)
        {
            return _context.ProvidedProducts.Any(e => e.Id == id);
        }
    }
}
