/*
 * Class for organizing content into pages, processing to split content into pages
 */
namespace BooksStock.API.Models
{
    public class Query
    {
        public const int MinPerPage = 5;
        public const int MaxPerPage = 20;
        public const int MinPage = 1;

        private readonly int _currentPage = MinPage;
        private readonly int _totalQuantity = MaxPerPage;
        private readonly int _totalPages;

        public Query(int currentPage, int quantityPerPage, int totalQuantity)
        {
            _currentPage = currentPage < MinPage ? MinPage : currentPage;
            QuantityPerPage = quantityPerPage > MinPerPage && quantityPerPage < MaxPerPage ?
                quantityPerPage :
                quantityPerPage > MaxPerPage ?
                MaxPerPage : MinPerPage;
            _totalQuantity = totalQuantity > 0 ? totalQuantity : 0;

            _totalPages = (_totalQuantity % quantityPerPage) == 0 ?
                _totalQuantity / quantityPerPage :
                (_totalQuantity / quantityPerPage) + 1;

            int sqip = (_currentPage - MinPage) * QuantityPerPage;

            ToSkipQuantity = sqip >= _totalQuantity || sqip < 0 ?
                (_totalPages - 1) * QuantityPerPage : sqip;
        }
        public int ToSkipQuantity { get; }
        public int QuantityPerPage { get; } = MaxPerPage;
    }
}
