using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace cotaparlamentar.api.Entitie;

[Table("tbcotaparlamentar")]
public class CotaParlamentar
{
    [Key]
    public int Id { get; set; }
    public int NuDeputadoId { get; set; }
    public string? TipoDespesa { get; set; }
    public string? LinkDespesa { get; set; }
    public string? LinkDespesaSumarizado { get; set; }
    public decimal Despesa { get; set; }
    public DateTime Data { get; set; }
    public DateTime DtCadastro { get; set; } = DateTime.Now;
}
