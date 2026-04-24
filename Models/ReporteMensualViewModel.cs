using System;

namespace ManejoPresupuesto.Models;

public class ReporteMensualViewModel
{
    public IEnumerable<ResultadoObtenerPorMes> TransaccionPorMes { get; set; } = new List<ResultadoObtenerPorMes>();
    public decimal Ingresos => TransaccionPorMes.Sum(x => x.Ingreso);
    public decimal Gasto => TransaccionPorMes.Sum(x => x.Gasto);    
    public decimal Total => Ingresos - Gasto;
    public int Año { get; set; }
}
