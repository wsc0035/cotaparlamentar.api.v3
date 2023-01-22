using cotaparlamentar.api.Entitie;
using cotaparlamentar.api.MysqlDataContext;
using HtmlAgilityPack;
using System.Text;
using System.Web;
using Magick.NET.WebImageExtensions;

namespace cotaparlamentar.api.Service;

public class DeputadoService
{
    private readonly MysqlContext _mysqlContext;
    private readonly IConfiguration _config;
    public DeputadoService(MysqlContext mysqlContext, IConfiguration config)
    {
        _mysqlContext = mysqlContext;
        _config = config;
    }


    public string BuscaTodosDeputadosSiteCompletoPorIdPerfil()
    {
        BuscaTodosDeputadoNovosSite();

        var listaSite = new List<Deputado>();

        var listaBanco = _mysqlContext.Deputado.ToList();

        Parallel.ForEach(listaBanco, deputado =>
        {
            listaSite.Add(BuscaDeputadoSiteAtualPorIdPerfil(deputado.IdPerfil));
        });

        foreach (var itemBanco in listaBanco)
        {
            foreach (var itemSite in (listaSite.Where(t => t.IdPerfil == itemBanco.IdPerfil)))
            {
                itemBanco.Nome = itemSite.Nome;
                itemBanco.NomeCivil = itemSite.NomeCivil;
                itemBanco.Partido = itemSite.Partido;
                itemBanco.Estado = itemSite.Estado;
                itemBanco.EmExercicio = itemSite.EmExercicio;
                itemBanco.DtAtualizacao = DateTime.Now;
            }
        }

        if (listaBanco.Count > 0)
        {
            _mysqlContext.UpdateRange(listaBanco);
            _mysqlContext.SaveChanges();
        }

        return LogReturn(listaBanco);
    }
    public string BuscaTodosDeputadosSiteCompletoPorIdPerfil(int nuDeputadoId)
    {

        var deputadoBanco = _mysqlContext.Deputado.Where(d => d.NuDeputadoId == nuDeputadoId).First();

        var deputadoSite = BuscaDeputadoSiteAtualPorIdPerfil(deputadoBanco.IdPerfil);

        deputadoBanco.Nome = deputadoSite.Nome;
        deputadoBanco.NomeCivil = deputadoSite.NomeCivil;
        deputadoBanco.Partido = deputadoSite.Partido;
        deputadoBanco.Estado = deputadoSite.Estado;
        deputadoBanco.EmExercicio = deputadoSite.EmExercicio;
        deputadoBanco.DtAtualizacao = DateTime.Now;

        _mysqlContext.Update(deputadoBanco);
        _mysqlContext.SaveChanges();

        return $"ATUALIZADO {deputadoBanco.NuDeputadoId} - {deputadoBanco.Nome} ({deputadoBanco.Partido}-{deputadoBanco.Estado})";
    }

    public async Task AtualizacaoDeFoto(int nuDeputadoId)
    {
        var deputadoBanco = _mysqlContext.Deputado.Where(d => d.NuDeputadoId == nuDeputadoId).First();

        var base64img = await BuscarFotoDeputado(deputadoBanco.IdPerfil);

        deputadoBanco.Foto = base64img;
        _mysqlContext.Update(deputadoBanco);
        _mysqlContext.SaveChanges();
    }

    public async Task AtualizacaoDeFoto()
    {
        var deputadoBanco = _mysqlContext.Deputado.Where(d => d.Foto == null).Take(10).ToList();

        foreach (var deputado in deputadoBanco)
        {
            deputado.Foto = await BuscarFotoDeputado(deputado.IdPerfil);
        }

        _mysqlContext.UpdateRange(deputadoBanco);
        _mysqlContext.SaveChanges();
    }

    public List<Deputado> BuscarTodosDeputadoSiteAtual()
    {
        var url = "https://www.camara.leg.br/cota-parlamentar/index.jsp";
        var web = new HtmlWeb();
        var doc = web.Load(url);

        HtmlNodeCollection matchesList = doc.DocumentNode.SelectNodes("//ul[@id='listaDeputados']//li");

        var deputadosList = new List<Deputado>();

        foreach (var deputado in matchesList)
        {
            deputadosList.Add(new Deputado
            {
                NuDeputadoId = Convert.ToInt32(deputado.SelectSingleNode(".//label//span").Attributes["id"].Value),
                Nome = HttpUtility.HtmlDecode(deputado.SelectSingleNode(".//label//span").InnerText).Trim()
            });
        }

        return deputadosList;
    }
    private void BuscaTodosDeputadoNovosSite()
    {
        var listaDeputadoSite = new List<Deputado>();

        var deputadosApi = _mysqlContext.Deputado.ToList();

        var deputadosSite = BuscarTodosDeputadoSiteAtual();

        var diff = deputadosSite.Where(p => !deputadosApi.Any(l => p.NuDeputadoId == l.NuDeputadoId)).ToList();

        Parallel.ForEach(diff, deputado => AtualizaIDPerfilDeputado(deputado));
        Parallel.ForEach(diff, deputado =>
        {
            var deputadoBusca = BuscaDeputadoSiteAtualPorIdPerfil(deputado.IdPerfil);
            deputado.NomeCivil = deputadoBusca.NomeCivil;
            deputado.EmExercicio = deputadoBusca.EmExercicio;
            deputado.Estado = deputadoBusca.Estado;
            deputado.Partido = deputadoBusca.Partido;
            deputado.IdPerfil = deputadoBusca.IdPerfil;
            deputado.DtCadastro = DateTime.Now;
            deputado.DtAtualizacao = DateTime.Now;
            listaDeputadoSite.Add(deputado);
        });

        if (listaDeputadoSite.Count > 0)
        {
            _mysqlContext.AddRange(listaDeputadoSite);
            _mysqlContext.SaveChanges();
        }
    }
    private Deputado BuscaDeputadoSiteAtualPorIdPerfil(int idperfil)
    {
        var url = $"https://www.camara.leg.br/deputados/{idperfil}";
        var web = new HtmlWeb();
        var doc = web.Load(url);

        HtmlNodeCollection depInterno = doc.DocumentNode.SelectNodes("//div[@class='l-identificacao-row']");

        if (depInterno == null)
            return new Deputado();

        var nome = depInterno.FirstOrDefault().SelectSingleNode("//*[@id='identificacao']/div/div/div[3]/div/div/div[2]/div[1]/ul/li[1]/text()") != null ? depInterno.FirstOrDefault().SelectSingleNode("//*[@id='identificacao']/div/div/div[3]/div/div/div[2]/div[1]/ul/li[1]/text()").InnerText : depInterno.FirstOrDefault().SelectSingleNode("//*[@id='identificacao']/div/div[4]/ul/li[1]/text()").InnerText;

        nome = HttpUtility.HtmlDecode(nome);

        var emExercicio = depInterno.FirstOrDefault().SelectSingleNode("//*[@id='identificacao']/div/div/div[2]/span/span[1]");

        var deputado = new Deputado();

        deputado.NomeCivil = nome.Trim();
        deputado.Nome = HttpUtility.HtmlDecode(depInterno.FirstOrDefault().SelectSingleNode("//*[@id='nomedeputado']").InnerText).Trim();
        deputado.Estado = depInterno.FirstOrDefault().SelectSingleNode("//span[@class='foto-deputado__partido-estado']").InnerText.Split()[2];
        deputado.Partido = depInterno.FirstOrDefault().SelectSingleNode("//span[@class='foto-deputado__partido-estado']").InnerText.Split()[0];
        deputado.EmExercicio = !(emExercicio == null);
        deputado.IdPerfil = idperfil;

        return deputado;
    }
    private Deputado AtualizaIDPerfilDeputado(Deputado deputado)
    {
        var web = new HtmlWeb();
        var url = string.Format("https://www.camara.leg.br/deputados/quem-sao/resultado?search={0}&partido=&uf=&legislatura={1}&sexo=", deputado.Nome, _config.GetSection("Legislatura").Value);
        var doc = web.Load(url);

        HtmlNodeCollection linkDetalhe = doc.DocumentNode.SelectNodes("//h3//a");
        if (linkDetalhe != null)
        {
            int idPerfil = Convert.ToInt32(linkDetalhe.FirstOrDefault().Attributes["href"].Value.Split('/')[4]);
            deputado.IdPerfil = idPerfil;
        }

        return deputado;
    }
    private string LogReturn(IEnumerable<Deputado> list)
    {
        var builder = new StringBuilder();
        builder.AppendLine("[ATUALIZACAO DEPUTADO]");
        foreach (var item in list)
        {
            builder.AppendLine($"{item.NuDeputadoId} - {item.Nome} ({item.Partido}-{item.Estado})");
        }
        builder.AppendLine("[ATUALIZACAO DEPUTADO]");
        return builder.ToString();
    }

    private async Task<string> BuscarFotoDeputado(int idperfil)
    {  
        var uri = $"https://www.camara.leg.br/internet/deputado/bandep/pagina_do_deputado/{idperfil}.jpg";

        using (var httpClient = new HttpClient())
        {
            var response = await httpClient.GetByteArrayAsync(uri);

            using (var image = new ImageMagick.MagickImage(response))
            {
                image.Crop(new ImageMagick.MagickGeometry(240, 0, 200, 200));
                image.Scale(new ImageMagick.MagickGeometry(0, 0, 160, 160));
                image.Write("output.jpg");
            }
        }

        var bytes = File.ReadAllBytes("output.jpg");
        return Convert.ToBase64String(bytes);
    }
}
