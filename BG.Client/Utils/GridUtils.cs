namespace BG.Client.Utils;

public static class GridUtils
{
    public const int TotalRows = 300;
    public const int TotalColumns = 300;
    public const double CellWidth = 80;
    public const double CellHeight = 24;

    public static double GetColumnWidth(int colIndex)
    {
        if (colIndex == 1) return CellWidth * 2;
        if (colIndex == 2) return CellWidth * 3;
        if (colIndex == 3) return CellWidth * 2;
        return CellWidth;
    }

    public static double GetColumnLeft(int colIndex)
    {
        double left = 0;
        for (var c = 0; c < colIndex; c++)
            left += GetColumnWidth(c);
        return left;
    }

    public static double TotalGridWidth
    {
        get
        {
            double w = 0;
            for (var c = 0; c < TotalColumns; c++)
                w += GetColumnWidth(c);
            return w;
        }
    }

    public static int GetColumnAtScroll(double scrollLeft)
    {
        for (var c = TotalColumns - 1; c >= 0; c--)
            if (GetColumnLeft(c) <= scrollLeft) return c;
        return 0;
    }

    public static int GetLastVisibleColumn(double scrollLeft, double viewportWidth)
    {
        var end = scrollLeft + viewportWidth;
        for (var c = 0; c < TotalColumns; c++)
            if (GetColumnLeft(c) + GetColumnWidth(c) >= end) return c;
        return TotalColumns - 1;
    }

    public static string ColumnIndexToLetter(int colIndex)
    {
        if (colIndex < 0) return "";
        var result = "";
        while (colIndex >= 0)
        {
            result = (char)('A' + (colIndex % 26)) + result;
            colIndex = colIndex / 26 - 1;
        }
        return result;
    }

    public static string GetCellRef(int row0Based, int col0Based)
    {
        return ColumnIndexToLetter(col0Based) + (row0Based + 1).ToString();
    }
}
