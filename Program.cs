using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.IO;
using System.Net.Http;
using Newtonsoft.Json;
using System.Runtime.Serialization;
using System.Web.Script.Serialization;
using System.Text.RegularExpressions;
using System.Security.Cryptography;

namespace DesafioCodenation
{
    class Program
    {
        static readonly HttpClient client = new HttpClient();

        #region Processamento
        static async Task Main()
        {
            try
            {
                HttpResponseMessage response = await client.GetAsync("https://api.codenation.dev/v1/challenge/dev-ps/generate-data?token=SEUTOKEN");
                response.EnsureSuccessStatusCode();
                string responseBody = await response.Content.ReadAsStringAsync();

                string arquivo = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "answer.json");

                File.Create(arquivo).Dispose();

                File.WriteAllText(arquivo, responseBody);

                string palavra = "";
                string encrypt = "";

                Answer answer;

                using (StreamReader r = new StreamReader(arquivo))
                {
                    string json = r.ReadToEnd();
                    answer = JsonConvert.DeserializeObject<Answer>(json);
                    palavra = answer.Cifrado;
                }

                for (int i = 0; i < palavra.Length; i++)
                {
                    string verificar = palavra[i].ToString();
                    var retorno = Regex.IsMatch(verificar, @"\w");

                    if (retorno)
                    {
                        int letra = Convert.ToInt32(palavra[i]) - 3;

                        if (letra < 97)
                        {
                            //letra = (letra - 122) + 97;
                            letra += 26;
                        }

                        encrypt += Convert.ToChar(letra);
                    }
                    else
                    {
                        int letra = Convert.ToInt32(palavra[i]);
                        encrypt += Convert.ToChar(letra);
                    }
                }

                Console.WriteLine("Resultado: " + encrypt);

                string hash = Hash(encrypt);

                Console.WriteLine("SHA1: " + hash);

                answer.Decifrado = encrypt;
                answer.Resumo_Criptografico = hash;

                string jsonAnswer = JsonConvert.SerializeObject(answer);

                using (var sw = new StreamWriter(arquivo))
                {
                    sw.Write(jsonAnswer);
                    sw.Flush();
                }

                byte[] bytes = Encoding.ASCII.GetBytes(jsonAnswer);
                var content = new MultipartFormDataContent();
                content.Add(new StreamContent(new MemoryStream(bytes)), "answer", arquivo);

                HttpResponseMessage submitResponse = await client.PostAsync(@"https://api.codenation.dev/v1/challenge/dev-ps/submit-solution?token=SEUTOKEN", content);

                string mensagem = await submitResponse.Content.ReadAsStringAsync();

                Console.WriteLine(mensagem);
            }
            catch (HttpRequestException e)
            {
                Console.WriteLine("\nException Caught!");
                Console.WriteLine("Message :{0} ", e.Message);
            }

            Console.ReadLine();
        } 
        #endregion
        #region Hash 
        static string Hash(string input)
        {
            using (SHA1Managed sha1 = new SHA1Managed())
            {
                var hash = sha1.ComputeHash(Encoding.UTF8.GetBytes(input));
                var sb = new StringBuilder(hash.Length * 2);

                foreach (byte b in hash)
                {
                    // can be "x2" if you want lowercase
                    sb.Append(b.ToString("x2"));
                }
                return sb.ToString();
            }
        } 
        #endregion
    }
}
