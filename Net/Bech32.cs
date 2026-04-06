namespace Uccs.Net;

using System;
using System.Buffers;
using System.Collections.Generic;
using System.Text;

public static class Bech32
{
    public const int TagLength = 4;
    private const string Charset = "qpzry9x8gf2tvdw0s3jn54khce6mua7l";
    private static readonly SearchValues<char> Alphanumeric = SearchValues.Create("abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789");
    private static readonly SearchValues<char> Bech32Chars = SearchValues.Create(Charset);
    private const uint Bech32mConst = 0x2bc830a3;

    public static string Encode(ReadOnlySpan<byte> data, string? tag = null)
    {
        bool useTag = !string.IsNullOrEmpty(tag);
        if(useTag)
        {
            if(tag!.Length != TagLength)
            {
                throw new ArgumentException($"Tag must be {TagLength} characters.");
            }
            if(tag.AsSpan().ContainsAnyExcept(Alphanumeric))
            {
                throw new ArgumentException("Tag must be [a-zA-Z0-9].");
            }
        }

        int base32DataLength = (data.Length * 8 + 4) / 5;
        int tagOffset = useTag ? TagLength : 0;
        byte[] combined = new byte[tagOffset + base32DataLength];

        if(useTag)
        {
            for(int i = 0; i < TagLength; i++)
            {
                combined[i] = (byte)(char.ToLowerInvariant(tag![i]) & 31);
            }
        }

        ConvertBits(data, combined.AsSpan(tagOffset), 8, 5, true);

        return string.Create(combined.Length + 6, (combined, tag), (dest, state) =>
        {
            var (bytes, originalTag) = state;
            if(!string.IsNullOrEmpty(originalTag))
            {
                originalTag.AsSpan().CopyTo(dest);
            }

            int offset = originalTag?.Length ?? 0;
            for(int i = offset; i < bytes.Length; i++)
            {
                dest[i] = Charset[bytes[i]];
            }

            Span<byte> cs = stackalloc byte[6];
            CreateChecksum(bytes, cs);
            for(int i = 0; i < 6; i++)
            {
                dest[bytes.Length + i] = Charset[cs[i]];
            }
        });
    }

    public static bool TryDecode(ReadOnlySpan<char> encoded, bool hasTag, out byte[] data, out string? tag)
    {
        data = [];
        tag = null;
        int minLen = (hasTag ? TagLength : 0) + 6;
        if(encoded.Length < minLen)
        {
            return false;
        }

        if(hasTag && encoded[..TagLength].ContainsAnyExcept(Alphanumeric))
        {
            return false;
        }
        if(encoded[(hasTag ? TagLength : 0)..].ContainsAnyExcept(Bech32Chars))
        {
            return false;
        }

        Span<byte> values = encoded.Length < 1024 ? stackalloc byte[encoded.Length] : new byte[encoded.Length];
        if(hasTag)
        {
            tag = encoded[..TagLength].ToString();
            for(int i = 0; i < TagLength; i++)
            {
                values[i] = (byte)(char.ToLowerInvariant(encoded[i]) & 31);
            }
        }

        for(int i = (hasTag ? TagLength : 0); i < encoded.Length; i++)
        {
            int idx = Charset.IndexOf(encoded[i]);
            if(idx == -1)
            {
                return false;
            }
            values[i] = (byte)idx;
        }

        if(PolyMod(values) != Bech32mConst)
        {
            return false;
        }

        ReadOnlySpan<byte> data5Bit = values[(hasTag ? TagLength : 0)..^6];
        int outputLen = (data5Bit.Length * 5) / 8;
        data = new byte[outputLen];
        ConvertBits(data5Bit, data, 5, 8, false);
        return true;
    }

    private static uint PolyMod(ReadOnlySpan<byte> values)
    {
        uint chk = 1;
        foreach(byte v in values)
        {
            uint top = chk >> 25;
            chk = ((chk & 0x1ffffff) << 5) ^ v;
            if((top & 1) != 0)
            {
                chk ^= 0x3b6a57b2;
            }
            if((top & 2) != 0)
            {
                chk ^= 0x26508e6d;
            }
            if((top & 4) != 0)
            {
                chk ^= 0x1ea119fa;
            }
            if((top & 8) != 0)
            {
                chk ^= 0x3d4233dd;
            }
            if((top & 16) != 0)
            {
                chk ^= 0x2a1462b3;
            }
        }
        return chk;
    }

    private static void CreateChecksum(ReadOnlySpan<byte> data, Span<byte> destination)
    {
        byte[] values = new byte[data.Length + 6];
        data.CopyTo(values);
        uint mod = PolyMod(values) ^ Bech32mConst;
        for(int i = 0; i < 6; i++)
        {
            destination[i] = (byte)((mod >> (5 * (5 - i))) & 31);
        }
    }

    private static void ConvertBits(ReadOnlySpan<byte> input, Span<byte> output, int from, int to, bool pad)
    {
        int acc = 0, bits = 0, outIdx = 0;
        int maxv = (1 << to) - 1;
        foreach(byte b in input)
        {
            acc = (acc << from) | b;
            bits += from;
            while(bits >= to)
            {
                bits -= to;
                if(outIdx < output.Length)
                {
                    output[outIdx++] = (byte)((acc >> bits) & maxv);
                }
            }
        }
        if(pad && bits > 0 && outIdx < output.Length)
        {
            output[outIdx] = (byte)((acc << (to - bits)) & maxv);
        }
    }
}