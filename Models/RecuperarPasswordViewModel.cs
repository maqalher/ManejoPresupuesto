using System;
using System.ComponentModel.DataAnnotations;

namespace ManejoPresupuesto.Models;

public class RecuperarPasswordViewModel
{
    [Required(ErrorMessage = "El campo {0} es requerido")]
    [EmailAddress(ErrorMessage = "El campo debe ser un correo electronico valido")]
    public string Email { get; set; } = string.Empty;
    [Required(ErrorMessage = "El campo {0} es requerido")]
    [DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;
    public string CodigoReseteo { get; set; } = string.Empty;
}
