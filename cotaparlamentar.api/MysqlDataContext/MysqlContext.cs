using cotaparlamentar.api.Entitie;
using Microsoft.EntityFrameworkCore;

namespace cotaparlamentar.api.MysqlDataContext;

public class MysqlContext : DbContext
{
    public DbSet<Deputado> Deputado { get; set; }
    public DbSet<CotaParlamentar> CotaParlamentar { get; set; }
    public DbSet<Assessor> Assessor { get; set; }

    public MysqlContext(DbContextOptions<MysqlContext> options): base(options)
    {
    }
}
