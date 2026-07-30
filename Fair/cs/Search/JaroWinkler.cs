namespace Uccs.Fair;

using System;
using System.Buffers;
using System.Runtime.CompilerServices;

public class JaroWinkler : IMetric<string>
{
    private const int Scale = 10_000; // 10000 = 100.00% (0% дистанции)
    private const int DefaultScalingFactor = 1000; // 0.1 * Scale
    private const int MaxPrefixLength = 8;
    private const int StackAllocThreshold = 256; // Порог для выделения памяти на стекe

    /// <summary>
    /// Вычисляет дистанцию (несоответствие) между строками от 0 до 100,
    /// где 0 — полное совпадение, 100 — строки абсолютно разные.
    /// </summary>
    public int ComputeDistance(string a, string b)
    {
        int similarityFixed = GetSimilarityFixed(a, b);
        int distanceFixed = Scale - similarityFixed;

        // Округление до ближайшего целого процента [0..100]
        return (distanceFixed + 50) / 100;
    }

    /// <summary>
    /// Возвращает сходство в масштабе [0..10000], где 10000 = 100%
    /// </summary>
    public static int GetSimilarityFixed(ReadOnlySpan<char> s1, ReadOnlySpan<char> s2)
    {
        if (s1.IsEmpty || s2.IsEmpty) return 0;
        if (s1.Equals(s2, StringComparison.OrdinalIgnoreCase)) return Scale;

        int len1 = s1.Length;
        int len2 = s2.Length;

        int jaroDistance = GetJaroDistanceFixed(s1, s2, len1, len2);

        if (jaroDistance < 7000) return jaroDistance;

        int prefixLength = 0;
        int maxPrefix = Math.Min(MaxPrefixLength, Math.Min(len1, len2));

        for (int i = 0; i < maxPrefix; i++)
        {
            if (char.ToLowerInvariant(s1[i]) == char.ToLowerInvariant(s2[i])) 
                prefixLength++;
            else 
                break;
        }

        int remainder = Scale - jaroDistance;
        int bonus = (prefixLength * DefaultScalingFactor * remainder) / Scale;

        return Math.Min(jaroDistance + bonus, Scale);
    }

    private static int GetJaroDistanceFixed(ReadOnlySpan<char> s1, ReadOnlySpan<char> s2, int len1, int len2)
    {
        int matchWindow = Math.Max(0, (Math.Max(len1, len2) / 2) - 1);

        // Буферы для s1Matches и s2Matches без выделения памяти в куче (для строк <= 256 символов)
        bool[]? rented1 = len1 > StackAllocThreshold ? ArrayPool<bool>.Shared.Rent(len1) : null;
        bool[]? rented2 = len2 > StackAllocThreshold ? ArrayPool<bool>.Shared.Rent(len2) : null;

        Span<bool> s1Matches = rented1 != null ? rented1.AsSpan(0, len1) : stackalloc bool[len1];
        Span<bool> s2Matches = rented2 != null ? rented2.AsSpan(0, len2) : stackalloc bool[len2];

        // Очищаем Span перед использованием
        s1Matches.Clear();
        s2Matches.Clear();

        try
        {
            int matches = 0;
            int transpositions = 0;

            for (int i = 0; i < len1; i++)
            {
                char c1 = char.ToLowerInvariant(s1[i]);
                int start = Math.Max(0, i - matchWindow);
                int end = Math.Min(i + matchWindow + 1, len2);

                for (int j = start; j < end; j++)
                {
                    if (s2Matches[j]) continue;
                    if (c1 != char.ToLowerInvariant(s2[j])) continue;

                    s1Matches[i] = true;
                    s2Matches[j] = true;
                    matches++;
                    break;
                }
            }

            if (matches == 0) return 0;

            int k = 0;
            for (int i = 0; i < len1; i++)
            {
                if (!s1Matches[i]) continue;

                while (k < len2 && !s2Matches[k]) k++;

                if (k < len2)
                {
                    if (char.ToLowerInvariant(s1[i]) != char.ToLowerInvariant(s2[k])) 
                        transpositions++;
                    
                    k++;
                }
            }

            int term1 = (matches * Scale) / len1;
            int term2 = (matches * Scale) / len2;
            int term3 = ((2 * matches - transpositions) * Scale) / (2 * matches);

            return (term1 + term2 + term3) / 3;
        }
        finally
        {
            // Возвращаем массивы в пул, если они арендавались
            if (rented1 != null) ArrayPool<bool>.Shared.Return(rented1);
            if (rented2 != null) ArrayPool<bool>.Shared.Return(rented2);
        }
    }
}