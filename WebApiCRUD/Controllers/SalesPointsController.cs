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
    public class SalesPointsController : ControllerBase
    {
        private readonly MyDatabaseContext _context;

        public SalesPointsController(MyDatabaseContext context)
        {
            _context = context;
        }

        // GET: api/SalesPoints
        [HttpGet]
        public async Task<ActionResult<IEnumerable<SalesPoint>>> GetSalesPoints()
        {

            return await _context.SalesPoints.Include(sp => sp.ProvidedProducts).ToListAsync();
        }

        // GET: api/SalesPoints/5
        [HttpGet("{id}")]
        public async Task<ActionResult<SalesPoint>> GetSalesPoint(int id)
        {
            var salesPoint = await _context.SalesPoints.FindAsync(id);

            if (salesPoint == null)
            {
                return NotFound();
            }

            return salesPoint;
        }

        // PUT: api/SalesPoints/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutSalesPoint(int id, SalesPoint salesPoint)
        {
            if (id != salesPoint.Id)
            {
                return BadRequest();
            }

            _context.Entry(salesPoint).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!SalesPointExists(id))
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

        // POST: api/SalesPoints
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<SalesPoint>> PostSalesPoint(SalesPoint salesPoint)
        {
            _context.SalesPoints.Add(salesPoint);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetSalesPoint", new { id = salesPoint.Id }, salesPoint);
        }

        // DELETE: api/SalesPoints/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteSalesPoint(int id)
        {
            var salesPoint = await _context.SalesPoints.FindAsync(id);
            if (salesPoint == null)
            {
                NoSalesPointFoundException();
            }

            //подгрузим коллекцию имеющихся продуктов в точке
            _context.Entry(salesPoint).Collection(sp => sp.ProvidedProducts).Load();
            //при удалении точки, будем удалять все имеющиеся продукты
            foreach (var providedProductInSalesPoint in salesPoint.ProvidedProducts)
                _context.ProvidedProducts.Remove(providedProductInSalesPoint);

            _context.SalesPoints.Remove(salesPoint);
            await _context.SaveChangesAsync();

            return NoContent();
        }
        //метод выбрасывают ошибку
        private static void NoSalesPointFoundException()
        {
            throw new Exception("Не найдено торговой точки с таким Id");
        }

        private bool SalesPointExists(int id)
        {
            return _context.SalesPoints.Any(e => e.Id == id);
        }
    }
}
