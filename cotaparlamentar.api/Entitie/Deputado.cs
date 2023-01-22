using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace cotaparlamentar.api.Entitie;

[Table("tbdeputados")]
public class Deputado
{
    [Key]
    public int Id { get; set; }
    public int NuDeputadoId { get; set; }
    public string? Nome { get; set; }
    public string? Partido { get; set; }
    public string? Estado { get; set; }
    public string? NomeCivil { get; set; }
    public int IdPerfil { get; set; }
    public bool EmExercicio { get; set; }
    public DateTime DtAtualizacao { get; set; }
    public DateTime DtCadastro { get; set; }
    public DateTime? DtAtAssessor { get; set; }
    public string? Foto { get; set; }
}
