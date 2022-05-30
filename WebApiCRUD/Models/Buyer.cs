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
        [Column("SalesIds")]
        public List<Sale> Sales { get; set; }
    }
}
