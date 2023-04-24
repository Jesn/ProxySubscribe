using Microsoft.AspNetCore.Http.HttpResults;

namespace ProxySubscribe.Services.Protocols;

public interface IProtocolAppService
{
    
    Task Create(List<string>? ips);

    Task<string> Get(string id, string type = "");
}