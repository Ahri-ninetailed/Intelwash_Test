using System;
namespace WebApiCRUD.Models
{
    public class ProvidedProduct : IProductId
    {
       
        public int Id { get; set; }
        public int ProductId { get; set; }
        public int ProductQuantity
        {
            get => productQuantitty;
            set
            {
                if (value < 0)
                    throw new Exception("Количество товара не может быть меньше нуля.");
                productQuantitty = value;
            }
        }
        private int productQuantitty;
    }
}
