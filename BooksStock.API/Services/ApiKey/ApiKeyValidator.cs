/**
 * Implements the IApiKeyValidator interface class IsValid(string)
 * and injecting the IConfiguration into the constructor
 * 
 **/
namespace BooksStock.API.Services.ApiKey
{
    public class ApiKeyValidator(IConfiguration configuration) : IApiKeyValidator
    {
        private readonly IConfiguration _configuration = configuration;

        public bool IsValid(string apiKey)
        {
            if(string.IsNullOrEmpty(apiKey)) { return false; }

            string? key = _configuration.GetValue<string>(ApiConstants.ApiKeyName);

            return !string.IsNullOrEmpty(key) && key.Equals(apiKey);
        }
    }
}
