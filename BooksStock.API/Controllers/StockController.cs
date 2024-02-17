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
        #endregion


        #endregion

        #region of Helpper methods
        private void MyLogErrors(Exception error)
        {
            _logger.LogError(message: error.Message, args: error.StackTrace);
        }

        #endregion
    }
}
