using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Models.Dto.User
{
    public class UserRegisterDto
    {
        [Required(ErrorMessage = "Name can't be null")]
        public string Name { get; set; }

        [DataType(DataType.EmailAddress)]
        [Required(ErrorMessage = "Email can't be null")]
        [EmailAddress]
        public string Email { get; set; }

        [Required(ErrorMessage = "Password can't be null")]
        public string Password { get; set; }

        [Required(ErrorMessage = "Role can't be null")]
        public string Role { get; set; }
    }
}
