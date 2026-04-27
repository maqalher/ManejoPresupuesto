using System;
using System.Security.Claims;

namespace ManejoPresupuesto.Servicios;

public interface IServicioUsuarios
{
    int ObtenerUsuarioId();
}

public class ServicioUsuarios : IServicioUsuarios
{
    private readonly IHttpContextAccessor httpContextAccessor;
    public ServicioUsuarios(IHttpContextAccessor httpContextAccessor)
    {
        this.httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
    }

    public int ObtenerUsuarioId()
    {
        var httpContext = httpContextAccessor.HttpContext
            ?? throw new ApplicationException("No se pudo obtener el HttpContext");

        if (httpContext.User.Identity?.IsAuthenticated != true)
        {
            throw new ApplicationException("El usuario no está autenticado");
        }

        var idClaim = httpContext.User.Claims
            .FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier);

        if (idClaim == null || string.IsNullOrEmpty(idClaim.Value))
        {
            throw new ApplicationException("No se encontró el ID del usuario en los claims");
        }

        return int.Parse(idClaim.Value);
    }
}
