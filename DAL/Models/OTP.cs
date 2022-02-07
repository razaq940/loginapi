using System;
using System.Collections.Generic;

#nullable disable

namespace DAL.Models
{
    public partial class OTP
    {
        public long Id { get; set; }
        public string Otp { get; set; }
        public long FkUser { get; set; }
    }
}