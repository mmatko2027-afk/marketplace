namespace Marketplace.Application.DTOs;

public class CategoryDto
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public int ListingsCount { get; set; }
}