namespace Uccs.Fair;

public class NeedlemanWunsch
{
    static int matchScore = 1;
    static int mismatchPenalty = -1;
    static int gapPenalty = -2;

    public static int NeedlemanWunschAlignment(string seq1, string seq2)
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

        // Обратный путь (Traceback)
        int m = seq1.Length;
        int n = seq2.Length;
        string align1 = "";
        string align2 = "";

        while (m > 0 || n > 0)
        {
            if (m > 0 && n > 0 && scoreMatrix[m, n] == scoreMatrix[m - 1, n - 1] + (seq1[m - 1] == seq2[n - 1] ? matchScore : mismatchPenalty))
            {
                align1 = seq1[m - 1] + align1;
                align2 = seq2[n - 1] + align2;
                m--;
                n--;
            }
            else if (m > 0 && scoreMatrix[m, n] == scoreMatrix[m - 1, n] + gapPenalty)
            {
                align1 = seq1[m - 1] + align1;
                align2 = "-" + align2;
                m--;
            }
            else
            {
                align1 = "-" + align1;
                align2 = seq2[n - 1] + align2;
                n--;
            }
        }

        // Степень близости (нормализованная)
        int maxLen = Math.Max(seq1.Length, seq2.Length);
        int maxScore = maxLen * matchScore;

        return scoreMatrix[seq1.Length, seq2.Length] * 100 / maxScore;
    }
}
