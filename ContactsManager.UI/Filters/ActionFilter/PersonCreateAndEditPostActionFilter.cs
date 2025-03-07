using CRUDExample.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Rendering;
using ServiceContracts;
using ServiceContracts.DTO;

namespace CRUDExample.Filters.ActionFilter
{
    public class PersonCreateAndEditPostActionFilter : IAsyncActionFilter
    {
        private readonly ICountriesGetterService _countriesGetterService;

        public PersonCreateAndEditPostActionFilter(ICountriesGetterService countriesGetterService)
        {
            _countriesGetterService = countriesGetterService;
        }

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            if(context.Controller is PersonsController personsController)
            {
                if (!personsController.ModelState.IsValid)
                {
                    List<CountryResponse> countries = await _countriesGetterService.GetAllCountries();
                    personsController.ViewBag.Countries = countries.Select(temp =>
                    new SelectListItem() { Text = temp.CountryName, Value = temp.CountryID.ToString() });

                    var personRequest = context.ActionArguments["personRequest"];
                    context.Result = personsController.View(personRequest); //short-circuits or skips the subsequent action filters & action method
                }
                else
                {
                    await next(); //invokes the subsequent filter or action method
                }
            }
            else
            {
                await next(); //calls the subsequent filter or action method
            }
           
        }
    }
}
