using System;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
using API.Errors;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace API.Middleware
{
    public class ExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionMiddleware> _logger;
        private readonly IHostEnvironment _env;
        public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger, IHostEnvironment env)
        {
            _env = env;
            _logger = logger;
            _next = next;

        }

        public async Task InvokeAsync(HttpContext context) {
            try {
                await _next(context);
            }
            catch(Exception ex) {
                // if we don't do this step, our exception will become silenced, and we won't see it
                _logger.LogError(ex, ex.Message);

                // write out this exception to our response
                context.Response.ContentType = "application/json";
                context.Response.StatusCode = (int) HttpStatusCode.InternalServerError; // basically it will be a 500 response

                // check if we are in dev mode, and send more details
                var response = _env.IsDevelopment()
                    // StackTrace? checks if the value is not null before calling ToString()
                    ? new ApiException (context.Response.StatusCode, ex.Message, ex.StackTrace?.ToString())
                    : new ApiException(context.Response.StatusCode, "Internal Server Error");

                // set some options
                var options = new JsonSerializerOptions{PropertyNamingPolicy = JsonNamingPolicy.CamelCase};

                // create the resonse
                var json = JsonSerializer.Serialize(response, options);

                // write the response
                await context.Response.WriteAsync(json);
            }
        }
    }
}