namespace Uccs.Fair;

using System;
using System.Buffers;

public class JaroWinkler : IMetric<string> 
{
    private const int Scale = 10_000; // 100.00%
    private const int DefaultScalingFactor = 1000; // 0.1 * Scale (бонус Винклера)
    private const int MaxPrefixLength = 4;
    private const int StackAllocThreshold = 256;

    public int ComputeDistance(string a, string b)
    {
        int similarityFixed = GetSimilarityFixed(a, b);
        int distanceFixed = Scale - similarityFixed;

        // Округление до ближайшего целого процента [0..100]
        return (distanceFixed + 50) / 100;
    }

    public static int GetSimilarityFixed(ReadOnlySpan<char> s1, ReadOnlySpan<char> s2)
    {
        if (s1.Equals(s2, StringComparison.OrdinalIgnoreCase))
            return Scale;

        if (s1.IsEmpty || s2.IsEmpty)
            return 0;

        int len1 = s1.Length;
        int len2 = s2.Length;

        // Выделяем память под буферы совпадений и нормализованные символы
        bool[]? rentedBool1 = len1 > StackAllocThreshold ? ArrayPool<bool>.Shared.Rent(len1) : null;
        bool[]? rentedBool2 = len2 > StackAllocThreshold ? ArrayPool<bool>.Shared.Rent(len2) : null;
        char[]? rentedChar1 = len1 > StackAllocThreshold ? ArrayPool<char>.Shared.Rent(len1) : null;
        char[]? rentedChar2 = len2 > StackAllocThreshold ? ArrayPool<char>.Shared.Rent(len2) : null;

        Span<bool> s1Matches = rentedBool1 != null ? rentedBool1.AsSpan(0, len1) : stackalloc bool[len1];
        Span<bool> s2Matches = rentedBool2 != null ? rentedBool2.AsSpan(0, len2) : stackalloc bool[len2];
        Span<char> norm1 = rentedChar1 != null ? rentedChar1.AsSpan(0, len1) : stackalloc char[len1];
        Span<char> norm2 = rentedChar2 != null ? rentedChar2.AsSpan(0, len2) : stackalloc char[len2];

        s1Matches.Clear();
        s2Matches.Clear();

        try
        {
            // 1. Нормализуем регистр один раз для $O(N)$ вместо $O(N \times M)$
            for (int i = 0; i < len1; i++) norm1[i] = char.ToLowerInvariant(s1[i]);
            for (int i = 0; i < len2; i++) norm2[i] = char.ToLowerInvariant(s2[i]);

            int jaroDistance = GetJaroDistanceFixed(norm1, norm2, len1, len2, s1Matches, s2Matches);

            // Порог Винклера (0.7 / 7000)
            if (jaroDistance < 7000)
                return jaroDistance;

            // 2. Расчет префикса
            int prefixLength = 0;
            int maxPrefix = Math.Min(MaxPrefixLength, Math.Min(len1, len2));

            for (int i = 0; i < maxPrefix; i++)
            {
                if (norm1[i] == norm2[i])
                    prefixLength++;
                else
                    break;
            }

            int remainder = Scale - jaroDistance;
            int bonus = (prefixLength * DefaultScalingFactor * remainder) / Scale;

            return Math.Min(jaroDistance + bonus, Scale);
        }
        finally
        {
            if (rentedBool1 != null) ArrayPool<bool>.Shared.Return(rentedBool1);
            if (rentedBool2 != null) ArrayPool<bool>.Shared.Return(rentedBool2);
            if (rentedChar1 != null) ArrayPool<char>.Shared.Return(rentedChar1);
            if (rentedChar2 != null) ArrayPool<char>.Shared.Return(rentedChar2);
        }
    }

    private static int GetJaroDistanceFixed(
        ReadOnlySpan<char> s1, 
        ReadOnlySpan<char> s2, 
        int len1, 
        int len2,
        Span<bool> s1Matches,
        Span<bool> s2Matches)
    {
        int matchWindow = Math.Max(0, (Math.Max(len1, len2) / 2) - 1);
        int matches = 0;

        // Поиск совпадений
        for (int i = 0; i < len1; i++)
        {
            char c1 = s1[i];
            int start = Math.Max(0, i - matchWindow);
            int end = Math.Min(i + matchWindow + 1, len2);

            for (int j = start; j < end; j++)
            {
                if (s2Matches[j])
                    continue;

                if (c1 != s2[j])
                    continue;

                s1Matches[i] = true;
                s2Matches[j] = true;
                matches++;
                break;
            }
        }

        if (matches == 0)
            return 0;

        // Подсчет полу-транспозиций
        int halfTranspositions = 0;
        int k = 0;

        for (int i = 0; i < len1; i++)
        {
            if (!s1Matches[i])
                continue;

            while (k < len2 && !s2Matches[k])
                k++;

            if (k < len2)
            {
                if (s1[i] != s2[k])
                    halfTranspositions++;

                k++;
            }
        }

        int transpositions = halfTranspositions / 2;

        // Формула Джаро в Scale (10_000)
        long term1 = ((long)matches * Scale) / len1;
        long term2 = ((long)matches * Scale) / len2;
        long term3 = (((long)matches - transpositions) * Scale) / matches;

        return (int)((term1 + term2 + term3) / 3);
    }
}