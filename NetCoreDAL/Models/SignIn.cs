using System;
using System.ComponentModel.DataAnnotations;

namespace NetCoreDAL.Models
{
    public class SignIn
    {
        [Required]
        public string userName { get; set; }
        [Required, DataType(DataType.Password)]
        public string password { get; set; }
    }
}
