using System.Security.Cryptography;
using System.Text;

namespace cotaparlamentar.api.Service
{
    public class TokenService
    {
        private readonly string _secret;
        public TokenService(string secret)
        {
            _secret = secret;
        }
        public string GenerateToken()
        {            
            const int tempo = 3600;

            var data = DateTime.Now;
            var data2 = DateTime.Now.AddSeconds(tempo);
            string secretHash = MD5Hash($"{data}|{data2}|{_secret}");

            var strTexto = $"{data}|{data2}|{secretHash}";
            var hash = Convert.ToBase64String(Encoding.UTF8.GetBytes(strTexto));

            return hash;
        }
        public bool ValidToken(string token)
        {
            try
            {
                var secretHashHeader = MD5Hash(DecodeKey(token));
                var decodeObjHeder = RetornaObjHashJS(token);

                return (decodeObjHeder.DataExpira > DateTime.Now && decodeObjHeder.Chave == secretHashHeader);
            }
            catch
            {
                return false;
            }
        }
        private string MD5Hash(string input)
        {
            try
            {
                StringBuilder hash = new StringBuilder();
                MD5CryptoServiceProvider md5provider = new MD5CryptoServiceProvider();
                byte[] bytes = md5provider.ComputeHash(new UTF8Encoding().GetBytes(input));

                for (int i = 0; i < bytes.Length; i++)
                {
                    hash.Append(bytes[i].ToString("x2"));
                }
                return hash.ToString();
            }
            catch
            {
                return string.Empty;
            }

        }
        private string DecodeHash(string base64)
        {
            return Encoding.UTF8.GetString(Convert.FromBase64String(base64));
        }
        private string DecodeKey(string hash)
        {
            var decoded = DecodeHash(hash);
            var splitted = decoded.Split("|");
            return $"{splitted[0]}|{splitted[1]}|{_secret}";
        }
        private ObjHash RetornaObjHashJS(string hash)
        {
            var decoded = DecodeHash(hash);
            var splitted = decoded.Split("|");
            return new ObjHash()
            {
                DataInicio = Convert.ToDateTime(splitted[0]),
                DataExpira = Convert.ToDateTime(splitted[1]),
                Chave = splitted[2]
            };
        }
        private class ObjHash
        {
            public DateTime DataInicio { get; set; }
            public DateTime DataExpira { get; set; }
            public string Chave { get; set; }
        }


    }
}
