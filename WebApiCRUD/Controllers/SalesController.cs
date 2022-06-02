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
    public class SalesController : ControllerBase
    {
        private readonly MyDatabaseContext _context;

        public SalesController(MyDatabaseContext context)
        {
            _context = context;
        }

        // GET: api/Sales
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Sale>>> GetSales()
        {

            return await _context.Sales.Include(s => s.SalesData).ToListAsync();
        }

        // GET: api/Sales/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Sale>> GetSale(int id)
        {
            var sale = await _context.Sales.FindAsync(id);
            _context.Entry(sale).Collection(s => s.SalesData).Load();
            if (sale == null)
            {
                return NotFound();
            }

            return sale;
        }

        // PUT: api/Sales/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutSale(int id, Sale sale)
        {
            if (id != sale.Id)
            {
                return BadRequest();
            }

            //проверим изменяемые имеющиеся продукты на дубликаты
            CheckMethods.СheckForRepeatProductsIds(new List<IProductId>(sale.SalesData));
            //проверим существует ли торговая точка, на которую хотят изменить
            CheckSalePoint(sale.SalesPointId);
            foreach (var saleData in sale.SalesData)
            {
                //метод проверяет, существует ли такой товар
                CheckMethods.CheckProductInProductsTable(saleData.ProductId, _context);
                //проверим, не содержит ли изменяемый акт продажи, SaleData с другим Id такого же ProductIt
                if (_context.SaleDatas.Where(sd => sd.Id != saleData.Id).Any(sd => sd.ProductId == saleData.ProductId))
                    throw new Exception($"Товар с ProductId={saleData.ProductId} уже есть в этом акте продажи");

                _context.Entry(saleData).State = EntityState.Modified;
            }

            
            //если изменилось Id покупателя, то у старого покупателя удалим этот заказ, а у нового добавим
            var oldSales = _context.Sales.FirstOrDefault(s => s.Id == id);
            if (oldSales.BuyerId != sale.BuyerId)
            {
                //получим объект старого покупателя
                var oldBuyer = _context.Buyers.FirstOrDefault(b => b.Id == oldSales.BuyerId);
                _context.Entry(oldBuyer).Collection(b => b.SalesIds).Load();
                //удалим у старого покупателя этот акт продажи
                oldBuyer.SalesIds.Remove(_context.SalesIds.FirstOrDefault(salesIds => salesIds.SaleId == id));

                //получим объект нового покупателя
                var newBuyer = _context.Buyers.FirstOrDefault(b => b.Id == sale.BuyerId);
                _context.Entry(newBuyer).Collection(b => b.SalesIds).Load();
                //добавим новому покупателю этот акт продажи
                newBuyer.SalesIds.Add(_context.SalesIds.FirstOrDefault(salesIds => salesIds.SaleId == id));
            }

            //обновим акт продажи
            oldSales.BuyerId = sale.BuyerId;
            oldSales.Date = sale.Date;
            oldSales.SalesData = sale.SalesData;
            oldSales.SalesPointId = sale.SalesPointId;
            oldSales.Time = sale.Time;
            
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!SaleExists(id))
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

        // POST: api/Sales
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<Sale>> PostSale(Sale sale)
        {
            if (sale.SalesData is null || sale.SalesData.Count == 0)
                throw new Exception("Данные о товарах обязательно должны присутстовать в акте продажи");
            //проверим, нет ли дубликатов товаров в SaleData
            CheckMethods.СheckForRepeatProductsIds(new List<IProductId>(sale.SalesData));
            //проверим, существют ли товары в таблице товаров из SaleData
            foreach (var saleData in sale.SalesData)
            {
                CheckMethods.CheckProductInProductsTable(saleData.ProductId, _context);
            }

            CheckSalePoint(sale.SalesPointId);

            _context.Sales.Add(sale);
            await _context.SaveChangesAsync();

            //если id покупателся не равно 0 или null, добавим в его коллекцию покупок, этот акт продажи
            if (sale.BuyerId is not null && sale.BuyerId != 0)
            {
                var buyers = _context.Buyers.FirstOrDefault(b => b.Id == sale.BuyerId);
                _context.Entry(buyers).Collection(b => b.SalesIds).Load();
                buyers.SalesIds.Add(new SaleIdClass { SaleId = sale.Id });
                await _context.SaveChangesAsync();
            }
            

            return NoContent();
        }

        // DELETE: api/Sales/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteSale(int id)
        {
            var sale = await _context.Sales.FindAsync(id);
            if (sale == null)
            {
                return NotFound();
            }

            //подгрузим данные о заказе
            _context.Entry(sale).Collection(s => s.SalesData).Load();
            //перед удалением акта продажи, удалим, все его данные из листа
            foreach (var saleData in sale.SalesData)
                _context.SaleDatas.Remove(saleData);

            _context.Sales.Remove(sale);
            await _context.SaveChangesAsync();

            return NoContent();
        }
        //метод проверяет сущестование торговой точки
        private void CheckSalePoint(int idSalePoint)
        {
            //проверим, существует ли такая торговая точка
            if (!_context.SalesPoints.Any(sp => sp.Id == idSalePoint))
                throw new Exception("Торговой точки с таким Id не существует");
        }
        private bool SaleExists(int id)
        {
            return _context.Sales.Any(e => e.Id == id);
        }
    }
}
