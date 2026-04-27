using System;
using Dapper;
using ManejoPresupuesto.Models;
using Microsoft.Data.SqlClient;

namespace ManejoPresupuesto.Servicios;

public interface IRepositorioUsuarios
{
    Task<int> CrearUsuario(Usuario usuario);
    Task<Usuario?> BuscarUsuarioPorEmail(string emailNormalizado);
}

public class RepositorioUsuarios: IRepositorioUsuarios
{
    private readonly string connectionString;
    public RepositorioUsuarios(IConfiguration configuration)
    {
        connectionString = configuration.GetConnectionString("DefaultConnection") ?? throw new Exception("La cadena de conexión 'DefaultConnection' no fue encontrada.");
    }

    public async Task<int> CrearUsuario(Usuario usuario)
    {   
        // using var connection = new SqlConnection(connectionString);
        // var usuarioId = await connection.QuerySingleAsync<int>(
        //     @"INSERT INTO Usuarios  (Email, EmailNormalizado, PasswordHash)
        //     VALUES (@Email, @EmailNormalizado, @PasswordHash);
        //     SELECT SCOPE_IDENTITY();", usuario);

        // await connection.ExecuteAsync("CrearDatosusuarioNuevo", new {usuarioId}, commandType: System.Data.CommandType.StoredProcedure);

        // return usuarioId;
        using var connection = new SqlConnection(connectionString);
        await connection.OpenAsync();
    
        using var transaction = await connection.BeginTransactionAsync();

        try
        {
            // 1. Insertar el usuario
            var usuarioId = await connection.QuerySingleAsync<int>(
                @"INSERT INTO Usuarios (Email, EmailNormalizado, PasswordHash)
                VALUES (@Email, @EmailNormalizado, @PasswordHash);
                SELECT SCOPE_IDENTITY();", 
                usuario, 
                transaction: transaction);
            
            // 2. Insertar tipos de cuentas predefinidos
            await connection.ExecuteAsync(
                @"INSERT INTO TiposCuentas (Nombre, UsuarioId, Orden)
                VALUES ('Efectivo', @UsuarioId, 1),
                        ('Cuentas de Banco', @UsuarioId, 2),
                        ('Tarjetas', @UsuarioId, 3);",
                new { UsuarioId = usuarioId },
                transaction: transaction);
            
            // 3. Crear cuentas basadas en los tipos de cuentas recién insertados
            await connection.ExecuteAsync(
                @"INSERT INTO Cuentas (Nombre, Balance, TipoCuentaId)
                SELECT Nombre, 0, Id
                FROM TiposCuentas
                WHERE UsuarioId = @UsuarioId;",
                new { UsuarioId = usuarioId },
                transaction: transaction);
            
            // 4. Insertar categorías predefinidas
            await connection.ExecuteAsync(
                @"INSERT INTO Categorias (Nombre, TipoOperacionId, UsuarioId)
                VALUES ('Libros', 2, @UsuarioId),
                        ('Salario', 1, @UsuarioId),
                        ('Mesada', 1, @UsuarioId),
                        ('Comida', 2, @UsuarioId);",
                new { UsuarioId = usuarioId },
                transaction: transaction);
            
            // Confirmar la transacción
            await transaction.CommitAsync();
            
            return usuarioId;
        }
        catch
        {
            // Si algo sale mal, deshacer todos los cambios
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task<Usuario?> BuscarUsuarioPorEmail(string emailNormalizado)
    {   
        using var connection = new SqlConnection(connectionString);
        return await connection.QuerySingleOrDefaultAsync<Usuario>(
            @"SELECT * FROM Usuarios WHERE EmailNormalizado = @emailNormalizado", new {emailNormalizado});

    }
}
