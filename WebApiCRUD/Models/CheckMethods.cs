using System;
using System.Linq;
using WebApiCRUD.Data;

namespace WebApiCRUD.Models
{
    public static class CheckMethods
    {
        //метод проверяет наличие товара в таблице товаров
        public static void CheckProductInProductsTable(int productId, MyDatabaseContext _context)
        {
            if (!_context.Products.Any(p => p.Id == productId))
                throw new Exception($"Не найдено продукта с Id={productId}");
        }
    }
}
