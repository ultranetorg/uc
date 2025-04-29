namespace Uccs.Fair;

public class NeedlemanWunsch : IMetric<string>
{
    static int matchScore = 1;
    static int mismatchPenalty = -1;
    static int gapPenalty = -2;

	public int ComputeDistance(string seq1, string seq2)
    {
		return ComputeAlignmentFast(seq1, seq2);
	}

	public static int ComputeAlignment(string seq1, string seq2)
    {
        int rows = seq1.Length + 1;
        int cols = seq2.Length + 1;
        int[,] scoreMatrix = new int[rows, cols];

        // Инициализация краев матрицы
        for (int i = 0; i < rows; i++)
            scoreMatrix[i, 0] = i * gapPenalty;
        for (int j = 0; j < cols; j++)
            scoreMatrix[0, j] = j * gapPenalty;

        // Заполнение матрицы
        for (int i = 1; i < rows; i++)
        {
            for (int j = 1; j < cols; j++)
            {
                int match = scoreMatrix[i - 1, j - 1] + (seq1[i - 1] == seq2[j - 1] ? matchScore : mismatchPenalty);
                int delete = scoreMatrix[i - 1, j] + gapPenalty;
                int insert = scoreMatrix[i, j - 1] + gapPenalty;
                scoreMatrix[i, j] = Math.Max(match, Math.Max(delete, insert));
            }
        }

        // Только расчет similarity
        int maxLen = Math.Max(seq1.Length, seq2.Length);
        int maxScore = maxLen * matchScore;

        return 100 - scoreMatrix[seq1.Length, seq2.Length] * 100 / maxScore;
    }

	public static int ComputeAlignmentFast(string seq1, string seq2)
    {
        int m = seq1.Length;
        int n = seq2.Length;

        if (m == 0 && n == 0)
            return 100; // Обе строки пустые — 100% совпадение

        int[] previousRow = new int[n + 1];
        int[] currentRow = new int[n + 1];

        // Инициализация первой строки
        for (int j = 0; j <= n; j++)
            previousRow[j] = j * gapPenalty;

        // Заполнение строк
        for (int i = 1; i <= m; i++)
        {
            currentRow[0] = i * gapPenalty;
            for (int j = 1; j <= n; j++)
            {
                int match = previousRow[j - 1] + (seq1[i - 1] == seq2[j - 1] ? matchScore : mismatchPenalty);
                int delete = previousRow[j] + gapPenalty;
                int insert = currentRow[j - 1] + gapPenalty;
                currentRow[j] = Math.Max(match, Math.Max(delete, insert));
            }
            // Перекидываем текущую строку в previousRow
            var temp = previousRow;
            previousRow = currentRow;
            currentRow = temp;
        }

        // Корректная нормализация
        int totalLength = Math.Max(m, n);
        int maxScore = totalLength * matchScore;

        int rawScore = previousRow[n];
        int similarity = 100 - (rawScore * 100) / maxScore;

        return similarity;
    }

    public static int ComputeGreedyAlignment(string seq1, string seq2)
    {
        int i = 0, j = 0;
        int score = 0;

        while (i < seq1.Length && j < seq2.Length)
        {
            if (seq1[i] == seq2[j])
            {
                score += matchScore;
                i++;
                j++;
            }
            else
            {
                // Выбираем наименьшее "наказание"
                int mismatch = mismatchPenalty;
                int gap1 = gapPenalty;
                int gap2 = gapPenalty;

                if (i + 1 < seq1.Length && seq1[i + 1] == seq2[j])
                {
                    // Пропускаем символ в seq1
                    score += gapPenalty;
                    i++;
                }
                else if (j + 1 < seq2.Length && seq1[i] == seq2[j + 1])
                {
                    // Пропускаем символ в seq2
                    score += gapPenalty;
                    j++;
                }
                else
                {
                    // Несовпадение
                    score += mismatchPenalty;
                    i++;
                    j++;
                }
            }
        }

        // Остаток пропусков
        score += (seq1.Length - i) * gapPenalty;
        score += (seq2.Length - j) * gapPenalty;

        // Нормализация
        int maxLen = Math.Max(seq1.Length, seq2.Length);
        int maxScore = maxLen * matchScore;

        return 100 - (score * 100) / maxScore;
    }
}
