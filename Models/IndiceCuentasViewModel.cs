using System;

namespace ManejoPresupuesto.Models;

public class IndiceCuentasViewModel
{
    public string TipoCuenta { get; set; }  = string.Empty;
    public IEnumerable<Cuenta> Cuentas { get; set; } = new List<Cuenta>();
    public decimal Balance => Cuentas.Sum(x => x.Balance);
}
