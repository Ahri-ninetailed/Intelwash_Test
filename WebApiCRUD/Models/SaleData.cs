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
        private readonly MyDatabaseContext _context;
        public SaleData()
        {

        }
        public SaleData(MyDatabaseContext context)
        {
            _context = context;
        }
        public int Id { get; set; }
        public int ProductId { get; set; }
        public int ProductQuantity { get; set; }
        public double ProductIdAmount 
        { 
            get
            {
                    var product = _context.Products.FirstOrDefault(p => p.Id == ProductId);
                    return ProductQuantity * product.Price;  
            }
        }
    }
}
