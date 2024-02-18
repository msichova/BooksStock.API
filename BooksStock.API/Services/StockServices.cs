/*
 * 
 * Performs database operations
 * Creates a new database with collection and copies data from a read-only database.
 * In each name of created database adds date and time it has been created and serial index number
 * Because this API maybe accessed by different users(manipulation to Collection)
 * So every time this API starts it creates the collection that user of API will interacts with
 * And copies every time data from not editable Collection to the Collecion for user API
 * Just to avoid possible issues like: empty Collection or to avoid data entered (for example profanity) 
 * by a previous user
 * 
 * !ATTENTION! Could be error: in case if Cluster over size or multiple users uses at same time
 * 
 */
using BooksStock.API.Models;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace BooksStock.API.Services
{
    public class StockServices
    {
        private readonly IMongoCollection<BookProduct> _booksCollection; 

        public StockServices(IOptions <StockDatabseSettings> settings) 
        {
            var mongoClient = new MongoClient(settings.Value.Connection);
            var mongoDatabase = mongoClient.GetDatabase(settings.Value.DatabaseName);

            //getting the current list of all databases
            List<string> listDatabasesNames = mongoClient.ListDatabaseNames().ToList().Where(name => name.StartsWith(settings.Value.DatabaseName + "_")).ToList();

            //creating new name for new database or getting previous 
            string databaseName = GetSetDatabaseName(listDatabasesNames, settings.Value.DatabaseName);

            var newUsersDatabase = mongoClient.GetDatabase(databaseName);
            if (!newUsersDatabase.ListCollectionNames().Any())
            {
                //getting data from collection to copy into new database and collection
                var content = mongoDatabase.GetCollection<BookProduct>(settings.Value.CollectionName);
                newUsersDatabase.CreateCollection(settings.Value.CollectionName);
                newUsersDatabase.GetCollection<BookProduct>(settings.Value.CollectionName).InsertMany(content.Find(_ => true).ToList());
            }

            _booksCollection = newUsersDatabase.GetCollection<BookProduct>(settings.Value.CollectionName);
        }

        #region of retrive data from DB Collection

        public async Task<List<BookProduct>> GetAllBooksAsync() =>
            await _booksCollection.Find(_ => true).ToListAsync();

        public async Task<BookProduct> GetBookByIdAsync(string id) =>
            await _booksCollection.Find(book => book.Id!.Equals(id)).FirstOrDefaultAsync();

        //returns list of books where Title,Author, Language or one of item from array of Genres EQUALS to searchCondition
        public async Task<List<BookProduct>> GetAllBooksEqualsConditionAsync(string condition) =>
            await _booksCollection.Find(book =>
            book.Author!.Equals(condition, StringComparison.OrdinalIgnoreCase) ||
            book.Title!.Equals(condition, StringComparison.OrdinalIgnoreCase) ||
            book.Language!.Equals(condition, StringComparison.OrdinalIgnoreCase) ||
            book.Genres!.ToArray().Any(genre => genre.Equals(condition, StringComparison.OrdinalIgnoreCase)
            )).ToListAsync();

        //returns list of books where Title,Author, Language or one of item from array of genres CONTAINS to searchCondition
        public async Task<List<BookProduct>> GetAllBooksContainsConditionAsync(string condition) =>
            await _booksCollection.Find(book =>
            book.Author!.ToUpper().Contains(condition.ToUpper()) ||
            book.Title!.ToUpper().Contains(condition.ToUpper()) ||
            book.Language!.ToUpper().Contains(condition.ToUpper()) ||
            book.Genres!.ToArray().Any(genre => genre.ToUpper().Contains(condition.ToUpper())
            )).ToListAsync();
        #endregion

        #region of manipulations with data from DB Collection
        public async Task AddNewAsync(BookProduct bookProduct) =>
            await _booksCollection.InsertOneAsync(bookProduct);

        public async Task UpdateNewAsync(BookProduct bookProduct) =>
            await _booksCollection.FindOneAndReplaceAsync(book => 
            book.Id!.Equals(bookProduct.Id), bookProduct);

        public async Task DeleteAsync(string id) =>
            await _booksCollection.FindOneAndDeleteAsync(book => book.Id!.Equals(id));

        #endregion

        #region of Help methods
        //creates name for new database
        private string GetSetDatabaseName(List<string> listDatabasesNames, string mainDatabaseName)
        {
            string databaseName = String.Empty;

            //getting last database name
            string lastDatabaseName = listDatabasesNames.Count > 0 ? 
                listDatabasesNames.ElementAt(listDatabasesNames.Count - 1) : String.Empty;

            if(File.Exists("databaseName.txt"))
            {
                using(StreamReader reader = new("databaseName.txt"))
                {
                    databaseName = reader.ReadLine()!;
                    reader.Close();

                    //checking if text file with name of database not empty or if database with name from text file already been deleted
                    if(string.IsNullOrEmpty(databaseName) || !listDatabasesNames.Contains(databaseName))
                    {
                        databaseName = mainDatabaseName + '_' + GetDateTimeNow() + "_" + GetDatabaseSerialNumber(lastDatabaseName);
                        SaveDatabaseName(databaseName);
                    }
                }
            }
            else
            {
                databaseName = mainDatabaseName + '_' + GetDateTimeNow() + "_" + GetDatabaseSerialNumber(lastDatabaseName);
                SaveDatabaseName(databaseName);
            }
            return databaseName;
        }

        //Saves new or updates old database name for each user, in databaseName.txt
        private void SaveDatabaseName(string databaseName)
        {
            using(StreamWriter writer = new("databaseName.txt"))
            {
                writer.WriteLine(databaseName);
                writer.Close();
            }
        }

        //gets last serial number of exists databse  and increments it by 1
        //if there nor databases with serial numbers, then just returns 1
        private int GetDatabaseSerialNumber(string databaseName)
        {
            //return string.IsNullOrEmpty(databaseName) ? 1 : int.Parse(databaseName!.Split('_').ElementAt(4)) + 1;
            int number = 1;
            if (string.IsNullOrEmpty(databaseName))
            {
                return number;
            }
            if (databaseName.Contains("PM") || databaseName.Contains("AM"))
            {
                number = int.Parse(databaseName!.Split('_').ElementAt(4)) + 1;
            }
            else
            {
                number = int.Parse(databaseName!.Split('_').ElementAt(3)) + 1;
            }
            return number;
        }

        //return current date and time to add it to new name of database
        private string GetDateTimeNow()
        {
            //return DateTime.Now.ToString().Replace('/', '-').Replace(':', '-').Replace(' ', '_');
            string dateTime = DateTime.Now.ToString().Replace(' ', '_');
            dateTime = dateTime.Replace('/', '-');
            dateTime = dateTime.Replace(':', '-');
            dateTime = dateTime.Replace('.', '-');
            return dateTime;
        }
        #endregion
    }
}
