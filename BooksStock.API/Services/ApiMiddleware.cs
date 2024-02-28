using BooksStock.API.Controllers;
using BooksStock.API.Services.ApiKey;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding.Binders;
using MongoDB.Bson.Serialization.Serializers;
using System.Net;
using System.Text;

/**
 * Middleware 
 * First Checks the API version, if API Version specifies to 1 then:
 * Checks the if current request is authorized,
 * the presence of the right API key in request header
 * 
 **/
namespace BooksStock.API.Services
{
    public class ApiMiddleware(RequestDelegate request, IApiKeyValidator keyValidator, ILogger<ApiMiddleware> logger)
    {
        private readonly IApiKeyValidator _apiKeyValidator = keyValidator;
        private readonly RequestDelegate _requestDelegate = request;
        private readonly ILogger<ApiMiddleware> _logger = logger;

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                if (!decimal.TryParse(context.Request.Headers[ApiConstants.ApiVersionHeader], out decimal version) || version != 1 && version != 2)
                {
                    context.Response.StatusCode = (int)HttpStatusCode.ExpectationFailed;
                    _logger.LogInformation(message: @"Access declined at {@DateTime}, entered API Version is not supported", DateTime.Now);
                    return;
                }
                if (version == 1)
                {
                    if (string.IsNullOrEmpty(context.Request.Headers[ApiConstants.ApiKeyHeader]))
                    {
                        context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                        _logger.LogInformation(message: @"Access declined at {@DateTime}, API key was not provided", DateTime.Now);
                        return;
                    }
                    string? userKey = context.Request.Headers[ApiConstants.ApiKeyHeader];
                    if (!_apiKeyValidator.IsValid(userKey!))
                    {
                        context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                        _logger.LogInformation(message: @"Access declined at {@DateTime}, API key is not valid", DateTime.Now);
                        return;
                    }
                }
                await _requestDelegate(context);
            }
            catch (Exception error)
            {
                _logger.LogError(message: error.Message, args: error.StackTrace);
                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                return;
            }
        }
    }
}
