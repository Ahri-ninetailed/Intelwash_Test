using System;
using System.Linq;
using WebApiCRUD.Data;
using System.Collections.Generic;

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
        ////метод проверяет, есть ли дубликаты ProductId и если есть выкидывает ошибку
        public static void СheckForRepeatProductsIds(List<IProductId> productIds)
        {
            //получим лист, который содержит все Id добавляемых продуктов
            var salesPointProductsIdList = (from providedProduct in productIds
                                            select providedProduct.ProductId).ToList();
            //если лист содержит дубликаты, то такую торговую нельзя добавлять
            if (salesPointProductsIdList.Count != salesPointProductsIdList.Distinct().Count())
                throw new Exception("Точка содержит повторяющиеся Id продуктов");
        }
    }
}
