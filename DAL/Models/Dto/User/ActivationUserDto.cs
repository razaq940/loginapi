using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Models.Dto.User
{
    public class ActivationUserDto
    {
        [EmailAddress]
        public string Email { get; set; }
        public string otp { get; set; }
    }
}
