using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SAV.API.Datos.Context;

namespace SAV.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ClientesApiController : ControllerBase
{
    private readonly ApiContext _context;

    public ClientesApiController(ApiContext context)
    {
        _context = context;
    }

    [HttpGet("GetClientes")]
    public async Task<IActionResult> GetClientes()
    {
        var clientes = await _context.Clientes.ToListAsync();
        return Ok(clientes);
    }
}
