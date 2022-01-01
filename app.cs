using IPClient.Shared.IServices;
using IPClient.Shared.Model;
using IPClient.Shared.ModelDto;
using Newtonsoft.Json;
using Spectre.Console;
using System.Net;

namespace IPclient
{
    public class App : IApp
    {
        private IPingService _ping;
        private ApiConfiguration _config;

        public App(IPingService ping, ApiConfiguration config)
        {
            _ping = ping;
            _config = config;
        }

        public async Task Run()
        {
            Console.Clear();
            AnsiConsole.MarkupLine("[b green]Starting...[/]");
            Console.WriteLine();
            AnsiConsole.Markup("[blue] Testing server: [/]");
            bool isRunning = true;

            Response response = await _ping.TestServer();
            if (!response.Success)
            {
                AnsiConsole.Markup($"[red] response in {response.Errors[0]}ms.[/]\n");
            }
            AnsiConsole.Markup($"[green] response in [b]{response.Message}[/]ms.[/]\n");
            await Task.Delay(800);
            while (isRunning)
            {
                Console.Clear();
                string ipAddress = AnsiConsole.Ask<string>("Enter the IP address to search for:");
                if (!IsValidIPAddress(ipAddress))
                {
                    AnsiConsole.MarkupLine("[red][b]Error[/] Invalid IP address[/]");
                }
                else await LoadIPDataAsync(ipAddress);
                AnsiConsole.WriteLine();
                AnsiConsole.WriteLine();
                AnsiConsole.Markup("[dim] ... press [/]any key to search [dim]again or [/] q [dim] to exit ...[/]");
                char result = Console.ReadKey().KeyChar;
                if (result == 'q' || result == 'Q')
                    isRunning = false;
            }
            Console.WriteLine("OK");
        }

        public bool IsValidIPAddress(string IpAddress)
        {
            try
            {
                IPAddress? IP;
                if (IpAddress.Count(c => c == '.') == 3)
                {
                    return IPAddress.TryParse(IpAddress, out IP);
                }
            }
            catch (Exception) { return false; }
            return false;
        }

        public async Task LoadIPDataAsync(string IpAddress)
        {
            string searchUrl = $"{_config.ServerHttp}{IpAddress}?access_key={_config.AccessKey}";
            HttpClient client = new HttpClient();
            string jsonResponse;
            AnsiConsole.MarkupLine("[blue]Searching...[/]");

            try
            {
                var response = await client.GetAsync(searchUrl);
                if (response.IsSuccessStatusCode)
                {
                    Console.Clear();
                    jsonResponse = await response.Content.ReadAsStringAsync();
                    ApiResponseObject ipData = JsonConvert.DeserializeObject<ApiResponseObject>(jsonResponse);
                    if (ipData != null)
                    {
                        var font = FigletFont.Load("flf/starwars.flf");
                        AnsiConsole.Write(
                            new FigletText(IpAddress)
                            .Centered()
                            .Color(Color.Yellow3));

                        var table = new Table();
                        table.Centered();
                        table.Border(TableBorder.MinimalHeavyHead);

                        table.AddColumn("[Yellow b]Parameter[/]");
                        table.AddColumn("[Cyan b]Value[/]");

                        table.AddRow(":globe_showing_americas:[Yellow] Region: [/]", $"[palegreen1]{ipData.region_name}[/] {ipData.city} :telephone_receiver:[palegreen1]+{ipData.location.calling_code.ToString()}[/] ZIP: [palegreen1]{ipData.zip}[/]");
                        table.AddRow(":globe_with_meridians:[Yellow] Location: [/]", $"[palegreen1]long:[/] {ipData.longitude.ToString()} [palegreen1]lat: [/]{ipData.latitude.ToString()}");
                        table.AddRow(":globe_showing_europe_africa:[Yellow] Continent: [/]", $"[palegreen1]{ipData.continent_code}[/] {ipData.continent_name} ");
                        table.AddRow(":globe_showing_asia_australia:[Yellow] Country: [/]", $"[palegreen1]{ipData.country_code}[/] {ipData.country_name} ");
                        table.AddRow(":link:[Yellow] Flag file: [/]", $"{ipData.location.country_flag_emoji}[palegreen1][link={ipData.location.country_flag}]{ipData.location.country_flag}[/][/]");
                        string languageString = "";

                        foreach (var language in ipData.location.languages)
                        {
                            languageString += $"[palegreen1]{language.code} [/]{language.name}, [Cyan]{language.native}[/],";
                        }
                        languageString = languageString.Substring(0, languageString.Length - 1);

                        table.AddRow(":desktop_computer:[Yellow] Languages used: [/]", $"{languageString}");

                        AnsiConsole.Write(table);
                        AnsiConsole.WriteLine();
                        AnsiConsole.WriteLine();
                        AnsiConsole.WriteLine();
                    }
                }
                else
                {
                    Console.Write(response.Content);
                }
            }
            catch
            {
                Console.Write("Error");
            }
        }

        public string DecodeEncodedNonAsciiCharacters(string value)
        {
            return System.Text.RegularExpressions.Regex.Replace(value, @"\\u(?<value>[a-zA-Z0-9]{4})",
                m =>
                {
                    return ((char)int.Parse(m.Groups["Value"].Value, System.Globalization.NumberStyles.HexNumber)).ToString();
                });
        }
    }
}