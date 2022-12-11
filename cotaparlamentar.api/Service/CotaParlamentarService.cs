using cotaparlamentar.api.Entitie;
using cotaparlamentar.api.MysqlDataContext;
using HtmlAgilityPack;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Web;

namespace cotaparlamentar.api.Service;

public class CotaParlamentarService
{
    private readonly DeputadoService _deputadoService;
    private readonly MysqlContext _mysqlContext;

    public CotaParlamentarService(DeputadoService deputadoService, MysqlContext mysqlContext)
    {
        _deputadoService = deputadoService;
        _mysqlContext = mysqlContext;
    }

    public string BuscarCotaParlamentarPorData(string data)
    {
        var listaCota = new List<CotaParlamentar>();
        var deputados = _deputadoService.BuscarTodosDeputadoSiteAtual();

        Parallel.ForEach(deputados, dep =>
        {
            listaCota.AddRange(ListaCotaParlamentarPorId(data, dep.NuDeputadoId));
        });

        if (listaCota.Count > 0)
        {
            _mysqlContext.Database.ExecuteSqlRaw("DELETE FROM tbcotaparlamentar WHERE data = {0}", FormatData(data).ToString("yyyy-MM-dd"));
            _mysqlContext.SaveChanges();

            _mysqlContext.CotaParlamentar.AddRange(listaCota);
            _mysqlContext.SaveChanges();
        }

        return LogReturn(listaCota, FormatData(data).ToString("yyyy-MM-dd"));
    }
    public string BuscarCotaParlamentarPorDataId(string data, int id)
    {

        var cotaParlamentar = ListaCotaParlamentarPorId(data, id);

        if (cotaParlamentar.Count > 0)
        {
            _mysqlContext.Database.ExecuteSqlRaw("DELETE FROM tbcotaparlamentar WHERE data = {0} and nuDeputadoId = {1}", FormatData(data).ToString("yyyy-MM-dd"), id);
            _mysqlContext.SaveChanges();

            _mysqlContext.CotaParlamentar.AddRange(cotaParlamentar);
            _mysqlContext.SaveChanges();
        }

        return LogReturn(cotaParlamentar, FormatData(data).ToString("yyyy-MM-dd"));
    }
    private List<CotaParlamentar> ListaCotaParlamentarPorId(string data, int nuDeputadoId)
    {
        var listaCota = new List<CotaParlamentar>();

        var web = new HtmlWeb();
        var url = string.Format("https://www.camara.leg.br/cota-parlamentar/sumarizado?nuDeputadoId={0}&dataInicio={1}&dataFim={1}&despesa=&nomeHospede=&nomePassageiro=&nomeFornecedor=&cnpjFornecedor=&numDocumento=&sguf=&filtroNivel1=1&filtroNivel2=2&filtroNivel3=3", nuDeputadoId, data);
        var doc = web.Load(url);

        HtmlNodeCollection tabelaCota = doc.DocumentNode.SelectNodes("//table[@class='tabela-2']//tbody//tr");

        if (tabelaCota != null)
        {
            for (int j = 0; j < tabelaCota.Count - 1; j++)
            {
                var idTd = string.Format("sumarizado{0}", j);
                listaCota.Add(new CotaParlamentar
                {
                    NuDeputadoId = nuDeputadoId,
                    Data = FormatData(data),
                    Despesa = Convert.ToDecimal(tabelaCota[j].SelectSingleNode("//tbody//tr//td[@id='" + idTd + "']").InnerText.Trim().Substring(3), CultureInfo.CreateSpecificCulture("pt-BR"),
                    TipoDespesa = tabelaCota[j].SelectSingleNode("//tbody//a[@id='linkSumarizado" + j + "']").InnerText.Trim(),
                    LinkDespesa = Regex.Replace(HttpUtility.HtmlDecode("https://" + new Uri(url).Host.ToString() + tabelaCota[j].SelectSingleNode("//tbody//a[@id='linkSumarizado" + j + "']").Attributes["href"].Value.Trim()), @"\s+", ""),
                    LinkDespesaSumarizado = url
                });
            }
        }

        return listaCota;
    }
    private static DateTime FormatData(string data)
    {
        data = HttpUtility.UrlDecode(data);
        int pos = data.IndexOf("/");
        var mes = data.Remove(pos, data.Length - pos);
        var ano = data.Substring(pos + 1);

        return new DateTime(Convert.ToInt32(ano), Convert.ToInt32(mes), 1);
    }
    private string LogReturn(List<CotaParlamentar> list, string data)
    {
        return $"[INSERINDO COTA BATCH] LOP TOTAL {list.Count} - Data {data} Total: {list.Select(e => e.Despesa).Sum().ToString("C", CultureInfo.CurrentCulture)} ";
    }
}
