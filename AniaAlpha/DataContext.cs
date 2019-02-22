using AniaAlpha.Models;
using Discord;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace AniaAlpha
{
    public class DataContext: DbContext
    {
        public DbSet<MALAccount> MALAccounts { get; set; }
        public DbSet<AnimeAlias> Aliases { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer(@"Server=(localdb)\mssqllocaldb;Database=AniaDB;Trusted_Connection=True;");
        }
    }
}
