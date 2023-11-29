using System;
using System.Collections.Generic;

namespace Project.Models;

public partial class Order
{
    public int Id { get; set; }

    public int AccountId { get; set; }

    public int ProductId { get; set; }

    public string? Image { get; set; }

    public int Quantity { get; set; }

    public double TotalPrice { get; set; }

    public string? Address { get; set; }

    public DateTime Date { get; set; }

    public virtual Account Account { get; set; } = null!;

    public virtual Product Product { get; set; } = null!;
}
