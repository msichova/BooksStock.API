/*
 * Interface to validate API key
 * 
 */
namespace BooksStock.API.Services.ApiKey
{
    public interface IApiKeyValidator
    {
        bool IsValid(string apiKey);
    }
}
