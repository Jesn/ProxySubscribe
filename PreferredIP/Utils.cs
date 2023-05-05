using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace PreferredIP;

public static class Utils
{
    /// <summary>
    /// 获取对应平台 CloudflareSpeedTest 文件路径
    /// </summary>
    /// <returns></returns>
    public static string GetCloudflareSpeedTestPath()
    {
        var path = "./tool";
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            // windows
            path += "/windows";
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            path += "/linux";
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            // macos
            path += "/mac";
        }

        switch (RuntimeInformation.OSArchitecture)
        {
            // linux 
            case Architecture.Arm:
            case Architecture.Arm64:
                path += "/arm/CloudflareST";
                break;
            case Architecture.X64:
            case Architecture.X86:
                path += "/amd/CloudflareST";
                break;
        }

        if (path.Contains("windows"))
        {
            path += ".exe";
        }

        return path;
    }

    /// <summary>
    /// CIDR 效验
    /// </summary>
    /// <param name="cidr"></param>
    /// <returns></returns>
    public static bool CidrValid(string cidr)
    {
        string[] cidrParts = cidr.Split('/');
        if (cidrParts.Length != 2)
        {
            Console.WriteLine("Invalid CIDR format: " + cidr);
            return false;
        }

        IPAddress? ip;
        if (!IPAddress.TryParse(cidrParts[0], out ip))
        {
            Console.WriteLine("Invalid IP address: " + cidrParts[0]);
            return false;
        }

        if (!int.TryParse(cidrParts[1], out var prefixLength))
        {
            Console.WriteLine("Invalid prefix length: " + cidrParts[1]);
            return false;
        }

        if (prefixLength is < 0 or > 32)
        {
            Console.WriteLine("Invalid prefix length: " + prefixLength);
            return false;
        }

        Console.WriteLine("CIDR is valid: " + cidr);
        return true;
    }
}