using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Nextag.Api.Services.TiposProveedor.Interface;

namespace Nextag.Api.Controllers;

[ApiController]
[Route("api/tipos-proveedor")]
[Authorize(Roles = "ADMINISTRADOR,ADMIN_EMPRESA,OPERADOR,CONSULTA")]
public class TiposProveedorController : ControllerBase
{
    private readonly ITipoProveedorService service;
    public TiposProveedorController(ITipoProveedorService service) => this.service = service;

    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken ct) =>
        Ok(await service.GetAllAsync(ct));
}
