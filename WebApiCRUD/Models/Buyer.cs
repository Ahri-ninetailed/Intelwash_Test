using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

namespace WebApiCRUD.Models
{
    public class Buyer
    {
        public int Id { get; set; }
        [Required]
        public string Name { get; set; }
        public List<int> SalesIds { get; set; }
    }
}
