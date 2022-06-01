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
    public class BuyersController : ControllerBase
    {
        private readonly MyDatabaseContext _context;

        public BuyersController(MyDatabaseContext context)
        {
            _context = context;
        }

        // GET: api/Buyers
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Buyer>>> GetBuyers()
        {
            return await _context.Buyers.Include(b => b.SalesIds).ToListAsync();
        }

        // GET: api/Buyers/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Buyer>> GetBuyer(int id)
        {
            var buyer = await _context.Buyers.FindAsync(id);

            if (buyer == null)
            {
                return NotFound();
            }
            _context.Entry(buyer).Collection(b => b.SalesIds).Load();
            return buyer;
        }

        // PUT: api/Buyers/{id}
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutBuyer(int id, string newName)
        {
            var buyer = _context.Buyers.FirstOrDefault(b => b.Id == id);
            if (id != buyer.Id)
            {
                return BadRequest();
            }
            buyer.Name = newName;
            

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!BuyerExists(id))
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

        // POST: api/Buyers
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<Buyer>> PostBuyer(Buyer buyer)
        {
            //если лист идентификаторов продаж не пуст, проверим, существуют ли такие акты продаж, если нет, то сообщим об этом
            if (buyer.SalesIds is not null || buyer.SalesIds.Count != 0)
            {
                foreach (var SaleId in buyer.SalesIds)
                {
                    if (!_context.Sales.Any(s => s.Id == SaleId.SaleId))
                        throw new Exception("Нет акта продажи с таким Id");
                    //если акт продажи существует, то проверим, не совершил ли его другой пользователь
                    else
                    {
                        //при добавлении покупателя, в его список покупок, можно добавить только те покупки, у которых покупатель не был зарегестрирован
                        var thisSale = _context.Sales.FirstOrDefault(s => s.Id == SaleId.SaleId);
                        if (thisSale.BuyerId is not null && thisSale.BuyerId != 0)
                            throw new Exception("Эту сделку совершил другой пользовтель");
                    }
                        
                }
            }

            _context.Buyers.Add(buyer);

            await _context.SaveChangesAsync();
            return CreatedAtAction("GetBuyer", new { id = buyer.Id }, buyer);
        }

        // DELETE: api/Buyers/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteBuyer(int id)
        {
            var buyer = await _context.Buyers.FindAsync(id);
            if (buyer == null)
            {
                return NotFound();
            }

            //подгрузим лист Id покупок покупателя
            _context.Entry(buyer).Collection(b => b.SalesIds).Load();
            //удалим из таблицы SalesIds соотвествующие строки, которые есть в листе удаляемого пользователя
            if (buyer.SalesIds is not null)
            {
                foreach (var saleId in buyer.SalesIds)
                    _context.SalesIds.Remove(saleId);
            }    
            _context.Buyers.Remove(buyer);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool BuyerExists(int id)
        {
            return _context.Buyers.Any(e => e.Id == id);
        }
    }
}
