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
                alpn = string.Empty,
                fp = string.Empty
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

    public async Task<string> Get(string id, string type = "")
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            throw new ArgumentException("请输入对应的ID");
        }

        var filePath = $"./Data/Subscribe/{id.ToLower()}.txt";
        if (!string.IsNullOrWhiteSpace(type) && type.ToLower().Equals("vmess"))
        {
            filePath = $"./Data/Subscribe/{id.ToLower()}-vmess.txt";
        }

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

            // https://ip125.com/api/154.53.56.159?lang=zh-CN

            // var url = $"http://ip-api.com/json/{ip}?lang=zh-CN";

            var listUrl = new List<string>()
            {
                $"http://ip-api.com/json/{ip}?lang=zh-CN",
                $"https://ip125.com/api/{ip}?lang=zh-CN"
            };
            var random = new Random();
            var index = random.Next(listUrl.Count);
            var url = listUrl[index];

            
            
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