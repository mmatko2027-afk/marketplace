using System.Reflection;

namespace Marketplace.Domain.Entities;

public class Category
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public List<Listing> Listings { get; set; } = new();
}