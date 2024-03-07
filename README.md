<h2>BooksStock.API - API for retriving data from Mongo Atlas Database</h2>
<b><i>BooksStock.API</i></b> use Mongo Atlas shared Replica Set database, Azure SQL, ASP.NET Core.<br>
<b><i>API-KEY</i></b>: "6CBxzdYcEgNDrRhMbDpkBF7e4d4Kib46dwL9ZE5egiL0iL5Y3dzREUBSUYVUwUkN"<br>
<b><i>Logging data for admin</i></b>:<br>
  <b>UserLogin</b>=admin<br>
  <b>Password</b>=P@ssw0rd<br>
<h3>Mongo Atlas Database:</h3>
<b>Mongo Atlas Shared Replica set</b> contains all data about books library, because in NoSQL data is stored in a more freeform, without rigid schemas, 
that makes NoSQL more flexible than SQL.
In case of increase in traffic there is no need to add additional servers for scaling, there is enough hardware for this.
NoSQL has high performance as data or traffic increases due to its scalable architecture.
NoSQL database can automatically duplicate data across multiple servers, data centers or cloud resources. This benefit helps to minimize latency for users. 
It also reduces the load on database management.
<br>
<h3>Azure SQL:</h3>
<b>Azure SQL</b> used for Authentication and Authorization for API.
<br>
<h3>About API:</h3>
Async methods in API, providing the ability to handle several concurrent HTTP requests. They are not blocking the main thread while waiting for the database response. 
This API consumes and produces data in Json format, because this format is simple and lightweighted.
<br>
<h3>API Security:</h3>
All session tokens, api version, api-key and user loging, user email and user password sends as Headers.
<br>
To use API-Version:2, don't need to provide JWT token, api-key and user data. But API-Version:2 only supports some GET methods. <br>
Just required to specify API-Version:2.
<br>
API-Version:1 supports all methods of API (<b>GET, POST, PUT, DELETE</b>), for access to it requires: <b>api-key</b> and <b>valid JWT token</b>. <br>
To <b>JWT token</b>, required provide <b>api-key</b> and correct <b>admin loging details</b>, after thet API will generate <b>JWT token that valid for 1 hour</b>.<br>
Also there ability to <b>create own administration acccount</b>, for that only required to provide <b>api-key</b>.<br>
<br>
The user guid for API could be found and downloaded from my portfolio: <br>
https://msichova.github.io/projects.html under section for project: BooksStock.API, in light version of API project or secure version of API project.<br>
<table>
  <thead>
    <tr>
      <th colspan=3><h3>Authentication Methods:</h3></th>
    </tr>
    <tr>
      <th>Methods:</th>
      <th>Supports:</th>
      <th>Description:</th>
    </tr>
  </thead>
  <tbody>
    <tr>
      <td>POST/authorization/loging</td>
      <td>ApiVersion-BooksStore :  1</td>
      <td>Singing in existing administration account and provides token.<br>
        Default account:<br>
        UserName: admin<br>
        Password: P@ssw0rd</td>
    </tr>
     <tr>
      <td>POST</mark>/authorization/register</td>
      <td>ApiVersion-BooksStore :  1</td>
      <td>Registering new administration account, require unique username, unique email, and password</td>
    </tr>
  </tbody>
  <thead>
    <tr>
      <th colspan=3><h3>StockV Methods:</h3></th>
    </tr>
    <tr>
      <th>Methods:</th>
      <th>Supports:</th>
      <th>Description:</th>
    </tr>
  </thead>
  <tbody>
    <tr>
      <td>GET/all-books </td>
      <td>ApiVersion-BooksStore :  1<br>
        ApiVersion-BooksStore :  2</td>
      <td>Retrieves all data from database. For ApiVersion-BooksStore :  2 retrieves only were parameter “isAvailable” : true</td>
    </tr>
     <tr>
      <td>GET/book-id</td>
      <td>ApiVersion-BooksStore :  1</td>
      <td>Gets book by “id”</td>
    </tr>
    <tr>
      <td>GET/books-on-page</td>
      <td>ApiVersion-BooksStore :  1<br>ApiVersion-BooksStore :  2</td>
      <td>Retrieves all data from database, with requested page and requested quantity per page (divides all data from database into pages) and sorts the data in ascending or descending order. For ApiVersion-BooksStore :  2 retrieves only were parameter “isAvailable” : true</td>
    </tr>
     <tr>
      <td>GET/books-available</td>
      <td>ApiVersion-BooksStore :  1</td>
      <td>Retrieves all data from database with requested available - “isAvailable”: true or false</td>
    </tr>
     <tr>
      <td>GET/books-available-page</td>
      <td>ApiVersion-BooksStore :  1</td>
      <td>Retrieves all data from database with requested page, quantity of books per page and available - “isAvailable”: true or false</td>
    </tr>
    <tr>
      <td>GET/genres</td>
      <td>ApiVersion-BooksStore :  1 <br>ApiVersion-BooksStore :  2</td>
      <td>Gets the list of all genres in database. For ApiVersion-BookStore: 2 only genres where at least one book “isAvailable” : true</td>
    </tr>
    <tr>
      <td>GET/books-equals-condition</td>
      <td>ApiVersion-BooksStore :  1<br>ApiVersion-BooksStore :  2</td>
      <td>Gets all books where at least one property equals to the entered term, For ApiVersion-BookStore: 2, only books “isAvailable” : true</td>
    </tr>
    <tr>
      <td>GET/books-equals-condition-page</td>
      <td>ApiVersion-BooksStore :  1<br>ApiVersion-BooksStore :  2</td>
      <td>Same as previous, just divides content into pages, with requested quantity per page.</td>
    </tr>
    <tr>
      <td>GET/books-contains-condition</td>
      <td>ApiVersion-BooksStore :  1<br>ApiVersion-BooksStore :  2</td>
      <td>Gets all books where at least one property contains to the entered term, For ApiVersion-BookStore: 2, only books “isAvailable” : true</td>
    </tr>
    <tr>
      <td>GET/books-contains-condition-page</td>
      <td>ApiVersion-BooksStore :  1<br>ApiVersion-BooksStore :  2</td>
      <td>Same as previous, just divides content into pages, with requested quantity per page.</td>
    </tr>
    <tr>
      <td>GET/books-filtered</td>
      <td>ApiVersion-BooksStore :  1<br>ApiVersion-BooksStore :  2</td>
      <td>Gets all books where data in each property contains entered data for each property. For ApiVersion-BookStore: 2, only books “isAvailable” : true.</td>
    </tr>
    <tr>
      <td>GET/books-filtered -page</td>
      <td>ApiVersion-BooksStore :  1<br>ApiVersion-BooksStore :  2</td>
      <td>Same as previous, just divides content into pages, with requested quantity per page.</td>
    </tr>
    <tr>
      <td>GET/all-books-count</td>
      <td>ApiVersion-BooksStore :  1<br>ApiVersion-BooksStore :  2</td>
      <td>Returns quantity of all books in database. For ApiVersion-BookStore: 2, only counts books “isAvailable” : true.</td>
    </tr>
    <tr>
      <td>GET/all-available-count</td>
      <td>ApiVersion-BooksStore :  1</td>
      <td>Returns quantity of all books in database, where parameter available - “isAvailable” : as requested.</td>
    </tr>
    <tr>
      <td>GET/books-genre-count</td>
      <td>ApiVersion-BooksStore :  1<br>ApiVersion-BooksStore :  2</td>
      <td>Returns quantity of books under requested genre. For ApiVersion-BookStore: 2, only counts books “isAvailable” : true.</td>
    </tr>
    <tr>
      <td>POST/book-add</td>
      <td>ApiVersion-BooksStore :  1</td>
      <td>Add new book/data in database. Requires entering all data, except the Id, Id generated automatically.</td>
    </tr>
    <tr>
      <td>PUT/book-update</td>
      <td>ApiVersion-BooksStore :  1</td>
      <td>Update/Change existing book/data in database, by Id. Myst be entered Id, IsAvailable. Rest data that was not entered would be kept the current. ATTENTION if need to add new genre, but keep the currents, then all genres that needs to be kept must be entered, otherwise they would be deleted or should not enter any new.</td>
    </tr>
    <tr>
      <td>DELETE/book-delete</td>
      <td>ApiVersion-BooksStore :  1</td>
      <td>Delete existing book/data from database. Requires existing Id.</td>
    </tr>
  </tbody>
</table>
