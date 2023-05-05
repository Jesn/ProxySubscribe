using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace PreferredIP.aws;

public class Cloudfront
{
    /// <summary>
    /// 下载aws 所有CDN ip段，并且整理IP
    /// </summary>
    public async Task DownIps()
    {
        var url = "https://ip-ranges.amazonaws.com/ip-ranges.json";

        var httpClient = new HttpClient();
        var response = await httpClient.GetAsync(url);
        if (!response.IsSuccessStatusCode)
        {
            return;
        }

        var content = await response.Content.ReadAsStringAsync();

        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };
        var awsIpRanges = JsonSerializer.Deserialize<AWSIPRanges>(content, options);

        var listIp4 = new List<string>();
        var listIp6 = new List<string>();


        foreach (var prefix in awsIpRanges.Prefixes)
        {
            if (Utils.CidrValid(prefix.Ip_prefix))
                listIp4.Add(prefix.Ip_prefix);
        }

        foreach (var prefix in awsIpRanges.Ipv6_Prefixes)
        {
            if (Utils.CidrValid(prefix.Ipv6_prefix))
                listIp6.Add(prefix.Ipv6_prefix);
        }

        string dirPath = Directory.GetCurrentDirectory();
        string filePath = Path.Combine(dirPath, "filename.txt");

        var path4 = Path.Combine(dirPath, $"./data/source/aws/ip4-list.txt");
        if (!File.Exists(path4))
        {
            File.Create(path4);
        }

        var path6 = Path.Combine(dirPath, $"./data/source/aws/ip6-list.txt");
        if (!File.Exists(path6))
        {
            File.Create(path6);
        }

        await File.WriteAllLinesAsync(path4, listIp4);
        await File.WriteAllLinesAsync(path6, listIp6);
    }

    /// <summary>
    /// 优选IP
    /// </summary>
    public async Task FilterIp()
    {
        // CloudflareSpeedTest 参数设置参考： https://github.com/XIU2/CloudflareSpeedTest
        var cftStartInfo = new ProcessStartInfo
        {
            FileName = Utils.GetCloudflareSpeedTestPath(),
            //Arguments = $"-f ./data/source/aws/ip4-list.txt -url https://speed.cloudflare.com/__down?bytes=800000000",
            Arguments = $" -f ./data/source/aws/ip4-list.txt -p 20 -o ./data/output/cft-result.csv -dd",
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true
        };
        using var process = new Process();
        process.StartInfo = cftStartInfo;

        process.Start();
        Console.WriteLine("开始测速。。。。");

        // 读取输出
        var output = await process.StandardOutput.ReadToEndAsync();
        var error = await process.StandardError.ReadToEndAsync();
        // 打印输出和错误信息
        Console.WriteLine("Output:");
        Console.WriteLine(output);

        Console.WriteLine("Error:");
        Console.WriteLine(error);
        // 等待进程结束
        await process.WaitForExitAsync();
        process.Close();


        Console.WriteLine("Press any key to exit...");
        Console.ReadKey();
    }
}