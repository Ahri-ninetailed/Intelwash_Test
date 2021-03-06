using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace WebApiCRUD.Models
{
    public class SalesPoint
    {
        public int Id { get; set; }
        [Required]
        public string Name { get; set; }
        public List<ProvidedProduct> ProvidedProducts { get; set; }
    }
}
