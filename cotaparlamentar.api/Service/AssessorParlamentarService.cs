using cotaparlamentar.api.Entitie;
using cotaparlamentar.api.MysqlDataContext;
using HtmlAgilityPack;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using System.Text;

namespace cotaparlamentar.api.Service;

public class AssessorParlamentarService
{
    private readonly MysqlContext _mysqlContext;

    public AssessorParlamentarService(MysqlContext mysqlContext)
    {
        _mysqlContext = mysqlContext;
    }
    public string AtualizarAssessor()
    {
        var contextDeputados = _mysqlContext.Deputado.Where(w => w.DtAtAssessor < DateTime.Now.AddDays(-20) || w.DtAtAssessor == null)
                                                    .OrderBy(o => o.DtAtAssessor)
                                                    .Select(s => new { s.NuDeputadoId, s.IdPerfil, s.Nome })
                                                    .AsNoTracking()
                                                    .Take(50)
                                                    .ToList();

        var listaAssessores = new List<Assessor>();

        var listNuDeputadoId = contextDeputados.Select(s => s.NuDeputadoId).ToList();

        Parallel.ForEach(contextDeputados, deputado =>
        {
            listaAssessores.AddRange(BuscarAssessorParlamentar(deputado.NuDeputadoId, deputado.IdPerfil));
        });

        if (listaAssessores.Count > 0)
        {
            var queryDelete = string.Format("DELETE FROM tbassessor WHERE nuDeputadoId IN ({0})", string.Join(",", listNuDeputadoId));
            _mysqlContext.Database.ExecuteSqlRaw(queryDelete);
            _mysqlContext.SaveChanges();

            _mysqlContext.Assessor.AddRange(listaAssessores);
            _mysqlContext.SaveChanges();

            var queryUpdate = string.Format("UPDATE tbdeputados SET dtAtAssessor = NOW() WHERE nuDeputadoId IN ({0})", string.Join(",", listNuDeputadoId));
            _mysqlContext.Database.ExecuteSqlRaw(queryUpdate);
            _mysqlContext.SaveChanges();
        }        

        return LogReturn(contextDeputados);
    }
    public string AtualizacaoAssessorID(int nuDeputadoId)
    {
        var idperfil = _mysqlContext.Deputado.Where(d => d.NuDeputadoId == nuDeputadoId)
                                             .Select(s => s.IdPerfil)
                                             .FirstOrDefault();

        if (idperfil == 0) return "Deputado não encontrado";

        var assessor = BuscarAssessorParlamentar(nuDeputadoId,idperfil);

        if (assessor.Count > 0)
        {
            var queryDelete = string.Format("DELETE FROM tbassessor WHERE nuDeputadoId = {0}", nuDeputadoId);
            _mysqlContext.Database.ExecuteSqlRaw(queryDelete);
            _mysqlContext.SaveChanges();

            _mysqlContext.Assessor.AddRange(assessor);
            _mysqlContext.SaveChanges();

            var queryUpdate = string.Format("UPDATE tbdeputados SET dtAtAssessor = NOW() WHERE nuDeputadoId = {0}", nuDeputadoId);
            _mysqlContext.Database.ExecuteSqlRaw(queryUpdate);
            _mysqlContext.SaveChanges();

            return $"Asessor deputado {nuDeputadoId} Atualizado: {assessor.Count} assessores";
        }

        return "Não retornou resultado";
    }
    private List<Assessor> BuscarAssessorParlamentar(int nuDeputadoId, int idperfil)
    {
        var listAssessor = new List<Assessor>();

        var web = new HtmlWeb();
        var url = string.Format("https://www.camara.leg.br/deputados/{0}/pessoal-gabinete?ano={1}", idperfil, DateTime.Now.Year.ToString());
        var doc = web.Load(url);

        HtmlNodeCollection assessores = doc.DocumentNode.SelectNodes("//*[@id='main-content']/section/div[1]/table[1]");
        if (assessores != null)
        {
            HtmlNodeCollection cells = assessores.FirstOrDefault().SelectNodes("/html[1]/body[1]/div[1]/main[1]/section[1]/div[1]/table[1]/tbody[1]/tr");

            for (int i = 0; i < cells.Count; ++i)
            {
                HtmlNodeCollection td = cells[i].SelectNodes("td");
                if (td != null)
                {
                    var assessor = new Assessor();
                    var data = DateTime.Now;
                    DateTime.TryParse(td[3].InnerText.Substring(6), CultureInfo.CreateSpecificCulture("pt-BR"), DateTimeStyles.AdjustToUniversal, out data);
                    assessor.Nome = td[0].InnerText;
                    assessor.Cargo = td[1].InnerText;
                    assessor.PeriodoExercicio = data;
                    assessor.LinkRemuneracao = td[4].SelectSingleNode("a").Attributes["href"].Value;
                    assessor.NuDeputadoId = nuDeputadoId;

                    listAssessor.Add(assessor);
                }
            }

            AtualizarSalario(listAssessor);
        }

        return listAssessor;
    }
    private static void AtualizarSalario(List<Assessor> lstAssessor)
    {
        Parallel.ForEach(lstAssessor, assessor =>
        {
            var url = assessor.LinkRemuneracao;
            var web = new HtmlWeb();
            var doc = web.Load(url);

            HtmlNodeCollection tabRemuneracaoScript = doc.DocumentNode.SelectNodes("//main[@id='main-content']");
            var script = tabRemuneracaoScript.FirstOrDefault().SelectSingleNode("//body//script");

            var scriptUrl = script.InnerText.Replace("window.location.href = '", "").Trim().Replace("';", "").Trim();

            if (!string.IsNullOrEmpty(scriptUrl))
            {
                var docScript = web.Load(scriptUrl);

                HtmlNodeCollection tabRemuneracao = docScript.DocumentNode.SelectNodes("//table[@class='tabela-responsiva tabela-responsiva--remuneracao-funcionario']");

                if (tabRemuneracao != null)
                {
                    var remuneracaoFixa = Convert.ToDecimal(tabRemuneracao.FirstOrDefault().SelectSingleNode("//tr[td='a - Remuneração Fixa']/td[2]").InnerText, System.Globalization.CultureInfo.CreateSpecificCulture("pt-BR"));
                    var vangatens = Convert.ToDecimal(tabRemuneracao.FirstOrDefault().SelectSingleNode("//tr[td='b - Vantagens de Natureza Pessoal']/td[2]").InnerText, System.Globalization.CultureInfo.CreateSpecificCulture("pt-BR"));
                    var remuneracao = Convert.ToDecimal(tabRemuneracao.FirstOrDefault().SelectSingleNode("//tr[td='a - Função ou Cargo em Comissão']/td[2]").InnerText, System.Globalization.CultureInfo.CreateSpecificCulture("pt-BR"));
                    var gratifica = Convert.ToDecimal(tabRemuneracao.FirstOrDefault().SelectSingleNode("//tr[td='b - Gratificação Natalina']/td[2]").InnerText, System.Globalization.CultureInfo.CreateSpecificCulture("pt-BR"));
                    var ferias = tabRemuneracao.FirstOrDefault().SelectSingleNode("//tr[td='c - Férias (1/3 Constitucional)']/td[2]") != null ? Convert.ToDecimal(tabRemuneracao.FirstOrDefault().SelectSingleNode("//tr[td='c - Férias (1/3 Constitucional)']/td[2]").InnerText, System.Globalization.CultureInfo.CreateSpecificCulture("pt-BR")) : 0;
                    var outros = Convert.ToDecimal(tabRemuneracao.FirstOrDefault().SelectSingleNode("//tr[td='d - Outras Remunerações Eventuais/Provisórias(*)']/td[2]").InnerText, System.Globalization.CultureInfo.CreateSpecificCulture("pt-BR"));

                    var auxilio = Convert.ToDecimal(tabRemuneracao.FirstOrDefault().SelectSingleNode("//tr[td='b - Auxílios']/td[2]").InnerText, System.Globalization.CultureInfo.CreateSpecificCulture("pt-BR"));
                    var diaria = Convert.ToDecimal(tabRemuneracao.FirstOrDefault().SelectSingleNode("//tr[td='a - Diárias']/td[2]").InnerText, System.Globalization.CultureInfo.CreateSpecificCulture("pt-BR"));
                    var ideniza = Convert.ToDecimal(tabRemuneracao.FirstOrDefault().SelectSingleNode("//tr[td='c - Vantagens Indenizatórias']/td[2]").InnerText, System.Globalization.CultureInfo.CreateSpecificCulture("pt-BR"));

                    assessor.Remuneracao = remuneracao + remuneracaoFixa + vangatens + gratifica + ferias + outros;
                    assessor.Auxilio = auxilio + diaria + ideniza;
                }
            }
        });
    }
    private string LogReturn(IEnumerable<dynamic> list)
    {
        var builder = new StringBuilder();
        builder.AppendLine("[ATUALIZACAO ASSESSOR]");
        foreach (var item in list)
        {
            builder.AppendLine($"{item.NuDeputadoId} - {item.IdPerfil} : {item.Nome}");
        }
        builder.AppendLine("[ATUALIZACAO ASSESSOR]");
        return builder.ToString();
    }
}
