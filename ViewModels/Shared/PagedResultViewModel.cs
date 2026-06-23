namespace WebWorkNew.ViewModels.Shared;

public class PagedResultViewModel<T>
{
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalItems { get; set; }

    public int TotalPages => (int)Math.Ceiling((double)TotalItems / PageSize);

    public string? SearchTerm { get; set; }
    public List<T> Items { get; set; } = new();

    public bool HasPreviousPage => PageNumber > 1;
    public bool HasNextPage => PageNumber < TotalPages;
}

