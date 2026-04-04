using System;
using System.Buffers;
using System.Runtime.CompilerServices;
using System.Text;

public static class Bech32m
{
    private const string Charset = "qpzry9x8gf2tvdw0s3jn54khce6mua7l";
    private const uint M = 0x2be810b3;

    // .NET 9 SearchValues для сверхбыстрой проверки алфавита
    private static readonly SearchValues<char> ValidChars = SearchValues.Create(Charset + Charset.ToUpperInvariant());

    private static readonly sbyte[] ReverseCharset = CreateReverseCharset();

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static uint PolyMod(ReadOnlySpan<byte> values)
    {
        uint chk = 1;
        foreach (byte v in values)
        {
            uint top = chk >> 25;
            chk = (chk & 0x1ffffff) << 5 ^ v;
            // Развернутый цикл для JIT-оптимизации
            if ((top & 1) != 0) chk ^= 0x3b6a57b2;
            if ((top & 2) != 0) chk ^= 0x26508e6d;
            if ((top & 4) != 0) chk ^= 0x1ea119fa;
            if ((top & 8) != 0) chk ^= 0x3d4233dd;
            if ((top & 16) != 0) chk ^= 0x2a1462b3;
        }
        return chk;
    }

	public static string Encode(string hrp, ReadOnlySpan<byte> data)
	{
		ArgumentException.ThrowIfNullOrEmpty(hrp);

		int wordsLen = (data.Length * 8 + 4) / 5;
		int totalBufLen = (hrp.Length * 2 + 1) + wordsLen + 6;

		// 1. Подготовка 5-битных слов
		byte[] wordsArray = ArrayPool<byte>.Shared.Rent(wordsLen);
		try
		{
			ConvertBits(data, 8, 5, true, wordsArray.AsSpan(0, wordsLen));

			// 2. Подготовка буфера для PolyMod (используем stackalloc, это законно)
			Span<byte> combined = stackalloc byte[totalBufLen];
			ExpandHrp(hrp, combined[..(hrp.Length * 2 + 1)]);
			wordsArray.AsSpan(0, wordsLen).CopyTo(combined.Slice(hrp.Length * 2 + 1, wordsLen));

			uint mod = PolyMod(combined) ^ M;

			int stringLen = hrp.Length + 1 + wordsLen + 6;

			// 3. string.Create требует, чтобы State не содержал Span
			// Мы передаем hrp, копию words (через массив) и mod
			return string.Create(stringLen, (hrp, wordsLen, wordsArray, mod),	static (chars, state) =>
																				{
																					var (hrpStr, wLen, wArray, checksumMod) = state;
																					ReadOnlySpan<byte> wSpan = wArray.AsSpan(0, wLen);

																					// Заполняем HRP
																					int pos = hrpStr.AsSpan().ToLowerInvariant(chars);
            
																					// Разделитель
																					chars[pos++] = '1';

																					// Данные
																					for (int i = 0; i < wSpan.Length; i++)
																					{
																						chars[pos++] = Charset[wSpan[i]];
																					}

																					// Контрольная сумма (6 символов)
																					for (int i = 0; i < 6; i++)
																					{
																						chars[pos++] = Charset[(int)((checksumMod >> (5 * (5 - i))) & 31)];
																					}
																				});
		}
		finally
		{
			ArrayPool<byte>.Shared.Return(wordsArray);
		}
	}

    public static byte[] Decode(string expectedHrp, string bechString)
    {
        // 1. Валидация входной строки через SearchValues
        if (string.IsNullOrEmpty(bechString) || bechString.Length < 8)
            throw new FormatException("Invalid length");

        int sepPos = bechString.LastIndexOf('1');
        if (sepPos < 1 || sepPos + 7 > bechString.Length)
            throw new FormatException("Invalid separator position");

        ReadOnlySpan<char> hrpPart = bechString.AsSpan(0, sepPos);
        if (!hrpPart.Equals(expectedHrp, StringComparison.OrdinalIgnoreCase))
            throw new FormatException("HRP mismatch");

        ReadOnlySpan<char> dataPart = bechString.AsSpan(sepPos + 1);
        
        // Проверка на недопустимые символы за O(1) на символ
        int invalidIdx = dataPart.IndexOfAnyExcept(ValidChars);
        if (invalidIdx != -1) throw new FormatException($"Invalid character at {invalidIdx}");

        Span<byte> values = stackalloc byte[dataPart.Length];
        for (int i = 0; i < dataPart.Length; i++)
        {
            values[i] = (byte)ReverseCharset[dataPart[i]];
        }

        // 2. Проверка контрольной суммы
        int hrpExtLen = hrpPart.Length * 2 + 1;
        Span<byte> combined = stackalloc byte[hrpExtLen + values.Length];
        ExpandHrp(hrpPart, combined[..hrpExtLen]);
        values.CopyTo(combined[hrpExtLen..]);

        if (PolyMod(combined) != M) throw new FormatException("Checksum mismatch (Bech32m)");

        // 3. Конвертация 5->8 с Zero-filling
        int outLen = (values.Length - 6) * 5 / 8;
        byte[] result = new byte[outLen];
        ConvertBits(values[..(values.Length - 6)], 5, 8, false, result);

        return result;
    }

    private static void ConvertBits(ReadOnlySpan<byte> data, int fromBits, int toBits, bool pad, Span<byte> result)
    {
        int acc = 0, bits = 0, pos = 0;
        int maxv = (1 << toBits) - 1;
        int max_acc = (1 << (fromBits + toBits - 1)) - 1;

        foreach (byte value in data)
        {
            acc = ((acc << fromBits) | value) & max_acc;
            bits += fromBits;
            while (bits >= toBits)
            {
                bits -= toBits;
                result[pos++] = (byte)((acc >> bits) & maxv);
            }
        }

        if (pad)
        {
            if (bits > 0) result[pos++] = (byte)((acc << (toBits - bits)) & maxv);
        }
        else if (bits >= fromBits || ((acc << (toBits - bits)) & maxv) != 0)
        {
            throw new FormatException("Zero-filling error: trailing non-zero bits");
        }
    }

    private static void ExpandHrp(ReadOnlySpan<char> hrp, Span<byte> output)
    {
        for (int i = 0; i < hrp.Length; i++) output[i] = (byte)(hrp[i] >> 5);
        output[hrp.Length] = 0;
        for (int i = 0; i < hrp.Length; i++) output[hrp.Length + 1 + i] = (byte)(hrp[i] & 31);
    }

    private static sbyte[] CreateReverseCharset()
    {
        sbyte[] rev = new sbyte[128];
        Array.Fill(rev, (sbyte)-1);
        for (int i = 0; i < Charset.Length; i++)
        {
            rev[Charset[i]] = (sbyte)i;
            rev[char.ToUpperInvariant(Charset[i])] = (sbyte)i;
        }
        return rev;
    }
}