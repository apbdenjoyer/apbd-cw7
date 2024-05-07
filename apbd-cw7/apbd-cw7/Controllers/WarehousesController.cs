using System.Transactions;
using apbd_cw7.Models.DTOs;
using apbd_cw7.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace apbd_cw7.Controllers;

[Route("api/[controller]")]
[ApiController]
public class WarehousesController : ControllerBase
{
    private readonly IWarehousesRepository _warehousesRepository;

    public WarehousesController(IWarehousesRepository warehousesRepository)
    {
        _warehousesRepository = warehousesRepository;
    }

    [HttpPost]
    [Route("with-scope")]
    public async Task<IActionResult> AddProductToWarehouse(NewProductInWarehouse newProductInWarehouse)
    {
        if (!await _warehousesRepository.DoesProductExist(newProductInWarehouse.IdProduct))
        {
            return NotFound($"Product of ID {newProductInWarehouse.IdProduct} does not exist.");
        }
        var price = await _warehousesRepository.GetPriceOfProduct(newProductInWarehouse.IdProduct);

        if (!await _warehousesRepository.DoesWarehouseExist(newProductInWarehouse.IdWarehouse))
            return NotFound($"Warehouse of ID {newProductInWarehouse.IdWarehouse} does not exist.");

        if (!await _warehousesRepository.OrderAmountPositive(newProductInWarehouse.IdProduct))
            return BadRequest($"Number of products with ID {newProductInWarehouse.IdProduct} ordered <= 0");

        var idOrder = await _warehousesRepository.GetOrderOfProduct(newProductInWarehouse.IdProduct,
            newProductInWarehouse.Amount);
        if (idOrder is null)
            return NotFound(
                $"No unfulfilled orders for product with ID {newProductInWarehouse.IdProduct} in amount {newProductInWarehouse.Amount}");

        await _warehousesRepository.UpdateFullfilledAt(idOrder.Value);

        using (TransactionScope scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
        {
            await _warehousesRepository.AddProductToWarehouse(newProductInWarehouse, idOrder, price);
            
            scope.Complete();
        }


        return Created(Request.Path.Value ?? "api/warehouses", newProductInWarehouse);
    }
}