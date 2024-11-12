namespace API.Extensions;

public static class MathEx
{
    public static int LevenshteinDistance(string a, string b)
    {
        int[,] d = new int[a.Length + 1, b.Length + 1];

        for (int i = 0; i <= a.Length; i++)
            d[i, 0] = i;

        for (int j = 0; j <= b.Length; j++)
            d[0, j] = j;

        for (int j = 1; j <= b.Length; j++)
        {
            for (int i = 1; i <= a.Length; i++)
            {
                int cost = (a[i - 1] == b[j - 1]) ? 0 : 1;

                int prevRow = d[i - 1, j];
                int prevCol = d[i, j - 1];
                int prevRowAndCol = d[i - 1, j - 1];

                d[i, j] = Math.Min(Math.Min(prevRow + 1, prevCol + 1), prevRowAndCol + cost);
            }
        }

        return d[a.Length, b.Length];
    }
}
