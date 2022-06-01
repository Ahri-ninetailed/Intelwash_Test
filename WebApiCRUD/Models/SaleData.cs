using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApiCRUD.Data;
using WebApiCRUD.Models;

namespace WebApiCRUD.Models
{
    public class SaleData
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public int ProductQuantity { get; set; }
        public double ProductIdAmount 
        { 
            get
            {

                using (var context = new MyDatabaseContext(new DbContextOptions<MyDatabaseContext>()))
                {
                    var product = context.Products.FirstOrDefault(p => p.Id == ProductId);
                    return ProductQuantity * product.Price;
                }
            }
        }
    }
}
