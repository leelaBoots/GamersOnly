using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using API.Helpers;
using Microsoft.AspNetCore.Http;

namespace API.Extensions
{
    public static class HttpExtensions
    {
        // this method used to add the pagination info to the response header
        public static void AddPaginationHeader(this HttpResponse response, int currentPage,
            int itemsPerPage, int totalItems, int totalPages) {
              var paginationHeader = new PaginationHeader(currentPage, itemsPerPage, totalItems, totalPages);

              var options = new JsonSerializerOptions {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
              };

              // we must serialize this because our resonse header takes a key and a string value
              // "Pagination" is the name we decided for our custom header. In the old days custom were defined as x-pagination
              response.Headers.Add("Pagination", JsonSerializer.Serialize(paginationHeader, options));
              // "Access-Control-Expose-Headers" must be spelled exactly this way
              response.Headers.Add("Access-Control-Expose-Headers", "Pagination");

        }
    }
}