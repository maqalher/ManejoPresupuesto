using System.ComponentModel.DataAnnotations;
using ManejoPresupuesto.Validaciones;
using Microsoft.AspNetCore.Mvc;

namespace ManejoPresupuesto.Models;

// public class TipoCuenta: IValidatableObject
public class TipoCuenta
{
   public int Id { get; set; }
   [Required(ErrorMessage = "El campo {0} es requerido")]
//    [StringLength(maximumLength:50, MinimumLength = 3, ErrorMessage = "La longuitud del campo {0} debe de estar entre {2} y {1}")]
//    [Display(Name = "Nombre del tipo cuenta")]
    [PrimeraLetraMayuscula]
    [Remote(action:"VerificarExisteCuenta", controller: "TiposCuentas")]
   public string Nombre { get; set; } = string.Empty;
   public int UsuarioId { get; set; }
   public int Orden { get; set; }

    // public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    // {
    //     if(Nombre != null && Nombre.Length > 0)
    //     {
    //         var primeraLetra = Nombre[0].ToString();
    //         if(primeraLetra != primeraLetra.ToUpper())
    //         {
    //             yield return new ValidationResult("La primara letra debe ser mayuscula", 
    //             new[] {nameof(Nombre)});
    //         }
    //     }

    // }

    /*Pruebas de otras validacions por defecto*/
    //    [Required(ErrorMessage = "El campo {0} es requerido")]
    //    [EmailAddress(ErrorMessage = "El campo debe ser un correo electronico valido")]
    //    public string Email { get; set; } = string.Empty;
    //    [Range(minimum: 18, maximum: 130, ErrorMessage = "El valor debe estar entre {1} y {2}")]
    //    public int Edad { get; set; } 
    //    [Url(ErrorMessage = "El campo debe ser una URL valida")]
    //    public string URL { get; set; } = string.Empty;
    //    [CreditCard(ErrorMessage = "La tarjeta de credito no es valida")]
    //    [Display(Name = "Tarjeta de Credito")]
    //    public string TarjetaDeCredito { get; set; } = string.Empty;

}
