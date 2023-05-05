namespace PreferredIP.aws;

public class AWSIPRanges
{
    public string SyncToken { get; set; }
    public string CreateDate { get; set; }
    public List<IPPrefix> Prefixes { get; set; }
    
    public List<IPPrefix> Ipv6_Prefixes { get; set; }

    public class IPPrefix
    {
        public string Ip_prefix { get; set; }
        public string Ipv6_prefix { get; set; }
        public string Region { get; set; }
        public string Service { get; set; }
        public string NetworkBorderGroup { get; set; }
    }
}