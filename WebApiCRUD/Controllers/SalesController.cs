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

            //проверим изменяемые продукты на дубликаты
            CheckMethods.СheckForRepeatProductsIds(new List<IProductId>(sale.SalesData));
            //проверим существует ли торговая точка, на которую хотят изменить
            CheckSalePoint(sale.SalesPointId);
            foreach (var saleData in sale.SalesData)
            {
                //метод проверяет, существует ли такой товар
                CheckMethods.CheckProductInProductsTable(saleData.ProductId, _context);

                _context.Entry(saleData).State = EntityState.Modified;
            }

            
            //если изменилось Id покупателя, то у старого покупателя удалим этот заказ, а у нового добавим
            var oldSales = _context.Sales.FirstOrDefault(s => s.Id == id);
            _context.Entry(oldSales).Collection(s => s.SalesData).Load();

            if (oldSales.BuyerId != sale.BuyerId)
            {
                //если старый покупатель есть, то удалим у него этот акт продажи
                if (oldSales.BuyerId is not null)
                {
                    var oldBuyer = _context.Buyers.FirstOrDefault(b => b.Id == oldSales.BuyerId);
                    _context.Entry(oldBuyer).Collection(b => b.SalesIds).Load();
                    oldBuyer.SalesIds.Remove(_context.SalesIds.FirstOrDefault(si => si.SaleId == id));
                }

                var newBuyer = _context.Buyers.FirstOrDefault(b => b.Id == sale.BuyerId);
                _context.Entry(newBuyer).Collection(b => b.SalesIds).Load();
                var addedSaleId = _context.SalesIds.FirstOrDefault(s => s.SaleId == id);
                if (addedSaleId is null)
                    newBuyer.SalesIds.Add(new SaleIdClass { SaleId = id });
                else
                    newBuyer.SalesIds.Add(addedSaleId);
            }

            //обновим акт продажи
            oldSales.BuyerId = sale.BuyerId;
            oldSales.Date = sale.Date;
            oldSales.SalesData = sale.SalesData;
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

        //через метод будет осуществляться продажа
        // POST: api/Sales
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost("Sell")]
        public async Task<ActionResult<Sale>> PostSell(SellInfo sellInfo)
        {
            //проверим на наличие такой торговой точки
            CheckSalePoint(sellInfo.SalePointId);
            //проверим наличие товара в этой точке
            var salePoint = _context.SalesPoints.FirstOrDefault(sp => sp.Id == sellInfo.SalePointId);
            _context.Entry(salePoint).Collection(sp => sp.ProvidedProducts).Load();
            var providedProductsInThisPoint = salePoint.ProvidedProducts.FirstOrDefault(pp => pp.ProductId == sellInfo.ProductId);
            if (providedProductsInThisPoint.ProductQuantity < sellInfo.ProductQuantity)
                throw new Exception($"В точке товара всего {providedProductsInThisPoint.ProductQuantity}шт.");
            //изменим количество продуктов в точке и добавим объект Sale в бд
            if (sellInfo.BuyerId == 0)
                sellInfo.BuyerId = null;
            await ChangeProductsCountInPointAndAddSaleObjectInDb(sellInfo, providedProductsInThisPoint, sellInfo.BuyerId is null);
            await _context.SaveChangesAsync();
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
        //метод изменяет количество товара в точке и формирует, и записывает Sale в бд. Если есть пользователь, который покупает, то добавляем SaleId в его лист
        private async Task ChangeProductsCountInPointAndAddSaleObjectInDb(SellInfo sellInfo, ProvidedProduct providedProductsInThisPoint, bool IsBuyerIdNull)
        {
            //изменим кол-во товара в точке
            providedProductsInThisPoint.ProductQuantity -= sellInfo.ProductQuantity;
            //сформируем объект Sale
            if (!IsBuyerIdNull)
            {
                var sale = new Sale
                {
                    Date = DateTime.Now,
                    Time = DateTime.Now,
                    SalesPointId = sellInfo.SalePointId,
                    BuyerId = sellInfo.BuyerId,
                    SalesData = new List<SaleData> { new SaleData { ProductId = sellInfo.ProductId, ProductQuantity = sellInfo.ProductQuantity } }
                };
                _context.Sales.Add(sale);
                await _context.SaveChangesAsync();
                //добавим объект Sale в лист пользоваттеля
                var buyer = _context.Buyers.FirstOrDefault(b => b.Id == sellInfo.BuyerId);
                _context.Entry(buyer).Collection(b => b.SalesIds).Load();
                if (buyer.SalesIds is not null)
                    buyer.SalesIds.Add(new SaleIdClass { SaleId = sale.Id });
                if (buyer.SalesIds is null)
                    buyer.SalesIds = new List<SaleIdClass> { new SaleIdClass { SaleId = sale.Id } };
            }
            else
            {
                var sale = new Sale
                {
                    Date = DateTime.Now,
                    Time = DateTime.Now,
                    SalesPointId = sellInfo.SalePointId,
                    BuyerId = null,
                    SalesData = new List<SaleData> { new SaleData { ProductId = sellInfo.ProductId, ProductQuantity = sellInfo.ProductQuantity } }
                };
                _context.Sales.Add(sale);
                await _context.SaveChangesAsync();
            }
        }
    }

    //класс содержит информацию о продаже
    public class SellInfo
    {
        public int SalePointId { get; set; }
        public int ProductQuantity { get; set; }
        public int? BuyerId { get; set; }
        public int ProductId { get; set; }
    }
}
