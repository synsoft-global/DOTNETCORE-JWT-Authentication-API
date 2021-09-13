using System;
using System.ComponentModel.DataAnnotations;

namespace NetCoreDAL.Models
{
    public class SignUp
    {
        [Required]
        public string name { get; set; }
        [Required]
        public string mobileNo { get; set; }
        [Required, DataType(DataType.Password)]
        public string password { get; set; }
    }
}
