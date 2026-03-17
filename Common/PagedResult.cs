using System.ComponentModel;
using System.Text.Json.Serialization;

namespace Common;

public record PagedResult<T>
{
    public List<T> Items { get; set; } = new();

    public int TotalItems { get; set; }
    public int TotalPages { get; set; }
    public int PageSize { get; set; }
    public int CurrentPage { get; set; }

    public bool Empty => !Items.Any();

    public PagedResult(List<T> items, int totalItems, int currentPage, int pageSize)
    {
        Items = items;
        TotalItems = totalItems;
        CurrentPage = currentPage;
        PageSize = pageSize;
        TotalPages = (int)Math.Ceiling(totalItems / (double)pageSize);
    }
}

public record BaseQueryParams
{
    private const int MaxPageSize = 10;

    public int Page { get; set; } = 1;

    private int _pageSize = 10;
    public int PageSize
    {
        get => _pageSize;
        set => _pageSize = value > MaxPageSize ? MaxPageSize : value;
    }

    public string SortBy { get; set; } = string.Empty;
    [Description("asc/desc")]
    public SortDirection SortDirection { get; set; }
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum SortDirection
{
    asc,
    desc
}