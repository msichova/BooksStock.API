/*
 * Class to store the appsettings.json file's ConnectionToMongoDB property values
 * 
 */
namespace BooksStock.API.Models
{
    public class StockDatabseSettings
    {
        public string Connection { get; set; } = null!;
        public string DatabaseName { get; set; } = null!;
        public string CollectionName { get; set; } = null!;
    }
}
