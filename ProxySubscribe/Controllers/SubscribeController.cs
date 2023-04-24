using Microsoft.AspNetCore.Mvc;
using ProxySubscribe.Services.Protocols;

namespace ProxySubscribe.Controllers;

[ApiController]
[Route("[controller]")]
public class SubscribeController : Controller
{
    private readonly IProtocolAppService _protocolAppService;

    public SubscribeController(IProtocolAppService protocolAppService)
    {
        _protocolAppService = protocolAppService;
    }

    /// <summary>
    /// 更新订阅文件
    /// </summary>
    /// <param name="ips"></param>
    [HttpPost]
    public void CreateSubScribe(List<string> ips)
    {
        _protocolAppService.Create(ips);
    }

    /// <summary>
    /// 根据ID获取数据
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpGet]
    public async Task<string> Get(string id)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            return string.Empty;
        }

        return await _protocolAppService.Get(id);
    }
}