using System.Buffers.Text;
using System.Text;
using ProxySubscribe.Model;
using ProxySubscribe.Services.Protocols;
using System.Text.Json;

namespace ProxySubscribe.Services;

public class ProtocolAppService : IProtocolAppService
{
    private const string ProxyDomain = "richfocks.top";

    private readonly IConfiguration _configuration;
    private readonly IHttpClientFactory _httpClientFactory;

    public ProtocolAppService(IConfiguration configuration, IHttpClientFactory httpClientFactory)
    {
        _configuration = configuration;
        _httpClientFactory = httpClientFactory;
    }

    /**
     * vmess://ew0KICAidiI6ICIyIiwNCiAgInBzIjogInRlc3QyIiwNCiAgImFkZCI6ICIxNTguMTAxLjE1MC4xMzkiLA0KICAicG9ydCI6ICI0NDMiLA0KICAiaWQiOiAiMGFlYmJjODgtOGM1My00MTg4LWVkYjctNzg4MWJiNDQzNTZmIiwNCiAgImFpZCI6ICIwIiwNCiAgInNjeSI6ICJhdXRvIiwNCiAgIm5ldCI6ICJ3cyIsDQogICJ0eXBlIjogIm5vbmUiLA0KICAiaG9zdCI6ICJyaWNoZm9ja3MudG9wIiwNCiAgInBhdGgiOiAiLzBhZWJiYzg4LThjNTMtNDE4OC1lZGI3LTc4ODFiYjQ0MzU2ZiIsDQogICJ0bHMiOiAidGxzIiwNCiAgInNuaSI6ICIiLA0KICAiYWxwbiI6ICIiDQp9

    {
      "v": "2",
      "ps": "test2",
      "add": "158.101.150.139",
      "port": "443",
      "id": "0aebbc88-8c53-4188-edb7-7881bb44356f",
      "aid": "0",
      "scy": "auto",
      "net": "ws",
      "type": "none",
      "host": "richfocks.top",
      "path": "/0aebbc88-8c53-4188-edb7-7881bb44356f",
      "tls": "tls",
      "sni": "",
      "alpn": ""
    }

     */
    public async Task Create(List<string>? ips)
    {
        var xuiJsonStr = await File.ReadAllTextAsync("./Data/XUI/user.json");
        var listXuiUser = JsonSerializer.Deserialize<List<XuiUserConfig>>(xuiJsonStr);


        foreach (var userConfig in listXuiUser)
        {
            var listVmess = new List<string>();
            // 直连规则添加
            var defaultVmess = new VmessModel()
            {
                v = "2",
                ps = $"{userConfig.User}-美国-直连",
                add = ProxyDomain,
                port = "443",
                id = userConfig.Id,
                aid = "0",
                scy = "auto",
                net = "ws",
                type = "none",
                host = string.Empty,
                path = userConfig.Path,
                tls = "tls",
                sni = string.Empty,
                alpn = string.Empty
            };

            listVmess.Add(ConvertToVmessBase64Protol(defaultVmess));

            // 代理
            if (ips is not null)
            {
                var dicIp = await GetIpLocation(ips);

                foreach (var ip in ips)
                {
                    var location = dicIp[ip] == string.Empty ? "跳转" : dicIp[ip];


                    var vmessCf = new VmessModel()
                    {
                        v = "2",
                        ps = $"{userConfig.User}|{location}",
                        add = ip.Trim(),
                        port = "443",
                        id = userConfig.Id,
                        aid = "0",
                        scy = "auto",
                        net = "ws",
                        type = "none",
                        host = ProxyDomain,
                        path = userConfig.Path,
                        tls = "tls",
                        sni = string.Empty,
                        alpn = string.Empty
                    };
                    listVmess.Add(ConvertToVmessBase64Protol(vmessCf));
                }
            }

            await File.WriteAllLinesAsync($"./Data/Subscribe/{userConfig.User.ToLower()}-vmess.txt", listVmess);


            var sb = new StringBuilder();
            listVmess.ForEach(x => sb.AppendLine(x));
            var originalBytes = Encoding.UTF8.GetBytes(sb.ToString());
            var vmessProtobase64Str = Convert.ToBase64String(originalBytes);
            await File.WriteAllTextAsync($"./Data/Subscribe/{userConfig.User.ToLower()}.txt", vmessProtobase64Str);
        }
    }

    public async Task<string> Get(string id)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            throw new ArgumentException("请输入对应的ID");
        }
        var filePath = $"./Data/Subscribe/{id.ToLower()}.txt";

        if (!File.Exists(filePath))
        {
            throw new ArgumentException("ID无效");
        }

        return await File.ReadAllTextAsync(filePath);
    }


    /// <summary>
    /// 获取IP所在地
    /// </summary>
    /// <param name="ips"></param>
    /// <returns></returns>
    private async Task<Dictionary<string, string>> GetIpLocation(List<string> ips)
    {
        //http://ip-api.com/json/150.230.221.227?lang=zh-CN


        var dic = new Dictionary<string, string>();

        foreach (var ip in ips)
        {
            var client = _httpClientFactory.CreateClient("GetIPLocation");
            var url = $"http://ip-api.com/json/{ip}?lang=zh-CN";
            var result = await client.GetStringAsync(url);

            var jsonDoc = JsonDocument.Parse(result);
            var country = jsonDoc.RootElement.GetProperty("country").GetString();
            var city = jsonDoc.RootElement.GetProperty("city").GetString();
            if (!string.IsNullOrEmpty(country) || !string.IsNullOrEmpty(city))
            {
                dic.Add(ip, $"{country}-{city}");
            }
            else
            {
                dic.Add(ip, "");
            }
        }

        return dic;
    }

    /// <summary>
    /// Vmess 实体转换为Vmess Base64加密后的协议
    /// </summary>
    /// <param name="model"></param>
    /// <returns></returns>
    private string ConvertToVmessBase64Protol(VmessModel model)
    {
        try
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            var originalBytes = Encoding.GetEncoding("GB2312").GetBytes(JsonSerializer.Serialize(model));
            var base64String = Convert.ToBase64String(originalBytes);

            var vmessProtol = $"vmess://{base64String}";
            return vmessProtol;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }
}