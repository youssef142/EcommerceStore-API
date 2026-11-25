namespace EcommerceStore.Dtos
{
    public sealed class PagedResult<T>
    {
        public IReadOnlyList<T> Items { get; init; } = Array.Empty<T>();
        public int Page { get; init; }
        public int PageSize { get; init; }
        public int TotalItems { get; init; }
        public int TotalPages { get; init; }
        public bool HasPreviousPage => Page > 1;
        public bool HasNextPage => Page < TotalPages;

        public PagedResult<TOut> Map<TOut>(Func<T, TOut> selector) => new()
        {
            Items = Items.Select(selector).ToList(),
            Page = Page,
            PageSize = PageSize,
            TotalItems = TotalItems,
            TotalPages = TotalPages
        };
    }
}
