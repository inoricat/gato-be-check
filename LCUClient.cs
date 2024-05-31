
using System.Net.Http.Headers;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;

using System.Text.Json.Nodes;

namespace gato_be_check;


public class LCUClient
{
    public ushort Port
    { get; private set; }
    public string Token
    { get; private set; }

    private HttpClient _client;

    public LCUClient(ushort port, string token, string proto = "https")
    {
        Port = port;
        Token = token;

        // Set client with handler that ignores cert
        _client = new HttpClient(
            new HttpClientHandler
            {
                // all certs are valid
                ServerCertificateCustomValidationCallback =
                    (HttpRequestMessage req, X509Certificate2? cert, X509Chain? chain, SslPolicyErrors errors) => true
            }
        );

        // Set headers
        _client.DefaultRequestHeaders.Accept.Clear();
        _client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        _client.DefaultRequestHeaders.Authorization = new("Basic", Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes($"riot:{token}")));

        // Set uri
        UriBuilder uri = new()
        {
            Host = "127.0.0.1",
            Port = Port,
            Scheme = proto
        };
        _client.BaseAddress = uri.Uri;

        Console.WriteLine(_client.BaseAddress);
    }

    public async Task<JsonNode?> Get(string path)
    {

        //HttpResponseMessage response = await _client.GetAsync(path);
        //response.EnsureSuccessStatusCode();
        //return JsonNode.Parse(response.Content.ReadAsStream());
        
        return JsonNode.Parse(await _client.GetStreamAsync(path));
    }

    public void Dispose() {
        _client.Dispose();
    }
}