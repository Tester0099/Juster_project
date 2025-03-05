using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Juster_Project.Models
{
    public class image_data
    {
        [Key]
        public int id { get; set; }
        [Required]
        public string image { get; set; }
    }
}