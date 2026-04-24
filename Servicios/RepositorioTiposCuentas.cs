using System;
using Dapper;
using ManejoPresupuesto.Models;
using Microsoft.Data.SqlClient;

namespace ManejoPresupuesto.Servicios;

public interface IRepositorioTiposCuentas
{
    Task Crear(TipoCuenta tipoCuenta);
    Task<bool> Existe(string nombre, int usuarioId);
    Task<IEnumerable<TipoCuenta>> Obtener(int usuarioId);
    Task Actualizar(TipoCuenta tipoCuenta);
    Task<TipoCuenta> ObtenerPorId(int id, int usuarioId);
    Task Borrar(int id);
    Task Ordenar(IEnumerable<TipoCuenta> tipoCuentasOrdenados);
}

public class RepositorioTiposCuentas: IRepositorioTiposCuentas
{
    private readonly string connectionString;

    public RepositorioTiposCuentas(IConfiguration configuration)
    {
        connectionString = configuration.GetConnectionString("DefaultConnection") ?? throw new Exception("La cadena de conexión 'DefaultConnection' no fue encontrada.");
    }

    public async Task Crear(TipoCuenta tipoCuenta)
    {
        using var connection = new SqlConnection(connectionString);
        // var id = await connection.QuerySingleAsync<int>
        //     (@"INSERT INTO TiposCuentas (Nombre, UsuarioId, Orden) 
        //     Values (@Nombre, @UsuarioId, 0); 
        //     SELECT SCOPE_IDENTITY();", tipoCuenta);

        // Obtener el máximo orden actual para el usuario y sumar 1
        var query = @"
            DECLARE @SiguienteOrden INT;
            
            SELECT @SiguienteOrden = ISNULL(MAX(Orden), 0) + 1
            FROM TiposCuentas 
            WHERE UsuarioId = @UsuarioId;
            
            INSERT INTO TiposCuentas (Nombre, UsuarioId, Orden) 
            VALUES (@Nombre, @UsuarioId, @SiguienteOrden);
            
            SELECT SCOPE_IDENTITY();";
        
        var id = await connection.QuerySingleAsync<int>(query, tipoCuenta);

        tipoCuenta.Id = id;
    }

    public async Task<bool> Existe(string nombre, int usuarioId)
    {
        using var connection = new SqlConnection(connectionString);
        var existe = await connection.QueryFirstOrDefaultAsync<int>(
            @"SELECT 1
            FROM TiposCuentas
            WHERE Nombre = @Nombre AND UsuarioId = @UsuarioId;",
            new {nombre, usuarioId});

        return existe == 1;
    }

    public async Task<IEnumerable<TipoCuenta>> Obtener(int usuarioId)
    {
        using var connection = new SqlConnection(connectionString);
        return await connection.QueryAsync<TipoCuenta>(
            @"SELECT Id, Nombre, Orden
            FROM TiposCuentas
            WHERE UsuarioId = @UsuarioId
            ORDER BY Orden;",
            new {usuarioId});

    }

    public async Task Actualizar(TipoCuenta tipoCuenta)
    {
        using var connection = new SqlConnection(connectionString);
        await connection.ExecuteAsync(
            @"UPDATE TiposCuentas
            SET Nombre = @Nombre
            WHERE Id = @Id;",
            tipoCuenta);
    }

    public async Task<TipoCuenta> ObtenerPorId(int id, int usuarioId)
    {
        using var connection = new SqlConnection(connectionString);
        var tipoCuenta = await connection.QueryFirstOrDefaultAsync<TipoCuenta>(
            @"SELECT Id, Nombre, Orden
            FROM TiposCuentas
            WHERE Id = @Id AND UsuarioId = @UsuarioId",
            new {id, usuarioId});

        if (tipoCuenta is null)
        {
            throw new KeyNotFoundException($"No se encontró un tipo de cuenta con Id {id} para el usuario {usuarioId}");
        }
        
        return tipoCuenta;
    }

    public async Task Borrar(int id)
    {
        using var connection = new SqlConnection(connectionString);
        await connection.ExecuteAsync(
            @"DELETE TiposCuentas
            WHERE Id = @Id",
            new {id});
    }

    public async Task Ordenar(IEnumerable<TipoCuenta> tipoCuentasOrdenados)
    {
        var query = "UPDATE TiposCuentas SET Orden = @Orden Where Id = @Id;";
        using var connection = new SqlConnection(connectionString);
        await connection.ExecuteAsync(query, tipoCuentasOrdenados);
    }
}
