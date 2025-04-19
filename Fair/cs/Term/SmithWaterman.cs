namespace Uccs.Fair;

public class SmithWaterman
{
    static int matchScore = 2;
    static int mismatchPenalty = -1;
    static int gapPenalty = -2;

    public static int SmithWatermanAlignment(string seq1, string seq2)
    {
        int rows = seq1.Length + 1;
        int cols = seq2.Length + 1;
        int[,] scoreMatrix = new int[rows, cols];
        int maxScore = 0;
        int maxI = 0, maxJ = 0;

        // Инициализация матрицы
        for (int i = 1; i < rows; i++)
        {
            for (int j = 1; j < cols; j++)
            {
                int match = scoreMatrix[i - 1, j - 1] + (seq1[i - 1] == seq2[j - 1] ? matchScore : mismatchPenalty);
                int delete = scoreMatrix[i - 1, j] + gapPenalty;
                int insert = scoreMatrix[i, j - 1] + gapPenalty;
                scoreMatrix[i, j] = Math.Max(0, Math.Max(match, Math.Max(delete, insert)));

                if (scoreMatrix[i, j] > maxScore)
                {
                    maxScore = scoreMatrix[i, j];
                    maxI = i;
                    maxJ = j;
                }
            }
        }

        // Обратный путь (Traceback)
        string align1 = "";
        string align2 = "";
        int m = maxI;
        int n = maxJ;

        while (m > 0 && n > 0 && scoreMatrix[m, n] > 0)
        {
            if (scoreMatrix[m, n] == scoreMatrix[m - 1, n - 1] + (seq1[m - 1] == seq2[n - 1] ? matchScore : mismatchPenalty))
            {
                align1 = seq1[m - 1] + align1;
                align2 = seq2[n - 1] + align2;
                m--;
                n--;
            }
            else if (scoreMatrix[m, n] == scoreMatrix[m - 1, n] + gapPenalty)
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

		return maxScore;
	}
}