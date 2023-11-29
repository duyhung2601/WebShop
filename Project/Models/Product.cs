using System;
using System.Collections.Generic;

namespace Project.Models;

public partial class Product
{
    public int Id { get; set; }

    public string ProductName { get; set; } = null!;

    public int AccountId { get; set; }

    public int CategoryId { get; set; }

    public string? Image { get; set; }

    public double Price { get; set; }

    public int Quantity { get; set; }

    public bool Status { get; set; }

    public virtual Account Account { get; set; } = null!;

    public virtual Category Category { get; set; } = null!;

    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();
}
