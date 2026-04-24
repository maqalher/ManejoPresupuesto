using System;
using System.ComponentModel.DataAnnotations;

namespace ManejoPresupuesto.Validaciones;

public class PrimeraLetraMayusculaAttribute: ValidationAttribute
{
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if(value == null || string.IsNullOrEmpty(value.ToString()))
        {
            return ValidationResult.Success;
        }

        var primaraLetra = value.ToString()[0].ToString();

        if(primaraLetra != primaraLetra.ToUpper())
        {
            return new ValidationResult("La primara letra debe ser mayuscula");
        }

        return ValidationResult.Success;
    }
}
