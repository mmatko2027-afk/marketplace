namespace Marketplace.Application.DTOs;

public class ListingDto
{
    public int Id { get; set; }

    public string Title { get; set; } = "";

    public string Description { get; set; } = "";

    public decimal Price { get; set; }

    public int CategoryId { get; set; }

    public string CategoryName { get; set; } = "";

    public string Status { get; set; } = "";
}