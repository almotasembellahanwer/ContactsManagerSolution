using Microsoft.AspNetCore.Mvc.Filters;

namespace CRUDExample.Filters.ActionFilter
{
    public class ResponseHeaderFilterFactoryAttribute : Attribute,IFilterFactory
    {
        public bool IsReusable => false;
        public string Key { get; set; }
        public string Value { get; set; }
        public int Order { get; set; }
        public ResponseHeaderFilterFactoryAttribute(string key,string value,int order)
        {
            Key = key;
            Value = value;
            Order = order;
        }


        //Controller -> FilterFactory -> Filter
        public IFilterMetadata CreateInstance(IServiceProvider serviceProvider)
        {
            var filter = serviceProvider.GetRequiredService<ResponseHeaderActionFilter>();
            filter.Key = Key;
            filter.Value = Value;
            filter.Order = Order;
            return filter;
        }
    }
    public class ResponseHeaderActionFilter : IAsyncActionFilter
    {
        private readonly ILogger<ResponseHeaderActionFilter> _logger;
        public string Key { get; set; }
        public string Value { get; set; }
        public int Order { get; set; }
        public ResponseHeaderActionFilter(ILogger<ResponseHeaderActionFilter> logger)
        {
            _logger = logger;
        }

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            _logger.LogInformation("Before excecution - ResponseHeaderActionFilter.OnActionExecutionAsync");

            await next();
            _logger.LogInformation("After excecution - ResponseHeaderActionFilter.OnActionExecutionAsync");

        }

    }
}
