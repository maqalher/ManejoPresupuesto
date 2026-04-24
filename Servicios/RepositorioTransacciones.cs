using System;
using Dapper;
using ManejoPresupuesto.Models;
using Microsoft.Data.SqlClient;

namespace ManejoPresupuesto.Servicios;

public interface IRepositorioTransacciones
{
    Task Crear(Transaccion transaccion);
    Task Actializar(Transaccion transaccion, decimal montoAnterior, int cuentaAnteriorId);
    Task<Transaccion?> ObtenerPorId(int id, int usuarioId);
    Task Borrar(int id);
    Task<IEnumerable<Transaccion>> ObtenerPorCuentaId(ObtenerTransaccionesPorCuenta modelo);
    Task<IEnumerable<Transaccion>> ObtenerPorUsuarioId(ParametroObtenerTransaccionesPorUsuario modelo);
    Task<IEnumerable<ResultadoObtenerPorSemana>> ObtenerPorSemana(ParametroObtenerTransaccionesPorUsuario modelo);

    Task<IEnumerable<ResultadoObtenerPorMes>> ObtenerPorMes(int usuarioId, int año);
}


public class RepositorioTransacciones: IRepositorioTransacciones
{   
    private readonly string connectionString;

    public RepositorioTransacciones(IConfiguration configuration)
    {
        connectionString = configuration.GetConnectionString("DefaultConnection") ?? throw new Exception("La cadena de conexión 'DefaultConnection' no fue encontrada.");
    }

    public async Task Crear(Transaccion transaccion)
    {
        using var connection = new SqlConnection(connectionString);
        
        var query = @"
            DECLARE @InsertedId INT;
            
            INSERT INTO Transacciones (UsuarioId, FechaTransaccion, Monto, CategoriaId, CuentaId, Nota)
            VALUES (@UsuarioId, @FechaTransaccion, ABS(@Monto), @CategoriaId, @CuentaId, @Nota);
            
            SET @InsertedId = SCOPE_IDENTITY();
            
            UPDATE Cuentas
            SET Balance += @Monto
            WHERE Id = @CuentaId;
            
            SELECT @InsertedId;";
        
        var id = await connection.ExecuteScalarAsync<int>(query, new
        {
            transaccion.UsuarioId,
            transaccion.FechaTransaccion,
            transaccion.Monto,
            transaccion.CategoriaId,
            transaccion.CuentaId,
            transaccion.Nota,
        });
        
        transaccion.Id = id;
    }

    public async Task<IEnumerable<Transaccion>> ObtenerPorCuentaId(ObtenerTransaccionesPorCuenta modelo)
    {
        using var connection = new SqlConnection(connectionString);
        return await connection.QueryAsync<Transaccion>(
            @"SELECT t.Id, t.Monto, t.FechaTransaccion, c.Nombre as Categoria,
                cu.Nombre as Cuenta, c.TipoOperacionId
                FROM Transacciones t
                INNER JOIN Categorias c
                ON c.Id = t.CategoriaId
                INNER JOIN Cuentas cu
                ON cu.Id = t.CuentaId
                WHERE t.CuentaId = @CuentaId AND t.UsuarioId = @UsuarioId
                AND FechaTransaccion BETWEEN @FechaInicio AND @FechaFin",modelo);
    }

    public async Task<IEnumerable<Transaccion>> ObtenerPorUsuarioId(ParametroObtenerTransaccionesPorUsuario modelo)
    {
        using var connection = new SqlConnection(connectionString);
        return await connection.QueryAsync<Transaccion>(
            @"SELECT t.Id, t.Monto, t.FechaTransaccion, c.Nombre as Categoria,
                cu.Nombre as Cuenta, c.TipoOperacionId
                FROM Transacciones t
                INNER JOIN Categorias c
                ON c.Id = t.CategoriaId
                INNER JOIN Cuentas cu
                ON cu.Id = t.CuentaId
                WHERE t.UsuarioId = @UsuarioId
                AND FechaTransaccion BETWEEN @FechaInicio AND @FechaFin
                ORDER BY t.FechaTransaccion DESC",modelo);
    }

    public async Task Actializar(Transaccion transaccion, decimal montoAnterior, int cuentaAnteriorId)
    {
        using var connection = new SqlConnection(connectionString);

        var query = @"
        -- Revertir transacción anterior
        UPDATE Cuentas
        SET Balance = Balance - @MontoAnterior
        WHERE Id = @CuentaAnteriorId;
        
        -- Realizar nueva transacción
        UPDATE Cuentas
        SET Balance = Balance + @Monto
        WHERE Id = @CuentaId;
        
        -- Actualizar la transacción
        UPDATE Transacciones
        SET Monto = ABS(@Monto), 
            FechaTransaccion = @FechaTransaccion,
            CategoriaId = @CategoriaId, 
            CuentaId = @CuentaId, 
            Nota = @Nota
        WHERE Id = @Id;";
    
        await connection.ExecuteAsync(query, new
        {
            transaccion.Id,
            transaccion.FechaTransaccion,
            transaccion.Monto,
            transaccion.CategoriaId,
            transaccion.CuentaId,
            transaccion.Nota,
            MontoAnterior = montoAnterior,
            CuentaAnteriorId = cuentaAnteriorId
        });
    }

    public async Task<Transaccion?> ObtenerPorId(int id, int usuarioId)
    {
        using var connection = new SqlConnection(connectionString);
        return await connection.QueryFirstOrDefaultAsync<Transaccion>(
            @"SELECT Transacciones.*, cat.TipoOperacionId
            FROM Transacciones
            INNER JOIN Categorias cat
            ON cat.Id = Transacciones.CategoriaId
            WHERE Transacciones.Id = @Id AND Transacciones.UsuarioId = @UsuarioId",
            new { id, usuarioId });
    }

    public async Task<IEnumerable<ResultadoObtenerPorSemana>> ObtenerPorSemana(ParametroObtenerTransaccionesPorUsuario modelo)
    {
        using var connection = new SqlConnection(connectionString);
        // return await connection.QueryAsync<ResultadoObtenerPorSemana>(
        //     @"SELECT datediff(d, @fechaInicio, FechaTransaccion) / 7 + 1 as Semana
        //     SUM(Monto) as Monto, cat.TipoOperacionId
        //     FROM Transacciones
        //     INNER JOIN Categorias cat
        //     ON cat.Id = Transacciones.CategoriaId
        //     WHERE Transacciones.UsusarioId = @usuarioId AND
        //     FechaTransaccion BETWEEN @fechaInicio AND @fechaFin
        //     GROUP BY datediff(d,@fechaInicio, FechaTransaccion) / 7, cat.TipoOperacionId
        //     ", modelo);
        return await connection.QueryAsync<ResultadoObtenerPorSemana>(
        @"SELECT 
            DATEDIFF(day, @fechaInicio, FechaTransaccion) / 7 + 1 AS Semana,
            SUM(Transacciones.Monto) AS Monto,
            cat.TipoOperacionId
        FROM Transacciones
        INNER JOIN Categorias cat
            ON cat.Id = Transacciones.CategoriaId
        WHERE Transacciones.UsuarioId = @usuarioId 
            AND FechaTransaccion BETWEEN @fechaInicio AND @fechaFin
        GROUP BY 
            DATEDIFF(day, @fechaInicio, FechaTransaccion) / 7,
            cat.TipoOperacionId
        ORDER BY Semana", modelo);
    }

    public async Task<IEnumerable<ResultadoObtenerPorMes>> ObtenerPorMes(int usuarioId, int año)
    {
        using var connection = new SqlConnection(connectionString);
        return await connection.QueryAsync<ResultadoObtenerPorMes>(
        @"SELECT
            MONTH(FechaTransaccion) as Mes,
            SUM(Monto) as Monto, cat.TipoOperacionId
        FROM Transacciones
        INNER JOIN Categorias cat
            ON cat.Id = Transacciones.CategoriaId
        WHERE Transacciones.usuarioId = @usuarioId AND YEAR(FechaTransaccion) = @Año
        GROUP BY Month(FechaTransaccion), cat.TipoOperacionId", new {usuarioId, año});
    }
    public async Task Borrar(int id)
    {
        using var connection = new SqlConnection(connectionString);
        
        var query = @"
            -- Obtener datos de la transacción a eliminar
            DECLARE @Monto decimal(18,2);
            DECLARE @CuentaId int;
            DECLARE @TipoOperacionId int;
            
            SELECT @Monto = t.Monto, 
                @CuentaId = t.CuentaId, 
                @TipoOperacionId = cat.TipoOperacionId
            FROM Transacciones t
            INNER JOIN Categorias cat ON cat.Id = t.CategoriaId
            WHERE t.Id = @Id;
            
            -- Calcular el factor multiplicativo según el tipo de operación
            DECLARE @FactorMultiplicativo int = 1;
            
            IF (@TipoOperacionId = 2)
                SET @FactorMultiplicativo = -1;
            
            -- Ajustar el monto (invertir el signo para revertir la operación)
            SET @Monto = @Monto * @FactorMultiplicativo;
            
            -- Actualizar el balance de la cuenta
            UPDATE Cuentas
            SET Balance = Balance - @Monto
            WHERE Id = @CuentaId;
            
            -- Eliminar la transacción
            DELETE Transacciones
            WHERE Id = @Id;";
        
        await connection.ExecuteAsync(query, new { id });
    }  
}
