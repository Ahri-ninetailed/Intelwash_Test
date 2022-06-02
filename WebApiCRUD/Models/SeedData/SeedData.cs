using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using WebApiCRUD.Data;
using System.Collections.Generic;

namespace WebApiCRUD.Models.SeedData
{
    public static class SeedData
    {
        public static void Initialize(IServiceProvider serviceProvider)
        {
            using (var context = new MyDatabaseContext(serviceProvider.GetRequiredService<DbContextOptions<MyDatabaseContext>>()))
            {
                if (context.Products.Any())
                {
                    return;   // DB has been seeded
                }

                context.Products.AddRange(
                    new Product { Name = "Молоко", Price = 60, Id = 1 },
                    new Product { Name = "Чечил балыкоый", Price = 120, Id = 2 },
                    new Product { Name = "Манго", Price = 180, Id = 3 }
                );

                context.SalesPoints.AddRange(
                    new SalesPoint
                    {
                        Name = "OZON",
                        ProvidedProducts = new List<ProvidedProduct>
                        {
                                new ProvidedProduct { ProductId = 2, ProductQuantity = 2 },
                                new ProvidedProduct { ProductId = 3, ProductQuantity = 1 }
                        }
                    },
                    new SalesPoint
                    {
                        Name = "Пятерочка",
                        ProvidedProducts = new List<ProvidedProduct>
                        {
                                new ProvidedProduct { ProductId = 1, ProductQuantity = 5 },
                                new ProvidedProduct { ProductId = 3, ProductQuantity = 2 },
                                new ProvidedProduct { ProductId = 2, ProductQuantity = 1 }
                        }
                    }
                );

                context.Buyers.AddRange
                    (
                        new Buyer { Name = "Вадим", SalesIds = new List<SaleIdClass> { new SaleIdClass { SaleId = 1 }, new SaleIdClass { SaleId = 2 } } },
                        new Buyer { Name = "Алексей", SalesIds = null }
                    );

                context.Sales.AddRange
                    (
                        new Sale 
                        { 
                            BuyerId = 1, 
                            Date = DateTime.Now, 
                            Time = DateTime.Now, 
                            SalesPointId = 1, 
                            SalesData = new List<SaleData> 
                            { 
                                new SaleData(context) { ProductId = 1, ProductQuantity = 2 },
                                new SaleData(context) {ProductId = 3, ProductQuantity = 10 }
                            } 
                        },
                        new Sale
                        {
                            BuyerId = 1,
                            Date = DateTime.Now,
                            Time = DateTime.Now,
                            SalesPointId = 2,
                            SalesData = new List<SaleData>
                            {
                                new SaleData(context) { ProductId = 2, ProductQuantity = 1 },
                                new SaleData(context) {ProductId = 1, ProductQuantity = 3 }
                            }
                        }
                    );

                context.SaveChanges();
            }
        }
    }
}
