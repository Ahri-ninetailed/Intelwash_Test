using System;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;


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
        public double TotalAmount
        {
            get
            {
                double answer = 0;
                foreach (var saleData in SalesData)
                    answer += saleData.ProductIdAmount;
                return answer;
            }
        }
    }
}
