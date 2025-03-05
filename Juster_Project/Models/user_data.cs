using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Juster_Project.Models
{
    public class user_data
    {
        [Key] 
        public int Id { get; set; }
        public string otp { get; set; }
        public string email {  get; set; }
        public string password { get; set; }
    }
}