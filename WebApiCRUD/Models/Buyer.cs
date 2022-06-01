using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebApiCRUD.Models
{
    public class Buyer
    {
        public int Id { get; set; }
        [Required]
        public string Name { get; set; }
        public List<SaleIdClass> SalesIds { get; set; }
    }
    public class SaleIdClass
    {
        public int Id { get; set; }
        public int SaleId { get; set; }
    }
}
