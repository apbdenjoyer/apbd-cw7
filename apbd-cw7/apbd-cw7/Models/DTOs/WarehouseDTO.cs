using System.ComponentModel.DataAnnotations;

namespace apbd_cw7.Models.DTOs;

public class ProductWarehouseDto
{
    [Required] public int IdProdWarehouse { get; set; }
    [Required] public ProductDto Product { get; set; } = null!;
    [Required] public OrderDto Order { get; set; } = null!;
    [Required] public WarehouseDTO Warehouse { get; set; } = null!;
}

public class WarehouseDTO
{
    [Required] public int IdWarehouse { get; set; }
    [Required] [MaxLength(200)] public String Name { get; set; }
    [Required] [MaxLength(200)] public String Address { get; set; }
}

public class ProductDto
{
    [Required] public int IdProduct { get; set; }
    [Required] [MaxLength(200)] public String Name { get; set; }
    [Required] [MaxLength(200)] public String Description { get; set; }
    [Required] public double Price { get; set; }
}

public class OrderDto
{
    public int IdOrder { get; set; }
    public ProductDto Product { get; set; } = null!;
    public int Amount { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? FulfilledAt { get; set; }
}