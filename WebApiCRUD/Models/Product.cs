using System;
using System.ComponentModel.DataAnnotations;

namespace WebApiCRUD.Models
{
    public class Product
    {
        public int Id { get; set; }
        [Required]
        public string Name { get; set; }
        public double Price
        {
            get => price;
            set
            {
                if (value < 0)
                    throw new Exception("Цена не может быть отрицательной");
                price = value;
            }
        }
        private double price;
    }
}
