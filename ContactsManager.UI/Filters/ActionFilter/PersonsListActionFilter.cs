using CRUDExample.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Data.SqlClient;
using ServiceContracts.DTO;
using ServiceContracts.Enums;
using System.Reflection;

namespace CRUDExample.Filters.ActionFilter
{
    public class PersonsListActionFilter : IActionFilter
    {
        private readonly ILogger<PersonsListActionFilter> _logger;

        public PersonsListActionFilter(ILogger<PersonsListActionFilter> logger)
        {
            _logger = logger;
        }

        public void OnActionExecuted(ActionExecutedContext context)
        {
            _logger.LogInformation("{FilterName}.{MethodName}"
                ,nameof(PersonsListActionFilter),nameof(OnActionExecuted));
            PersonsController personsController = (PersonsController)context.Controller;
            IDictionary<string, object?>? parameters = (IDictionary<string, object?>?)context.HttpContext.Items["arguments"];
            if(parameters != null)
            {
                if (parameters.ContainsKey("searchBy"))
                {
                    personsController.ViewData["CurrentSearchBy"]
                        = Convert.ToString(parameters["searchBy"]);
                    if (parameters.ContainsKey("searchString"))
                    {
                        personsController.ViewData["CurrentSearchString"]
                            = Convert.ToString(parameters["searchString"]);
                    }
                    if (parameters.ContainsKey("sortBy"))
                    {
                        personsController.ViewData["CurrentSortBy"]
                            = Convert.ToString(parameters["sortBy"]);

                    }
                    else
                    {
                        personsController.ViewData["CurrentSortBy"] = nameof(PersonResponse.PersonName);
                    }
                    if (parameters.ContainsKey("sortOrder"))
                    {
                        personsController.ViewData["CurrentSortOrder"]
                            = Convert.ToString(parameters["sortOrder"]);
                    }
                    else
                    {
                        personsController.ViewData["CurrentSortOrder"] = nameof(SortOrderOptions.ASC);
                    }
                }
            }
            personsController.ViewBag.SearchFields = new Dictionary<string, string>()
      {
        { nameof(PersonResponse.PersonName), "Person Name" },
        { nameof(PersonResponse.Email), "Email" },
        { nameof(PersonResponse.DateOfBirth), "Date of Birth" },
        { nameof(PersonResponse.Gender), "Gender" },
        { nameof(PersonResponse.CountryID), "Country" },
        { nameof(PersonResponse.Address), "Address" }
      };

        }

        public void OnActionExecuting(ActionExecutingContext context)
        {
            _logger.LogInformation("{FilterName}.{MethodName}"
                ,nameof(PersonsListActionFilter),nameof(OnActionExecuting));
            context.HttpContext.Items["arguments"] = context.ActionArguments;
            if (context.ActionArguments.ContainsKey("searchBy"))
            {
                string? searchBy = Convert.ToString(context.ActionArguments["searchBy"]);
                //Validate the searchBy parameter value
                if (!string.IsNullOrEmpty(searchBy))
                {
                    var searchByOption = new List<string>()
                    {
                        "PersonName",
                        "Email",
                        "DateOfBirth",
                        "Age",
                        "Gender",
                        "Country",
                        "Address",
                        "ReceiveNewsLetters"
                    };
                    //reset the searchBy parameter value
                    if(searchByOption.Any(temp=>temp == searchBy) == false)
                    {
                        _logger.LogInformation("searchBy actual value {searchBy}", searchBy);
                        context.ActionArguments["searchBy"] = nameof(PersonResponse.PersonName);
                        _logger.LogInformation("searchBy updated value {searchBy}", context.ActionArguments["searchBy"]);

                    }
                }
            }
        }
    }
}
