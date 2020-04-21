using System.ComponentModel.DataAnnotations;
using System;

namespace BankAccounts.Models
{
    public class UserLogin
    {
        [Required]
        [EmailAddress]
        public string Email {get; set;}
        [Required]
        [DataType(DataType.Password)]
        public string Password {get; set;}
    }
}