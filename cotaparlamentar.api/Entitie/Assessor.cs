using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace cotaparlamentar.api.Entitie;

[Table("tbassessor")]
public class Assessor : IEquatable<Assessor>
{
    [Key]
    public int Id { get; set; }
    public int NuDeputadoId { get; set; }
    public string? Nome { get; set; }
    public string? Cargo { get; set; }
    public DateTime PeriodoExercicio { get; set; }
    public decimal Remuneracao { get; set; }
    public decimal Auxilio { get; set; }
    public string? LinkRemuneracao { get; set; }
    public DateTime DtCadastro { get; set; } = DateTime.Now;

    public bool Equals(Assessor other)
    {
        return this.Nome == other.Nome;
    }

    public override int GetHashCode()
    {
        return this.Nome.GetHashCode();
    }
}
