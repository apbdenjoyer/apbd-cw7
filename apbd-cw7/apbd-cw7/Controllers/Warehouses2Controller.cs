using System.Transactions;
using apbd_cw7.Models.DTOs;
using apbd_cw7.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace apbd_cw7.Controllers;

[Route("api/[controller]")]
[ApiController]
public class Warehouses2Controller : ControllerBase
{
    private readonly IWarehousesRepository _warehousesRepository;

    public Warehouses2Controller(IWarehousesRepository warehousesRepository)
    {
        _warehousesRepository = warehousesRepository;
    }

    [HttpPost]
    [Route("with-procedure")]
    public async Task<IActionResult> AddProductToWarehouseWithProcedure(NewProductInWarehouse newProductInWarehouse)
    {
        var price = await _warehousesRepository.GetPriceOfProduct(newProductInWarehouse.IdProduct);

        return Created(Request.Path.Value ?? "api/warehouses", newProductInWarehouse);
    }
}