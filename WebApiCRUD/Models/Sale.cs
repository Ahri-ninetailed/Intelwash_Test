using System;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

namespace WebApiCRUD.Models
{
    public class Sale
    {
        public int Id { get; set; }
        [DataType(DataType.Date)]
        public DateTime Date { get; set; }
        [DataType(DataType.Time)]
        public DateTime Time { get; set; }
        public int SalesPointId { get; set; }
        public int? BuyerId { get; set; }
        [Required]
        public List<SaleData> SalesData { get; set; }
        public class SaleData
        {
            public int ProductId { get; set; }
            public int ProductQuantitty { get; set; }
            public double ProductIdAmount { get; set; }
        }
        public double TotalAmount { get; set; }
    }
}
