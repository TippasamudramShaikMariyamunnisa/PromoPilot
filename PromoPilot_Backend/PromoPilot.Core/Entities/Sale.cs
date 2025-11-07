using System;
using System.Collections.Generic;

namespace PromoPilot.Core.Entities;

public partial class Sale
{
    public int SaleId { get; set; }

    public int CustomerId { get; set; }

    public int ProductId { get; set; }

    public int CampaignId { get; set; }

    public int StoreId { get; set; }

    public int Quantity { get; set; }

    public decimal TotalAmount { get; set; }

    public DateTime SaleDate { get; set; }

    public string TransactionId { get; set; } = null!;

    public string PaymentMethod { get; set; } = null!;

    public string PaymentStatus { get; set; } = null!;

    public DateTime PaymentDate { get; set; }

    public virtual Campaign Campaign { get; set; } = null!;

    public virtual Customer Customer { get; set; } = null!;

    public virtual Product Product { get; set; } = null!;
}
