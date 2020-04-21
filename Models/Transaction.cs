using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System;

namespace BankAccounts.Models
{
    public class Transaction
    {
        [Key]
        public int TransactionID {get; set;}
        [Required]
        public decimal Amount {get; set;}
        public int UserId {get; set;}
        public DateTime CreatedAt {get; set;} = DateTime.Now;
        public DateTime UpdatedAt {get; set;} = DateTime.Now;
        public User Owner {get; set;}
    }
}