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
                NoSalesPointFoundException();
            }

            _context.Entry(salesPoint).Collection(sp => sp.ProvidedProducts).Load();

            return salesPoint;
        }

        // PUT: api/SalesPoints/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutSalesPoint(int id, SalesPoint salesPoint)
        {
            if (id != salesPoint.Id)
            {
                throw new Exception("Id не совпадают. Нельзя изменить Id торговой точки.");
            }
            //проверим изменяемые имеющиеся продукты на дубликаты
            СheckForRepeatProvidedProductsIds(salesPoint);

            foreach (var providedProduct in salesPoint.ProvidedProducts)
            {
                //пользователь не может менять Id имеющегося товара на тот, который уже есть в другой точке
                CheckProvidedProductInOtherSalesPoint(id, providedProduct.Id, "Нельзя изменить Id имеющегося товара, на Id товара из другой точки");
                //метод проверяет, существует ли такой товар
                CheckMethods.CheckProductInProductsTable(providedProduct.ProductId, _context);

                _context.Entry(providedProduct).State = EntityState.Modified;
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
                    NoSalesPointFoundException();
                }
                else
                {
                    throw;
                }
            }

            return CreatedAtAction("GetSalesPoint", new { id = salesPoint.Id }, salesPoint);
        }

        // POST: api/SalesPoints
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<SalesPoint>> PostSalesPoint(SalesPoint salesPoint)
        {
            //проверим на ProductId на дубликаты
            СheckForRepeatProvidedProductsIds(salesPoint);

            foreach (var providedProduct in salesPoint.ProvidedProducts)
            {
                //проверим, существуют ли продукты, которые мы хотим добавить
                CheckMethods.CheckProductInProductsTable(providedProduct.ProductId, _context);
            }

            _context.SalesPoints.Add(salesPoint);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetSalesPoint", new { id = salesPoint.Id }, salesPoint);
        }

        

        // POST: /api/SalesPoints/ProvidedProductInSalesPoint/{salesPointId}
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost("AddProvidedProductInSalesPoint/{salesPointId}")]
        public async Task<ActionResult<SalesPoint>> PostAddProvidedProductInSalesPoint(int salesPointId ,ProvidedProduct providedProduct)
        {
            //получим объект торговой точки, в которую будем добавлять имеющийся товар
            var salesPoint = _context.SalesPoints.FirstOrDefault(sp => sp.Id == salesPointId);
            if (salesPoint is null)
                NoSalesPointFoundException();

            //подрузим имеющиеся товары
            _context.Entry(salesPoint).Collection(sp => sp.ProvidedProducts).Load();

            //пользователь не может добавить товар, который уже есть в точке
            CheckProductExistInSalesPoint(salesPoint, providedProduct.ProductId);

            //метод проверяет наличие товара в таблице товаров
            CheckMethods.CheckProductInProductsTable(providedProduct.ProductId, _context);

            //метод проверяет, есть ли такое Id имеющегося товара, в других торговых точках, если есть, то вылетит ошибка
            CheckProvidedProductInOtherSalesPoint(salesPointId, providedProduct.Id, "Нельзя добавить имеющийся товар с таким же Id, как в другой точке");

            salesPoint.ProvidedProducts.Add(providedProduct);
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
        //метод проверяет, есть ли в имеющихся продуктах дубликаты и если есть выкидывает ошибку
        private static void СheckForRepeatProvidedProductsIds(SalesPoint salesPoint)
        {
            //получим лист, который содержит все Id добавляемых продуктов
            var salesPointProductsIdList = (from providedProduct in salesPoint.ProvidedProducts
                                            select providedProduct.ProductId).ToList();
            //если лист содержит дубликаты, то такую торговую нельзя добавлять
            if (salesPointProductsIdList.Count != salesPointProductsIdList.Distinct().Count())
                throw new Exception("Точка содержит повторяющиеся Id продуктов");
        }

        //метод проверяет, есть ли продукт с таким Id в конкретной торговой точке и если есть выкидывает ошибку
        private static void CheckProductExistInSalesPoint(SalesPoint salesPoint, int providedProductProductId)
        {
            if (salesPoint.ProvidedProducts.Any(pp => pp.ProductId == providedProductProductId))
                throw new Exception("Продукт с таким Id уже есть");
        }

        //метод проверяет есть ли имеющийся товар с таким Id в других торговых точках
        private void CheckProvidedProductInOtherSalesPoint(int salesPointId, int providedProductId, string errorMsg)
        {
            
            if (_context.SalesPoints.Where(sp => sp.Id != salesPointId).Any(sp => sp.ProvidedProducts.Any(pp => pp.Id == providedProductId)))
                throw new Exception(errorMsg);
        }

        //метод выбрасывает ошибку, которая сообщает, что торговой точки не существует
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
