using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace API.Helpers
{
    // making this as generic as posible, where T is any type
    public class PagedList<T> : List<T>
    {
      public PagedList(IEnumerable<T> items, int count, int pageNumber, int pageSize)
      {
        CurrentPage = pageNumber;

        // this calulates number of pages needed, and rounds up(ceiling)
        TotalPages = (int) Math.Ceiling(count / (double) pageSize);
        PageSize = pageSize;
        TotalCount = count;
        AddRange(items); // so that we have access to the items inside our PagedList
      }

      // what page were on
      public int CurrentPage { get; set;}
      public int TotalPages { get; set;}
      // items per page
      public int PageSize { get; set;}
      // total count of items returned
      public int TotalCount { get; set;}

      public static async Task<PagedList<T>> CreateAsync(IQueryable<T> source, int pageNumber, int pageSize) {
        // here we cant avoid making a call to the database to get the count
        var count = await source.CountAsync(); 

        // this figures how how many items to skip, to get to the "page" we want. ToListAsync() executes the query
        var items = await source.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync();

        return new PagedList<T>(items, count, pageNumber, pageSize);
      }
        
    }
}