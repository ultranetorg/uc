namespace Uccs.Fair;

using System;

public class JaroWinkler : IMetric<string>
{
    private const int Scale = 10_000; // 10000 = 100.00% (0% дистанции)
    private const int DefaultScalingFactor = 1000; // 0.1 * Scale
    private const int MaxPrefixLength = 4;

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
    public static int GetSimilarityFixed(string s1, string s2)
    {
        if (s1 == null || s2 == null) return 0;
        if (s1 == s2) return Scale;

        s1 = s1.ToLowerInvariant();
        s2 = s2.ToLowerInvariant();

        int len1 = s1.Length;
        int len2 = s2.Length;

        if (len1 == 0 || len2 == 0) return 0;

        int jaroDistance = GetJaroDistanceFixed(s1, s2, len1, len2);

        if (jaroDistance < 7000) return jaroDistance;

        int prefixLength = 0;
        int maxPrefix = Math.Min(MaxPrefixLength, Math.Min(len1, len2));

        for (int i = 0; i < maxPrefix; i++)
        {
            if (s1[i] == s2[i]) prefixLength++;
            else break;
        }

        int remainder = Scale - jaroDistance;
        int bonus = (prefixLength * DefaultScalingFactor * remainder) / Scale;
        
        return Math.Min(jaroDistance + bonus, Scale);
    }

    private static int GetJaroDistanceFixed(string s1, string s2, int len1, int len2)
    {
        int matchWindow = Math.Max(0, (Math.Max(len1, len2) / 2) - 1);

        bool[] s1Matches = new bool[len1];
        bool[] s2Matches = new bool[len2];

        int matches = 0;
        int transpositions = 0;

        for (int i = 0; i < len1; i++)
        {
            int start = Math.Max(0, i - matchWindow);
            int end = Math.Min(i + matchWindow + 1, len2);

            for (int j = start; j < end; j++)
            {
                if (s2Matches[j] || s1[i] != s2[j]) continue;

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
                if (s1[i] != s2[k]) transpositions++;
                k++;
            }
        }

        int term1 = (matches * Scale) / len1;
        int term2 = (matches * Scale) / len2;
        int term3 = ((2 * matches - transpositions) * Scale) / (2 * matches);

        return (term1 + term2 + term3) / 3;
    }
}