using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using ServiceContracts.DTO;
using ServiceContracts.Enums;
using ServiceContracts;
using Rotativa.AspNetCore;
using CRUDExample.Filters.ActionFilter;
using CRUDExample.Filters.ResultFilters;
using CRUDExample.Filters.ResourceFilters;
using CRUDExample.Filters.AuthorizationFilter;
using CRUDExample.Filters.ExceptionFilters;

namespace CRUDExample.Controllers
{
    [Route("[controller]")]
    [ResponseHeaderFilterFactoryAttribute("My-Key-From-Controller", "My-Value-From-Controller", 3)]
    [TypeFilter(typeof(HandleExceptionFilter))]
    public class PersonsController : Controller
    {
        //private fields
        private readonly IPersonsAdderService _personsAdderService;
        private readonly IPersonsGetterService _personsGetterService;
        private readonly IPersonsUpdaterService _personsUpdaterService;
        private readonly IPersonsDeleterService _personsDeleterService;
        private readonly IPersonsSorterService _personsSorterService;

        private readonly ICountriesGetterService _countriesGetterService;
        private readonly ILogger<PersonsController> _logger;

        #region Constructor

        public PersonsController(ICountriesGetterService countriesGetterService, ILogger<PersonsController> logger, IPersonsAdderService personsAdderService, IPersonsGetterService personsGetterService, IPersonsUpdaterService personsUpdaterService, IPersonsDeleterService personsDeleterService, IPersonsSorterService personsSorterService)
        {
            _countriesGetterService = countriesGetterService;
            _logger = logger;
            _personsAdderService = personsAdderService;
            _personsGetterService = personsGetterService;
            _personsUpdaterService = personsUpdaterService;
            _personsDeleterService = personsDeleterService;
            _personsSorterService = personsSorterService;
        }
        #endregion
        #region Index Action

        //Url: persons/index
        [Route("[action]")]
        [Route("/")]
        [TypeFilter(typeof(PersonsListActionFilter), Order = 4)]
        [ResponseHeaderFilterFactoryAttribute("MyKey-FromAction", "MyValue-From-Action", 1)]
        [TypeFilter(typeof(PersonsListResultFilter))]
        public async Task<IActionResult> Index(string searchBy, string? searchString,
            string sortBy = nameof(PersonResponse.PersonName), SortOrderOptions sortOrder = SortOrderOptions.ASC)
        {
            _logger.LogInformation("Index action comes from personsController");
            _logger.LogDebug($"searchBy : {searchBy} searchString : {searchString}" +
                $" sortBy : {sortBy} sortOrder : {sortOrder}");

            //Search
            List<PersonResponse> persons = await _personsGetterService.GetFilteredPersons(searchBy, searchString);

            //Sort
            List<PersonResponse> sortedPersons = await _personsSorterService.GetSortedPersons(persons, sortBy, sortOrder);

            return View(sortedPersons); //Views/Persons/Index.cshtml
        }

        #endregion
        #region Create Action

        //Executes when the user clicks on "Create Person" hyperlink (while opening the create view)
        //Url: persons/create
        [Route("[action]")]
        [HttpGet]
        [ResponseHeaderFilterFactoryAttribute("my-key", "my-value" ,5)]

        public async Task<IActionResult> Create()
        {
            List<CountryResponse> countries = await _countriesGetterService.GetAllCountries();
            ViewBag.Countries = countries.Select(temp =>
              new SelectListItem() { Text = temp.CountryName, Value = temp.CountryID.ToString() }
            );

            //new SelectListItem() { Text="Harsha", Value="1" }
            //<option value="1">Harsha</option>
            return View();
        }

        [HttpPost]
        //Url: persons/create
        [Route("[action]")]
        [TypeFilter(typeof(PersonCreateAndEditPostActionFilter))]
        [TypeFilter(typeof(FeatureDisabledResourceFilter))]
        public async Task<IActionResult> Create(PersonAddRequest personRequest)
        {

            //call the service method
            PersonResponse? personResponse = await _personsAdderService.AddPerson(personRequest);
            if(personResponse == null)
            {
                return RedirectToAction(nameof(Index));
            }

            //navigate to Index() action method (it makes another get request to "persons/index"
            return RedirectToAction("Index", "Persons");
        }
        #endregion
        #region Edit Action

        [HttpGet]
        [Route("[action]/{personID}")] //Eg: /persons/edit/1
        [TypeFilter(typeof(TokenResultFilter))]
        public async Task<IActionResult> Edit(Guid personID)
        {
            PersonResponse? personResponse = await _personsGetterService.GetPersonByPersonID(personID);
            if (personResponse == null)
            {
                return RedirectToAction("Index");
            }

            PersonUpdateRequest personUpdateRequest = personResponse.ToPersonUpdateRequest();

            List<CountryResponse> countries = await _countriesGetterService.GetAllCountries();
            ViewBag.Countries = countries.Select(temp =>
            new SelectListItem() { Text = temp.CountryName, Value = temp.CountryID.ToString() });

            return View(personUpdateRequest);
        }


        [HttpPost]
        [Route("[action]/{personID}")]
        [TypeFilter(typeof(PersonCreateAndEditPostActionFilter))]
        [TypeFilter(typeof(TokenAuthorizationFilter))]
        public async Task<IActionResult> Edit(PersonUpdateRequest personRequest)
        {
            PersonResponse? personResponse = await _personsGetterService.GetPersonByPersonID(personRequest.PersonID);

            if (personResponse == null)
            {
                return RedirectToAction("Index");
            }
            PersonResponse? updatedPerson = await _personsUpdaterService.UpdatePerson(personRequest);
                
                return RedirectToAction("Index");
        }
        #endregion
        #region Delete Action

        [HttpGet]
        [Route("[action]/{personID}")]
        public async Task<IActionResult> Delete(Guid? personID)
        {
            PersonResponse? personResponse = await _personsGetterService.GetPersonByPersonID(personID);
            if (personResponse == null)
                return RedirectToAction("Index");

            return View(personResponse);
        }

        [HttpPost]
        [Route("[action]/{personID}")]
        public async Task<IActionResult> Delete(PersonUpdateRequest personUpdateResult)
        {
            PersonResponse? personResponse = await _personsGetterService.GetPersonByPersonID(personUpdateResult.PersonID);
            if (personResponse == null)
                return RedirectToAction("Index");

            await _personsDeleterService.DeletePerson(personUpdateResult.PersonID);
            return RedirectToAction("Index");
        }

        #endregion
        #region PersonsPDF Action

        [Route("PersonsPDF")]
        public async Task<IActionResult> PersonsPDF()
        {
            //Get list of persons
            List<PersonResponse> persons = await _personsGetterService.GetAllPersons();

            //Return view as pdf
            return new ViewAsPdf("PersonsPDF", persons.ToString(), ViewData)
            {
                PageMargins = new Rotativa.AspNetCore.Options.Margins() { Top = 20, Right = 20, Bottom = 20, Left = 20 },
                PageOrientation = Rotativa.AspNetCore.Options.Orientation.Landscape
            };
        }
        #endregion
        #region PersonsCSV Action

        [Route("PersonsCSV")]
        public async Task<IActionResult> PersonsCSV()
        {
            MemoryStream memoryStream = await _personsGetterService.GetPersonsCSV();
            return File(memoryStream, "application/octet-stream", "persons.csv");
        }
        #endregion
        #region PersonsExcel Action

        [Route("PersonsExcel")]
        public async Task<IActionResult> PersonsExcel()
        {
            MemoryStream memoryStream = await _personsGetterService.GetPersonsExcel();
            return File(memoryStream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "persons.xlsx");
        }
        #endregion
    }
}
