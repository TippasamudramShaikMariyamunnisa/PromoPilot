using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using PromoPilot.Core.Entities;
using PromoPilot.Infrastructure.Data;

namespace PromoPilot.Tests.Helpers
{
    public static class TestDbContextFactory
    {
        public static PromoPilotDbContext Create()
        {
            var options = new DbContextOptionsBuilder<PromoPilotDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .EnableSensitiveDataLogging()
                .Options;

            var context = new PromoPilotDbContext(options);

            // ✅ Seed Campaigns with required fields
            context.Campaigns.AddRange(new List<Campaign>
            {
                new Campaign
                {
                    CampaignId = 1,
                    Name = "Campaign 1",
                    StartDate = DateTime.Today,
                    EndDate = DateTime.Today.AddDays(5),
                    StoreList = "Store1,Store2",
                    TargetProducts = "Product1,Product2"
                },
                new Campaign
                {
                    CampaignId = 2,
                    Name = "Campaign 2",
                    StartDate = DateTime.Today.AddDays(1),
                    EndDate = DateTime.Today.AddDays(6),
                    StoreList = "Store3,Store4",
                    TargetProducts = "Product3,Product4"
                }
            });

            // ✅ Seed Budgets
            context.Budgets.AddRange(new List<Budget>
            {
                new Budget
                {
                    BudgetId = 1,
                    CampaignId = 1,
                    StoreId = 101,
                    AllocatedAmount = 1000,
                    SpentAmount = 500
                },
                new Budget
                {
                    BudgetId = 2,
                    CampaignId = 2,
                    StoreId = 102,
                    AllocatedAmount = 2000,
                    SpentAmount = 1500
                }
            });
            // Seed CampaignReports
            context.CampaignReports.AddRange(new List<CampaignReport>
            {
                new CampaignReport
                {
                    ReportId = 1,
                    CampaignId = 1,
                    Roi = 100,
                    Reach = 200,
                    ConversionRate = 50,
                    GeneratedDate = DateTime.UtcNow,
                    Campaign = new Campaign
                    {
                        CampaignId = 1,
                        Name = "Campaign 1",
                        StoreList = "North,South",
                        TargetProducts = "Product1,Product2",
                        StartDate = DateTime.Today,
                        EndDate = DateTime.Today.AddDays(5)
                    }
                }
            });


            // Seed Customer data
            context.Customers.AddRange(
                new Customer { CustomerId = 1, Name = "Alice", Email = "alice@example.com" },
                new Customer { CustomerId = 2, Name = "Bob", Email = "bob@example.com" },
                new Customer { CustomerId = 3, Name = "Charlie", Email = "charlie@example.com" }
            );

            // Seed Engagement data
            context.Engagements.AddRange(new List<Engagement>
            {
                new Engagement
                {
                    EngagementId = 1,
                    CampaignId = 1,
                    CustomerId = 1,
                    RedemptionCount = 2,
                    PurchaseValue = 100
                },
                new Engagement
                {
                    EngagementId = 2,
                    CampaignId = 1,
                    CustomerId = 2,
                    RedemptionCount = 3,
                    PurchaseValue = 150
                },
                new Engagement
                {
                    EngagementId = 3,
                    CampaignId = 2,
                    CustomerId = 3,
                    RedemptionCount = 1,
                    PurchaseValue = 200
                }
            });

            // Seed ExecutionStatus data
            context.ExecutionStatuses.AddRange(new List<ExecutionStatus>
            {
                new ExecutionStatus
                {
                    StatusId = 1,
                    CampaignId = 1,
                    StoreId = 101,
                    Status = "Pending",
                    Feedback = "Waiting for approval"
                },
                new ExecutionStatus
                {
                    StatusId = 2,
                    CampaignId = 1,
                    StoreId = 102,
                    Status = "InProgress",
                    Feedback = "Execution started"
                },
                new ExecutionStatus
                {
                    StatusId = 3,
                    CampaignId = 2,
                    StoreId = 103,
                    Status = "Completed",
                    Feedback = "Execution completed successfully"
                }
            });

            // Seed Product data
            context.Products.AddRange(new List<Product>
            {
                new Product { ProductId = 1, Name = "Product A", Category = "Category A", Price = 100 },
                new Product { ProductId = 2, Name = "Product B", Category = "Category B", Price = 200 },
                new Product { ProductId = 3, Name = "Product C", Category = "Category C", Price = 300 }
            });

            // Seed Sales data
            context.Sales.AddRange(new List<Sale>
            {
                new Sale
                {
                    SaleId = 1,
                    CustomerId = 1,
                    ProductId = 1,
                    CampaignId = 1,
                    StoreId = 101,
                    Quantity = 2,
                    TotalAmount = 200,
                    SaleDate = DateTime.Today,
                    TransactionId = "TX1",
                    PaymentMethod = "Card",
                    PaymentStatus = "Paid",
                    PaymentDate = DateTime.Today
                },
                new Sale
                {
                    SaleId = 2,
                    CustomerId = 2,
                    ProductId = 2,
                    CampaignId = 1,
                    StoreId = 102,
                    Quantity = 1,
                    TotalAmount = 100,
                    SaleDate = DateTime.Today,
                    TransactionId = "TX2",
                    PaymentMethod = "UPI",
                    PaymentStatus = "Paid",
                    PaymentDate = DateTime.Today
                },
                new Sale
                {
                    SaleId = 3,
                    CustomerId = 3,
                    ProductId = 3,
                    CampaignId = 2,
                    StoreId = 103,
                    Quantity = 3,
                    TotalAmount = 300,
                    SaleDate = DateTime.Today,
                    TransactionId = "TX3",
                    PaymentMethod = "Cash",
                    PaymentStatus = "Pending",
                    PaymentDate = DateTime.Today
                }
            });
            context.SaveChanges();

            return context;
        }
    }
}