using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using PromoPilot.Models;

namespace PromoPilot.Data
{
    public class PromoPilotDbContext : DbContext
    {
        public PromoPilotDbContext(DbContextOptions<PromoPilotDbContext> options) : base(options) { }

        public DbSet<Campaign> Campaigns { get; set; }
        public DbSet<Budget> Budgets { get; set; }
        public DbSet<ExecutionStatus> ExecutionStatuses { get; set; }
        public DbSet<Engagement> Engagements { get; set; }
        public DbSet<CampaignReport> CampaignReports { get; set; }
        public DbSet<Customer> Customers { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<Sale> Sales { get; set; }

    }

}
