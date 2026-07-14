using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SAV.API.Datos.Context;

namespace SAV.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ProductosApiController : ControllerBase
{
    private readonly ApiContext _context;

    public ProductosApiController(ApiContext context)
    {
        _context = context;
    }

    [HttpGet("GetProductos")]
    public async Task<IActionResult> GetProductos()
    {
        var productos = await _context.Productos.ToListAsync();
        return Ok(productos);
    }
}
