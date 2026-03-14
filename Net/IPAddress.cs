using System.Net;

namespace Uccs.Net;

public class IPAddressComparer : IEqualityComparer<IPAddress>
{
    public bool Equals(IPAddress? x, IPAddress? y)
    {
        if(ReferenceEquals(x, y))
			return true;
        
		if(x is null || y is null)
			return false;

        if(x.AddressFamily != y.AddressFamily)
            return x.MapToIPv6().GetAddressBytes().SequenceEqual(y.MapToIPv6().GetAddressBytes());

        return x.Equals(y);
    }

    public int GetHashCode(IPAddress obj)
    {
        // Forcing to IPv6 ensures consistent hashes for mapped addresses
        return obj.MapToIPv6().GetHashCode();
    }
}