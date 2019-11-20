using Microsoft.EntityFrameworkCore;
using TowerScaffolding.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TowerScaffolding.Data
{
    public class TowerScaffoldingContext : DbContext
    {
        public TowerScaffoldingContext(DbContextOptions<TowerScaffoldingContext> options) : base(options)
        {
        }
        public DbSet<Customer> Customers { get; set; }
        public DbSet<Project> Projects { get; set; }
        public DbSet<Models.Task> Tasks { get; set; }
        public DbSet<DayWork> DayWorks { get; set; }
        public DbSet<LeadingHand> LeadingHands { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Customer>().ToTable("Customer");
            modelBuilder.Entity<Project>().ToTable("Project");
            modelBuilder.Entity<Models.Task>().ToTable("Task");
            modelBuilder.Entity<DayWork>().ToTable("DayWork");
            modelBuilder.Entity<LeadingHand>().ToTable("LeadingHand");
        }

    }
}
