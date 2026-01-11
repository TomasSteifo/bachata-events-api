namespace BachataEvents.Application.Common;

public static class Pagination
{
    public const int DefaultPage = 1;
    public const int DefaultPageSize = 10;
    public const int MaxPageSize = 50;

    public static (int Page, int PageSize) Normalize(int? page, int? pageSize)
    {
        var p = page.GetValueOrDefault(DefaultPage);
        if (p < 1) p = DefaultPage;

        var ps = pageSize.GetValueOrDefault(DefaultPageSize);
        if (ps < 1) ps = DefaultPageSize;
        if (ps > MaxPageSize) ps = MaxPageSize;

        return (p, ps);
    }
}
