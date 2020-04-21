using Microsoft.EntityFrameworkCore;

namespace BankAccounts.Models
{
    public class BankContext : DbContext
    {
        public BankContext(DbContextOptions options) : base(options) { }

        public DbSet<User> Users {get; set;}
        public DbSet<Transaction> Transactions {get; set;} 
    }
}