using Asp.Versioning;
using BooksStock.API.Models;
using BooksStock.API.Services;
using BooksStock.API.Services.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using System.ComponentModel.DataAnnotations;

namespace BooksStock.API.Controllers
{
    /*
     * Version for Admin to retrive all data and manipulates with data
     * Supports all methods: HttpGet, HttpPost, HttpPut and HttpDelete
     */
    [Authorize(Roles = ApiRolesConstants.Admin)]
    [ApiVersion("1.0")]
    [ApiController]
    [Produces("application/json")]
    [Consumes("application/json")]
    [EnableCors(policyName: "MyPolicyForAdmin")]
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

        #region of Filtering methods
        //returns list of ALL books where Title,Author, Language or one of item from array of Genres EQUALS to searchCondition
        [HttpGet, Route("books-equals-condition")]
        public async Task<ActionResult<List<BookProduct>>> GetAllBookEquals([Required]string term)
        {
            try
            {
                var books = await _services.GetAllBooksEqualsConditionAsync(term);
                return books is null || books.Count == 0 ?
                    NotFound("There no data in Collection that equals to: '" + term + "'") : Ok(books);
            }
            catch (Exception error)
            {
                MyLogErrors(error);
                return Problem(error.Message.ToString());
            }
        }

        //returns list of books AT the REQUESTED Page where Title,Author, Language or one of item from array of Genres EQUALS to search term
        [HttpGet, Route("books-equals-coondition-page")]
        public async Task<ActionResult<List<BookProduct>>> GetPerPageBookEquals([Required] string term, [FromQuery]PageModel pageModel)
        {
            try
            {
                var books = await _services.GetAllBooksEqualsConditionAsync(term);
                if(books is null || books.Count == 0 )
                {
                    return NotFound("There no data in Collection that equals to: '" + term + "'");
                }

                List<BookProduct> booksOnReturn = ContentOnPage(books, pageModel);

                return booksOnReturn is null || booksOnReturn.Count == 0 ?
                    NotFound("There no data in Collection that equals to: '" + term + "', on requested page = '" + pageModel.CurrentPage + "'.") :
                    Ok(booksOnReturn);
            }
            catch (Exception error)
            {
                MyLogErrors(error);
                return Problem(error.Message.ToString());
            }
        }

        //returns list of ALL books where Title,Author, Language or one of item from array of Genres CONTAINS to searchCondition
        [HttpGet, Route("books-contains-condition")]
        public async Task<ActionResult<List<BookProduct>>> GetAllBookContains([Required] string term)
        {
            try
            {
                var books = await _services.GetAllBooksContainsConditionAsync(term);
                return books is null || books.Count == 0 ?
                    NotFound("There no data in Collection that contains: '" + term + "'") : Ok(books);
            }
            catch (Exception error)
            {
                MyLogErrors(error);
                return Problem(error.Message.ToString());
            }
        }

        //returns list of books AT the REQUESTED Page where Title,Author, Language or one of item from array of Genres CONTAINS to search term
        [HttpGet, Route("books-contains-coondition-page")]
        public async Task<ActionResult<List<BookProduct>>> GetPerPageBookContains([Required] string term, [FromQuery] PageModel pageModel)
        {
            try
            {
                var books = await _services.GetAllBooksContainsConditionAsync(term);
                if (books is null || books.Count == 0)
                {
                    return NotFound("There no data in Collection that contains: '" + term + "'");
                }

                List<BookProduct> booksOnReturn = ContentOnPage(books, pageModel);

                return booksOnReturn is null || booksOnReturn.Count == 0 ?
                    NotFound("There no data in Collection that contains: '" + term + "', on requested page = '" + pageModel.CurrentPage + "'.") :
                    Ok(booksOnReturn);
            }
            catch (Exception error)
            {
                MyLogErrors(error);
                return Problem(error.Message.ToString());
            }
        }

        /* !ATTENTION! may be overflow error, or slowdown preformance. Depends on current size of database
         * matching books in collection by entered filters, where athour equals author, title equals title and etc.
         * if there was not entered any searching parameters returns all list of collection 
         */
        [HttpGet, Route("books-filtered")]
        public async Task<ActionResult<List<BookProduct>>> GetAllUnderFilter([FromQuery]FilterForBook filter)
        {
            try
            {
                var books = await _services.GetAllBooksAsync();

                if(books is null || books.Count == 0)
                {
                    return NotFound("There no data in Collection");
                }
                List<BookProduct> booksMatches = ApplyFilters(books, filter);

                return booksMatches is null || booksMatches.Count == 0 ?
                    NotFound("There no data in Collection by requested filter: \n" + filter.ToJson()) :
                    Ok(booksMatches);
            }
            catch (Exception error)
            {
                MyLogErrors(error);
                return Problem(error.Message.ToString());
            }
        }

        /* !ATTENTION! may be overflow error, or slowdown preformance. Depends on current size of database
         * matching books, AT the REQUESTED Page, in collection by entered filters, where athour equals author, 
         * title equalstitle and etc.
         * if there was not entered any searching parameters returns all list of collection 
        */
        [HttpGet, Route("books-filtered-page")]
        public async Task<ActionResult<List<BookProduct>>> GetPerPageUnderFilter([FromQuery] FilterForBook filter, [FromQuery]PageModel pageModel)
        {
            try
            {
                var books = await _services.GetAllBooksAsync();

                if (books is null || books.Count == 0)
                {
                    return NotFound("There no data in Collection");
                }
                List<BookProduct> booksMatches = ApplyFilters(books, filter);
                if(booksMatches is null || booksMatches.Count == 0)
                {
                    NotFound("There no data in Collection by requested filter: \n" + filter.ToJson());
                }
                booksMatches = ContentOnPage(booksMatches!, pageModel);

                return booksMatches is null || booksMatches.Count == 0 ?
                    NotFound("There no data in Collection that contains:\n'" + filter.ToJson() + "',\non requested page = '" + pageModel.CurrentPage + "'.") :
                    Ok(booksMatches);
            }
            catch (Exception error)
            {
                MyLogErrors(error);
                return Problem(error.Message.ToString());
            }
        }
        #endregion

        #region of Counting Methods
        //returning a number of all books in collection
        [HttpGet, Route("all-books-count")]
        public async Task<ActionResult<int>> GetQuantityOfAllBooks()
        {
            try
            {
                int quantity = (await _services.GetAllBooksAsync()).Count;

                return Ok(quantity);
            }
            catch (Exception error)
            {
                MyLogErrors(error);
                return Problem(error.Message.ToString());
            }
        }

        //returning a number of all books in collection, where isAvailable parameter equals requested true or false
        [HttpGet, Route("all-available-count")]
        public async Task<ActionResult<int>> GetQuantityAvailable([Required]bool available)
        {
            try
            {
                int quantity = (await _services.GetAllBooksAsync()).Where(book => book.IsAvailable == available).ToList().Count;
                return Ok(quantity);
            }
            catch (Exception error)
            {
                MyLogErrors(error);
                return Problem(error.Message.ToString());
            }
        }

        //returning a number of all books under particular genre
        [HttpGet, Route("books-genre-count")]
        public async Task<ActionResult<int>> GetQuantityAtGenre([Required]string genre)
        {
            try
            {
                return (await _services.GetAllBooksAsync()).Any(book => book.Genres!.Contains(genre)) ?
                    Ok((await _services.GetAllBooksAsync()).Where(book => book.Genres!.Contains(genre)).ToList().Count) :
                    NotFound("The genre = '" + genre + "', was not found in Collection");
            }
            catch (Exception error)
            {
                MyLogErrors(error);
                return Problem(error.Message.ToString());
            }
        }
        #endregion
        #endregion
        #region of HttpMethods for manipulations with Collection
        [HttpPost, Route("book-add")]
        public async Task<ActionResult> PostBook([FromQuery]BookProduct book)
        {
            try
            {
                if(book.Genres is null || book.Genres.Length == 0)
                {
                    book!.Genres = ["unspecified"];
                }
                BookProduct newBook = new()
                {
                    Title = book.Title,
                    Author = book.Author,
                    Description = book.Description,
                    Language = book.Language,
                    Link = !string.IsNullOrEmpty(book.Link!.ToString()) && Uri.IsWellFormedUriString(book.Link.ToString(), UriKind.Absolute) ? book.Link : new Uri("about:blank"),
                    Genres = [..book.Genres],
                    IsAvailable = book.IsAvailable,
                    Price = book.Price > 0 ? book.Price : 0
                };

                 await _services.AddNewAsync(newBook);

                _logger.LogInformation(message: @"Post Book: {@newBook} at {@DateTime}", newBook, DateTime.Now);
                return Ok("Successfully added: " + newBook.ToJson());
            }
            catch (Exception error)
            {
                MyLogErrors(error);
                return Problem(error.Message.ToString());
            }
        }

        /*
         * !ATTENTION! if new genre/genres was entered then the all existing genres 
         * would be replaced with new entered list
         * Required: Id, isAvailable another parameters could be not entered, 
         * parameters that was not entered would be not changed
         * If Id was not specified or incorrect - returns BadRequest(), 
         * if Id is not found - returns NotFound() object result
         * If isAvailable not specified - sets to false
        */
        [HttpPut, Route("book-update")]
        public async Task<ActionResult> PutBook([FromQuery]BookProduct book)
        {
            try
            {
                if(string.IsNullOrEmpty(book.Id) || book.Id.Length != 24)
                {
                    _logger.LogInformation(message: @"The Id was not provided, the Update request was declined at {@DateTime}", DateTime.Now);
                    return BadRequest("The Id should be valid Bson Id");
                }
                var bookOld = await _services.GetBookByIdAsync(book.Id);
                if(bookOld is null)
                {
                    _logger.LogInformation(message: @"The Id: {@id} was not found, the Update request was declined at {@DateTime}", book.Id.ToJson(), DateTime.Now);
                    return NotFound("Book with Id: '" + book.Id + "', was not found in collection, update request rejected");
                }

                //ATTENTION if new genre/genres was entered then the all existing genres would be replaced with new entered list
                book.Genres ??= bookOld.Genres!.Length == 0 ? ["unspecified"] : [..bookOld.Genres];

                BookProduct updatedBook = new()
                { 
                    Id = book.Id,
                    Title = string.IsNullOrEmpty(book.Title) ? bookOld.Title : book.Title,
                    Author = string.IsNullOrEmpty(book.Author) ? bookOld.Author : book.Author,
                    Description = string.IsNullOrEmpty(book.Description) ? bookOld.Description : book.Description,
                    Language = string.IsNullOrEmpty(book.Language) ? bookOld.Language : book.Language,
                    IsAvailable = book.IsAvailable,
                    Price = book.Price > 0 ? book.Price : bookOld.Price,
                    Genres = book.Genres,
                    Link = book.Link is not null && Uri.IsWellFormedUriString(book.Link.ToString(), UriKind.Absolute) ? book.Link : new Uri("about:blank")
                };

                await _services.UpdateNewAsync(updatedBook);
                _logger.LogInformation(message: @"Put Book: {@updatedBook} at {@DateTime}", updatedBook, DateTime.Now);
                return Ok("Successfully updated: " + updatedBook.ToJson());

            }
            catch (Exception error)
            {
                MyLogErrors(error);
                return Problem(error.Message.ToString());
            }
        }

        [HttpDelete, Route("book-delete")]
        public async Task<ActionResult> RemoveBook([Required][StringLength(24)]string id)
        {
            try
            {
                var book = await _services.GetBookByIdAsync(id);
                if(book is null)
                {
                    _logger.LogInformation(message: @"Book with Id: '{@id}' was not found in Collection, the deliting request was declined at {@DateTime}", id, DateTime.Now);
                    return BadRequest("The data with id: '" + id + "', was not found in Collection");
                }

                await _services.DeleteAsync(id);
                _logger.LogInformation(message: @"Book with Id: '{@id}' successfully deleted at {@DateTime}. Data Deleted: {@book}", id, DateTime.Now, book.ToJson());
                return Ok("Successfully deleted data: " + book.ToJson());
            }
            catch (Exception error)
            {
                MyLogErrors(error);
                return Problem(error.Message.ToString());
            }
        }
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

        /*
         * Method returning list of books where items contains all FilterForBook criterias
        */
        private List<BookProduct> ApplyFilters(List<BookProduct> allBooks, FilterForBook filter)
        {
            try
            {
                if(allBooks is not null || allBooks!.Count != 0)
                {
                    List<BookProduct> booksMatches = [.. allBooks];

                    booksMatches = !string.IsNullOrEmpty(filter.Author) ?
                        booksMatches.Where(book => book.Author!.Contains(filter.Author, StringComparison.OrdinalIgnoreCase)).ToList() : booksMatches;

                    booksMatches = !string.IsNullOrEmpty(filter.Title) ?
                        booksMatches.Where(book => book.Title!.Contains(filter.Title, StringComparison.OrdinalIgnoreCase)).ToList() : booksMatches;

                    booksMatches = !string.IsNullOrEmpty(filter.Annotation) ?
                        booksMatches.Where(book => book.Description!.Contains(filter.Annotation, StringComparison.OrdinalIgnoreCase)).ToList() : booksMatches;

                    booksMatches = !string.IsNullOrEmpty(filter.Language) ?
                        booksMatches.Where(book => book.Language!.Contains(filter.Language, StringComparison.OrdinalIgnoreCase)).ToList() : booksMatches;

                    booksMatches = filter.Genres is not null?
                        booksMatches.Where(book => book.Genres!.Any(genre => filter.Genres.Any(g => g.Contains(genre, StringComparison.OrdinalIgnoreCase)))).ToList() : 
                        booksMatches;

                    booksMatches = filter.IsAvailable.HasValue ?
                        booksMatches.Where(book => book.IsAvailable == filter.IsAvailable).ToList() :
                        booksMatches;

                    booksMatches = filter.MinPrice.HasValue && filter.MaxPrice.HasValue ?
                        booksMatches.Where(book => book.Price >= filter.MinPrice && book.Price <= filter.MaxPrice).ToList() :
                        filter.MinPrice.HasValue && !filter.MaxPrice.HasValue ?
                        booksMatches.Where(book => book.Price >= filter.MinPrice).ToList() :
                        !filter.MinPrice.HasValue && filter.MaxPrice.HasValue ?
                        booksMatches.Where(book => book.Price <= filter.MaxPrice).ToList() :
                        booksMatches;

                    return booksMatches;
                }
                return [];
            }
            catch (Exception error)
            {
                MyLogErrors(error);
                return [];
            }
        }
        #endregion
    }
    /*
     * Version for Users, retrives some data from Collection, where data.IsAvailable == true
     * Supports only some Http Get methods
    */
    [ApiVersion("2.0")]
    [ApiController]
    [Consumes("application/json")]
    [Produces("application/json")]
    [EnableCors(policyName: "MyPolicyForUsers")]
    public class StockV2Controller(StockServices services, ILogger<StockV2Controller> logger) : ControllerBase
    {
        private readonly StockServices _services = services;
        private readonly ILogger<StockV2Controller> _logger = logger;

        #region of HttpGet methods

        #region simple HttpGet methods
        /*!ATTENTION! may be overflow error, or slowdown preformance.Depends on current size of database
         * returns all data from database collection, where parameter IsAvailable == true
        */
        [HttpGet, Route("all-books")]
        public async Task<ActionResult<List<BookProduct>>> GetAllBooks()
        {
            try
            {
                var books = (await _services.GetAllBooksAsync()).Where(book => book.IsAvailable).ToList();
                return books is null || books.Count == 0 ?
                    NotFound("There no data found in Collection to display") : Ok(books);
            }
            catch (Exception error)
            {
                MyLogErrors(error);
                return Problem(error.Message.ToString());
            }
        }


        [HttpGet, Route("books-on-page")]
        public async Task<ActionResult<BookProduct>> GetPerPage([FromQuery] PageModel pageModel)
        {
            try
            {
                var books = (await _services.GetAllBooksAsync()).Where(book => book.IsAvailable).ToList();
                if (books is null || books.Count == 0)
                {
                    return NotFound("There no data found in Collection to display");
                }
                List<BookProduct> booksOnPage = ContentOnPage(books, pageModel);

                return booksOnPage is null || booksOnPage.Count == 0 ?
                    NotFound("There no data found for this page: " + pageModel.CurrentPage) :
                    Ok(booksOnPage);
            }
            catch (Exception error)
            {
                MyLogErrors(error);
                return Problem(error.Message.ToString());
            }
        }

        //returns list of all genres in collection, where at least one book IsAvailable == true
        [HttpGet, Route("genres")]
        public async Task<ActionResult<string>> GetAllGenres()
        {
            try
            {
                var books = (await _services.GetAllBooksAsync()).Where(book => book.IsAvailable).ToList();

                if (books is not null)
                {
                    List<string> genres = [];
                    foreach (var book in books)
                    {
                        if (book.Genres!.Length != 0)
                        {
                            genres.AddRange(book.Genres.ToArray().Where(genre => !genres.Contains(genre)).ToList());
                        }
                    }
                    return genres is null || genres.Count == 0 ?
                        NotFound("There no data in Collection") : Ok(genres);
                }
                return NotFound("There no data in Collection");
            }
            catch (Exception error)
            {
                MyLogErrors(error);
                return Problem(error.Message.ToString());
            }
        }
        #endregion

        #region of Filtering methods
        //returns list of ALL books.IsAvailable where Title,Author, Language or one of item from array of Genres EQUALS to searchCondition
        [HttpGet, Route("books-equals-condition")]
        public async Task<ActionResult<List<BookProduct>>> GetAllBookEquals([Required] string term)
        {
            try
            {
                var books = (await _services.GetAllBooksEqualsConditionAsync(term)).Where(book => book.IsAvailable).ToList();
                return books is null || books.Count == 0 ?
                    NotFound("There no data in Collection that equals to: '" + term + "'") : Ok(books);
            }
            catch (Exception error)
            {
                MyLogErrors(error);
                return Problem(error.Message.ToString());
            }
        }

        //returns list of books.IsAvailable AT the REQUESTED Page where Title,Author, Language or one of item from array of Genres EQUALS to search term
        [HttpGet, Route("books-equals-coondition-page")]
        public async Task<ActionResult<List<BookProduct>>> GetPerPageBookEquals([Required] string term, [FromQuery] PageModel pageModel)
        {
            try
            {
                var books = (await _services.GetAllBooksEqualsConditionAsync(term)).Where(book => book.IsAvailable).ToList();
                if (books is null || books.Count == 0)
                {
                    return NotFound("There no data in Collection that equals to: '" + term + "'");
                }

                List<BookProduct> booksOnReturn = ContentOnPage(books, pageModel);

                return booksOnReturn is null || booksOnReturn.Count == 0 ?
                    NotFound("There no data in Collection that equals to: '" + term + "', on requested page = '" + pageModel.CurrentPage + "'.") :
                    Ok(booksOnReturn);
            }
            catch (Exception error)
            {
                MyLogErrors(error);
                return Problem(error.Message.ToString());
            }
        }

        //returns list of ALL books.IsAvailable where Title,Author, Language or one of item from array of Genres CONTAINS to searchCondition
        [HttpGet, Route("books-contains-condition")]
        public async Task<ActionResult<List<BookProduct>>> GetAllBookContains([Required] string term)
        {
            try
            {
                var books = (await _services.GetAllBooksContainsConditionAsync(term)).Where(book => book.IsAvailable).ToList();
                return books is null || books.Count == 0 ?
                    NotFound("There no data in Collection that contains: '" + term + "'") : Ok(books);
            }
            catch (Exception error)
            {
                MyLogErrors(error);
                return Problem(error.Message.ToString());
            }
        }

        //returns list of books.IsAvailable AT the REQUESTED Page where Title,Author, Language or one of item from array of Genres CONTAINS to search term
        [HttpGet, Route("books-contains-coondition-page")]
        public async Task<ActionResult<List<BookProduct>>> GetPerPageBookContains([Required] string term, [FromQuery] PageModel pageModel)
        {
            try
            {
                var books = (await _services.GetAllBooksContainsConditionAsync(term)).Where(book => book.IsAvailable).ToList();
                if (books is null || books.Count == 0)
                {
                    return NotFound("There no data in Collection that contains: '" + term + "'");
                }

                List<BookProduct> booksOnReturn = ContentOnPage(books, pageModel);

                return booksOnReturn is null || booksOnReturn.Count == 0 ?
                    NotFound("There no data in Collection that contains: '" + term + "', on requested page = '" + pageModel.CurrentPage + "'.") :
                    Ok(booksOnReturn);
            }
            catch (Exception error)
            {
                MyLogErrors(error);
                return Problem(error.Message.ToString());
            }
        }

        /* !ATTENTION! may be overflow error, or slowdown preformance. Depends on current size of database
         * matching books.IsAvailable in collection by entered filters, where athour equals author, title equals title and etc.
         * if there was not entered any searching parameters returns all list of collection 
         */
        [HttpGet, Route("books-filtered")]
        public async Task<ActionResult<List<BookProduct>>> GetAllUnderFilter([FromQuery] FilterForBook filter)
        {
            try
            {
                var books = (await _services.GetAllBooksAsync()).Where(book => book.IsAvailable).ToList();

                if (books is null || books.Count == 0)
                {
                    return NotFound("There no data in Collection");
                }
                List<BookProduct> booksMatches = ApplyFilters(books, filter);

                return booksMatches is null || booksMatches.Count == 0 ?
                    NotFound("There no data in Collection by requested filter: \n" + filter.ToJson()) :
                    Ok(booksMatches);
            }
            catch (Exception error)
            {
                MyLogErrors(error);
                return Problem(error.Message.ToString());
            }
        }

        /* !ATTENTION! may be overflow error, or slowdown preformance. Depends on current size of database
         * matching books.IsAvailable, AT the REQUESTED Page, in collection by entered filters, where athour equals author, 
         * title equalstitle and etc.
         * if there was not entered any searching parameters returns all list of collection 
        */
        [HttpGet, Route("books-filtered-page")]
        public async Task<ActionResult<List<BookProduct>>> GetPerPageUnderFilter([FromQuery] FilterForBook filter, [FromQuery] PageModel pageModel)
        {
            try
            {
                var books = (await _services.GetAllBooksAsync()).Where(book => book.IsAvailable).ToList();

                if (books is null || books.Count == 0)
                {
                    return NotFound("There no data in Collection");
                }
                List<BookProduct> booksMatches = ApplyFilters(books, filter);
                if (booksMatches is null || booksMatches.Count == 0)
                {
                    NotFound("There no data in Collection by requested filter: \n" + filter.ToJson());
                }
                booksMatches = ContentOnPage(booksMatches!, pageModel);

                return booksMatches is null || booksMatches.Count == 0 ?
                    NotFound("There no data in Collection that contains:\n'" + filter.ToJson() + "',\non requested page = '" + pageModel.CurrentPage + "'.") :
                    Ok(booksMatches);
            }
            catch (Exception error)
            {
                MyLogErrors(error);
                return Problem(error.Message.ToString());
            }
        } 
        #endregion

        #region of Counting Methods
        //returning a number of all books.IsAvailable in collection
        [HttpGet, Route("all-books-count")]
        public async Task<ActionResult<int>> GetQuantityOfAllBooks()
        {
            try
            {
                int quantity = (await _services.GetAllBooksAsync()).Where(book => book.IsAvailable).ToList().Count;

                return Ok(quantity);
            }
            catch (Exception error)
            {
                MyLogErrors(error);
                return Problem(error.Message.ToString());
            }
        }

        //returning a number of all books.IsAvailable under particular genre
        [HttpGet, Route("books-genre-count")]
        public async Task<ActionResult<int>> GetQuantityAtGenre([Required] string genre)
        {
            try
            {
                return (await _services.GetAllBooksAsync()).Any(book => book.Genres!.Contains(genre) && book.IsAvailable) ?
                    Ok((await _services.GetAllBooksAsync()).Where(book => book.Genres!.Contains(genre) && book.IsAvailable).ToList().Count) :
                    NotFound("The genre = '" + genre + "', was not found in Collection");
            }
            catch (Exception error)
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
            catch (Exception error)
            {
                MyLogErrors(error);
                return [];
            }
        }

        /*
         * Method returning list of books where items contains all FilterForBook criterias, 
         * expect the 'IsAvailable' parameter not applys, because 2 API Version only has access where IsAvailable == true only
        */
        private List<BookProduct> ApplyFilters(List<BookProduct> allBooks, FilterForBook filter)
        {
            try
            {
                if (allBooks is not null || allBooks!.Count != 0)
                {
                    List<BookProduct> booksMatches = [.. allBooks];

                    booksMatches = !string.IsNullOrEmpty(filter.Author) ?
                        booksMatches.Where(book => book.Author!.Contains(filter.Author, StringComparison.OrdinalIgnoreCase)).ToList() : booksMatches;

                    booksMatches = !string.IsNullOrEmpty(filter.Title) ?
                        booksMatches.Where(book => book.Title!.Contains(filter.Title, StringComparison.OrdinalIgnoreCase)).ToList() : booksMatches;

                    booksMatches = !string.IsNullOrEmpty(filter.Annotation) ?
                        booksMatches.Where(book => book.Description!.Contains(filter.Annotation, StringComparison.OrdinalIgnoreCase)).ToList() : booksMatches;

                    booksMatches = !string.IsNullOrEmpty(filter.Language) ?
                        booksMatches.Where(book => book.Language!.Contains(filter.Language, StringComparison.OrdinalIgnoreCase)).ToList() : booksMatches;

                    booksMatches = filter.Genres is not null ?
                        booksMatches.Where(book => book.Genres!.Any(genre => filter.Genres.Any(g => g.Contains(genre, StringComparison.OrdinalIgnoreCase)))).ToList() :
                        booksMatches;

                    booksMatches = filter.MinPrice.HasValue && filter.MaxPrice.HasValue ?
                        booksMatches.Where(book => book.Price >= filter.MinPrice && book.Price <= filter.MaxPrice).ToList() :
                        filter.MinPrice.HasValue && !filter.MaxPrice.HasValue ?
                        booksMatches.Where(book => book.Price >= filter.MinPrice).ToList() :
                        !filter.MinPrice.HasValue && filter.MaxPrice.HasValue ?
                        booksMatches.Where(book => book.Price <= filter.MaxPrice).ToList() :
                        booksMatches;

                    return booksMatches;
                }
                return [];
            }
            catch (Exception error)
            {
                MyLogErrors(error);
                return [];
            }
        }
        #endregion
    }
}
