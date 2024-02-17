/*
 * Page Model from user request
 */
namespace BooksStock.API.Models
{
    public class PageModel
    {
        public int CurrentPage { get; set; } = Query.MinPage;
        public int QuantityPerPage { get; set; } = Query.MaxPerPage;
        public bool? InAscendingOrder { get; set; } = true;
    }
}
