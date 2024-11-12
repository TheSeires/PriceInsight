namespace API.Extensions;

public static class PaginationEx
{
    public static int GetPageSkip(int normalizedPage, int normalizedPageSize) =>
        Math.Max((normalizedPage - 1) * normalizedPageSize, 0);

    public static (int Skip, int Take) GetNormalizedPaginationFilters(int page, int pageSize, int maxLimit)
    {
        var normalizedPage = NormalizePage(page);
        var normalizedPageSize = NormalizePageSize(pageSize, maxLimit);
        var skip = GetPageSkip(normalizedPage, normalizedPageSize);

        return (skip, normalizedPageSize);
    }

    public static int NormalizePageSize(int pageSize, int maxValue = 20) => Math.Clamp(pageSize, 1, maxValue);

    public static int NormalizePage(int page) => Math.Max(page, 1);
}