namespace Uccs.Net;

using System;
using System.Buffers;

public static class Bech32
{
    public const int MaxTagLength = 31; 
    private const string Charset = "qpzry9x8gf2tvdw0s3jn54khce6mua7l";
    public static readonly SearchValues<char> Alphanumeric = SearchValues.Create("abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789");
    private static readonly SearchValues<char> Bech32Chars = SearchValues.Create(Charset + "QPZRY9X8GF2TVDW0S3JN54KHCE6MUA7L");
    private const uint Bech32mConst = 0x2bc830a3;

    public static string Encode(ReadOnlySpan<byte> data, string? tag = null)
    {
        tag ??= string.Empty;
        if (tag.Length > MaxTagLength)
            throw new ArgumentException($"Tag length must be between 0 and {MaxTagLength}.");
        
        if (tag.Length > 0 && tag.AsSpan().ContainsAnyExcept(Alphanumeric))
            throw new ArgumentException("Tag must be [a-zA-Z0-9].");

        int base32DataLength = (data.Length * 8 + 4) / 5;
        // Структура значений: [Tag5Bit] + [Data5Bit] + [TagLenByte]
        byte[] values = new byte[tag.Length + base32DataLength + 1];

        // 1. Кодируем тег для чексуммы (нижний регистр)
        for (int i = 0; i < tag.Length; i++)
        {
            values[i] = (byte)(char.ToLowerInvariant(tag[i]) & 31);
        }

        // 2. Кодируем данные в 5-бит
        ConvertBits(data, values.AsSpan(tag.Length, base32DataLength), 8, 5, true);

        // 3. Добавляем длину тега как последний байт перед чексуммой
        values[^1] = (byte)tag.Length;

        return string.Create(values.Length + 6, (values, tag), (dest, state) =>
        {
            var (vals, originalTag) = state;
            
            // Записываем тег "как есть" (сохраняя оригинальный регистр)
            if (originalTag.Length > 0)
                originalTag.AsSpan().CopyTo(dest);

            // Записываем данные и символ длины
            int offset = originalTag.Length;
            for (int i = offset; i < vals.Length; i++)
            {
                dest[i] = Charset[vals[i]];
            }

            // 4. Расчет контрольной суммы (Implementation 3: No extra allocation)
            uint mod = PolyMod(vals, true) ^ Bech32mConst;
            for (int i = 0; i < 6; i++)
            {
                dest[vals.Length + i] = Charset[(int)((mod >> (5 * (5 - i))) & 31)];
            }
        });
    }

    public static bool TryDecode(ReadOnlySpan<char> encoded, out byte[] data, out string? tag)
    {
        data = [];
        tag = null;

        // Минимально: LenChar(1) + Checksum(6) = 7 символов
        if (encoded.Length < 7) return false;

        // Извлекаем символ длины (7-й с конца)
        char tagLenChar = encoded[^7]; 
        int tagLen = Charset.IndexOf(char.ToLowerInvariant(tagLenChar));
        
        // Проверка корректности длины и физического наличия тега в строке
        if (tagLen == -1 || encoded.Length < tagLen + 7) return false;

        // Implementation 1: Проверка регистра (только для части после тега)
        var dataPart = encoded[tagLen..];
        bool hasUpper = false, hasLower = false;
        foreach (char c in dataPart)
        {
            if (char.IsUpper(c)) hasUpper = true;
            if (char.IsLower(c)) hasLower = true;
            if (hasUpper && hasLower) return false;
        }

        // Проверка допустимых символов
        if (tagLen > 0 && encoded[..tagLen].ContainsAnyExcept(Alphanumeric)) return false;
        if (dataPart.ContainsAnyExcept(Bech32Chars)) return false;

        // Собираем массив 5-битных значений
        Span<byte> values = encoded.Length < 1024 ? stackalloc byte[encoded.Length] : new byte[encoded.Length];
        
        if (tagLen > 0)
        {
            tag = encoded[..tagLen].ToString();
            for (int i = 0; i < tagLen; i++)
                values[i] = (byte)(char.ToLowerInvariant(encoded[i]) & 31);
        }
        else tag = string.Empty;

        for (int i = tagLen; i < encoded.Length; i++)
        {
            int idx = Charset.IndexOf(char.ToLowerInvariant(encoded[i]));
            if (idx == -1) return false;
            values[i] = (byte)idx;
        }

        // Проверка PolyMod
        if (PolyMod(values) != Bech32mConst) return false;

        // Извлекаем данные (от конца тега до символа длины)
        ReadOnlySpan<byte> data5Bit = values[tagLen..^7];
        int outputLen = (data5Bit.Length * 5) / 8;
        data = new byte[outputLen];
        ConvertBits(data5Bit, data, 5, 8, false);
        
        return true;
    }

    private static uint PolyMod(ReadOnlySpan<byte> values, bool addVirtualZeroSuffix = false)
    {
        uint chk = 1;
        foreach (byte v in values) chk = PolyModStep(chk, v);
        if (addVirtualZeroSuffix)
        {
            for (int i = 0; i < 6; i++) chk = PolyModStep(chk, 0);
        }
        return chk;
    }

    private static uint PolyModStep(uint chk, byte v)
    {
        uint top = chk >> 25;
        chk = ((chk & 0x1ffffff) << 5) ^ v;
        if ((top & 1) != 0)  chk ^= 0x3b6a57b2;
        if ((top & 2) != 0)  chk ^= 0x26508e6d;
        if ((top & 4) != 0)  chk ^= 0x1ea119fa;
        if ((top & 8) != 0)  chk ^= 0x3d4233dd;
        if ((top & 16) != 0) chk ^= 0x2a1462b3;
        return chk;
    }

    private static void ConvertBits(ReadOnlySpan<byte> input, Span<byte> output, int from, int to, bool pad)
    {
        int acc = 0, bits = 0, outIdx = 0;
        int maxv = (1 << to) - 1;
        foreach (byte b in input)
        {
            acc = (acc << from) | b;
            bits += from;
            while (bits >= to)
            {
                bits -= to;
                if (outIdx < output.Length)
                    output[outIdx++] = (byte)((acc >> bits) & maxv);
            }
        }
        if (pad && bits > 0 && outIdx < output.Length)
            output[outIdx] = (byte)((acc << (to - bits)) & maxv);
    }
}