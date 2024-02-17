using BooksStock.API.Models;
using BooksStock.API.Services;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using System.Drawing;

namespace BooksStock.API.Controllers
{
    [ApiController]
    [Produces("application/json")]
    [Consumes("application/json")]
    public class StockV1Controller(StockServices services, ILogger<StockV1Controller> logger) : ControllerBase
    {
        private readonly StockServices _services = services;
        private readonly ILogger<StockV1Controller> _logger = logger;

        #region of HttpGet methods

        #region simple HttpGet methods
        /*!ATTENTION! may be overflow error, or slowdown preformance.Depends on current size of database
         * returns all data from database collection
        */
        [HttpGet, Route("all-books")]
        public async Task<ActionResult<List<BookProduct>>> GetAllBooks()
        {
            try
            {
                var books = await _services.GetAllBooksAsync();
                return books is null || books.Count == 0 ? 
                    NotFound("There no data found in Collection to display") : Ok(books);
            }
            catch (Exception error)
            {
                MyLogErrors(error);
                return Problem(error.Message.ToString());
            }
        }

        [HttpGet, Route("book-id")]
        public async Task<ActionResult<BookProduct>> GetBookWithId([Required][StringLength(24)]string id)
        {
            try
            {
                var book = await _services.GetBookByIdAsync(id);
                return book is null ? NotFound("There no product with id: '" + id + "'") : Ok(book);
            }
            catch(Exception error)
            {
                MyLogErrors(error);
                return Problem(error.Message.ToString());
            }
        }

        [HttpGet, Route("books-on-page")]
        public async Task<ActionResult<BookProduct>> GetPerPage([FromQuery]PageModel pageModel)
        {
            try
            {
                var books = await _services.GetAllBooksAsync();
                if(books is null || books.Count == 0)
                {
                    return NotFound("There no data found in Collection to display");
                }
                List<BookProduct> booksOnPage = ContentOnPage(books, pageModel);

                return booksOnPage is null || booksOnPage.Count == 0 ? 
                    NotFound("There no data found for this page: " + pageModel.CurrentPage) : 
                    Ok(booksOnPage);
            }
            catch(Exception error)
            {
                MyLogErrors(error);
                return Problem(error.Message.ToString());
            }
        }

        [HttpGet, Route("books-available")]
        public async Task<ActionResult<BookProduct>> GetAllBooksByAvailability([Required]bool available)
        {
            try
            {
                var books = (await _services.GetAllBooksAsync()).Where(book => book.IsAvailable == available).ToList();
                return books is null || books.Count == 0 ? 
                    NotFound("There no data in Collection with requiested parameters") : Ok(books);
            }
            catch(Exception error)
            {
                MyLogErrors(error);
                return Problem(error.Message.ToString());
            }
        }

        [HttpGet, Route("books-available-page")]
        public async Task<ActionResult<BookProduct>> GetPerPageBooksByAvailability([Required] bool available, [FromQuery]PageModel pageModel)
        {
            try
            {
                var books = (await _services.GetAllBooksAsync()).Where(book => book.IsAvailable == available).ToList();

                if(books is null || books.Count == 0)
                {
                    NotFound("There no data in Collection with requiested parameters");
                }
                List<BookProduct> booksOnReturn = ContentOnPage(books!, pageModel);

                return booksOnReturn is null || booksOnReturn.Count == 0 ?
                    NotFound("There no data in Collection with requiested parameters") : Ok(booksOnReturn);
            }
            catch (Exception error)
            {
                MyLogErrors(error);
                return Problem(error.Message.ToString());
            }
        }

        //returns list of all genres in collection
        [HttpGet, Route("genres")]
        public async Task<ActionResult<string>> GetAllGenres()
        {
            try
            {
                var books = await _services.GetAllBooksAsync();

                if(books is not null)
                {
                    List<string> genres = [];
                    foreach(var book in books)
                    {
                        if(book.Genres!.Length != 0)
                        {
                            genres.AddRange(book.Genres.ToArray().Where(genre => !genres.Contains(genre)).ToList());
                        }
                    }
                    return genres is null || genres.Count == 0 ?
                        NotFound("There no data in Collection") : Ok(genres);
                }
                return NotFound("There no data in Collection");
            }
            catch(Exception error)
            {
                MyLogErrors(error);
                return Problem(error.Message.ToString());
            }
        }
        #endregion


        #endregion

        #region of Helpper methods
        private void MyLogErrors(Exception error)
        {
            _logger.LogError(message: error.Message, args: error.StackTrace);
        }

        //returning content on particular page divided by particular quantity per page
        //sorting returning List by default(if not specified) in ascending order by price
        private List<BookProduct> ContentOnPage(List<BookProduct> allBooks, PageModel pageModel)
        {
            try
            {
                List<BookProduct> booksOnReturn = !pageModel.InAscendingOrder.HasValue ||
                    pageModel.InAscendingOrder == true ?
                    [.. allBooks.OrderBy(book => book.Price)] :
                    [.. allBooks.OrderByDescending(book => book.Price)];

                Query query = new(pageModel.CurrentPage, (pageModel.QuantityPerPage < Query.MinPerPage ? Query.MinPerPage : pageModel.QuantityPerPage), booksOnReturn.Count);

                return booksOnReturn is null || booksOnReturn.Count == 0 ? 
                    [] : [.. booksOnReturn.Skip(query.ToSkipQuantity).Take(query.QuantityPerPage)];
            }
            catch(Exception error )
            {
                MyLogErrors(error);
                return [];
            }
        }
        #endregion
    }
}
