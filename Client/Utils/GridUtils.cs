namespace BG.Client.Utils;

public static class GridUtils
{
    public const int TotalRows = 300;
    public const int TotalColumns = 300;
    public const double CellWidth = 80;
    public const double CellHeight = 24;

    /// <summary>Column 1 (Upgrade Name) = 2x, Column 2 (Description) = 3x, rest = 1x.</summary>
    public static double GetColumnWidth(int colIndex)
    {
        if (colIndex == 1) return CellWidth * 2;  // Upgrade Name
        if (colIndex == 2) return CellWidth * 3;  // Description
        if (colIndex == 3) return CellWidth * 2;  // Description
        return CellWidth;
    }

    /// <summary>Left offset in px for the given column (0-based).</summary>
    public static double GetColumnLeft(int colIndex)
    {
        double left = 0;
        for (var c = 0; c < colIndex; c++)
            left += GetColumnWidth(c);
        return left;
    }

    /// <summary>Total width of the grid content.</summary>
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

    /// <summary>Column index at or before the given scroll position (0-based).</summary>
    public static int GetColumnAtScroll(double scrollLeft)
    {
        for (var c = TotalColumns - 1; c >= 0; c--)
            if (GetColumnLeft(c) <= scrollLeft) return c;
        return 0;
    }

    /// <summary>Last column index that fits within scrollLeft + viewportWidth.</summary>
    public static int GetLastVisibleColumn(double scrollLeft, double viewportWidth)
    {
        var end = scrollLeft + viewportWidth;
        for (var c = 0; c < TotalColumns; c++)
            if (GetColumnLeft(c) + GetColumnWidth(c) >= end) return c;
        return TotalColumns - 1;
    }

    /// <summary>
    /// Converts 0-based column index to Excel column letter(s): 0 -> A, 25 -> Z, 26 -> AA, etc.
    /// </summary>
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

    /// <summary>
    /// Returns the cell reference string, e.g. "A1". Row is 1-based for display.
    /// </summary>
    public static string GetCellRef(int row0Based, int col0Based)
    {
        return ColumnIndexToLetter(col0Based) + (row0Based + 1).ToString();
    }
}
