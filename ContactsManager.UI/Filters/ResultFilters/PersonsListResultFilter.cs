using Microsoft.AspNetCore.Mvc.Filters;

namespace CRUDExample.Filters.ResultFilters
{
    public class PersonsListResultFilter : IAsyncResultFilter
    {
        private readonly ILogger<PersonsListResultFilter> _logger;

        public PersonsListResultFilter(ILogger<PersonsListResultFilter> logger)
        {
            _logger = logger;
        }

        public async Task OnResultExecutionAsync(ResultExecutingContext context, ResultExecutionDelegate next)
        {
            _logger.LogInformation("{FilterName}.{FilterMethod} method - before",nameof(PersonsListResultFilter),nameof(OnResultExecutionAsync));
            context.HttpContext.Response.Headers["last-modified"]
                = DateTime.Now.ToString("yyyy-MM-dd HH:mm");
            await next();
            _logger.LogInformation("{FilterName}.{FilterMethod} method - after", nameof(PersonsListResultFilter), nameof(OnResultExecutionAsync));
            

        }
    }
}
