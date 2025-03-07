using AutoFixture;
using Castle.Core.Logging;
using CRUDExample.Controllers;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Rotativa.AspNetCore;
using ServiceContracts;
using ServiceContracts.DTO;
using ServiceContracts.Enums;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CRUDTests
{
    public class PersonsControllerTest
    {
        private readonly IFixture _fixture;
        private readonly Mock<ICountriesGetterService> _countriesGetterServiceMock;
        private readonly Mock<IPersonsAdderService> _personsAdderServiceMock;
        private readonly Mock<IPersonsGetterService> _personsGetterServiceMock;
        private readonly Mock<IPersonsUpdaterService> _personsUpdaterServiceMock;
        private readonly Mock<IPersonsDeleterService> _personsDeleterServiceMock;
        private readonly Mock<IPersonsSorterService> _personsSorterServiceMock;

        private readonly ICountriesGetterService _countriesGetterService;
        private readonly IPersonsAdderService _personsAdderService;
        private readonly IPersonsGetterService _personsGetterService;
        private readonly IPersonsUpdaterService _personsUpdaterService;
        private readonly IPersonsDeleterService _personsDeleterService;
        private readonly IPersonsSorterService _personsSorterService;

        private readonly Mock<ILogger<PersonsController>> _loggerMock;
        private readonly ILogger<PersonsController> _logger;
        public PersonsControllerTest()
        {
            _fixture = new Fixture();
            _countriesGetterServiceMock = new Mock<ICountriesGetterService>();

            _personsAdderServiceMock = new Mock<IPersonsAdderService>();
            _personsGetterServiceMock = new Mock<IPersonsGetterService>();
            _personsUpdaterServiceMock = new Mock<IPersonsUpdaterService>();
            _personsDeleterServiceMock = new Mock<IPersonsDeleterService>();
            _personsSorterServiceMock = new Mock<IPersonsSorterService>();

            _countriesGetterService = _countriesGetterServiceMock.Object;

            _personsAdderService = _personsAdderServiceMock.Object;
            _personsGetterService = _personsGetterServiceMock.Object;
            _personsUpdaterService = _personsUpdaterServiceMock.Object;
            _personsDeleterService = _personsDeleterServiceMock.Object;
            _personsSorterService = _personsSorterServiceMock.Object;

            _loggerMock = new Mock<ILogger<PersonsController>>();
            _logger = _loggerMock.Object;
        }
        #region Index
        [Fact]
        public async Task Index_ShouldReturnIndexViewWithPersonsList()
        {
            //Arrenge
            List<PersonResponse> personsResponseList = _fixture.Create<List<PersonResponse>>();
            // Mocking the service
            _personsGetterServiceMock.Setup(temp => temp.GetFilteredPersons(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(personsResponseList);
            _personsSorterServiceMock.Setup(temp => temp.GetSortedPersons(It.IsAny<List<PersonResponse>>(), It.IsAny<string>(), It.IsAny<SortOrderOptions>()))
                .ReturnsAsync(personsResponseList);
            //Act
            PersonsController personsController = new PersonsController(_countriesGetterService, _logger, _personsAdderService, _personsGetterService,_personsUpdaterService,_personsDeleterService,_personsSorterService);
            IActionResult result = await personsController.Index(_fixture.Create<string>(), _fixture.Create<string>(), _fixture.Create<string>(), _fixture.Create<SortOrderOptions>());
            //Assert
            ViewResult viewResult = Assert.IsType<ViewResult>(result);
            viewResult.Model.Should().BeAssignableTo<List<PersonResponse>>();
            viewResult.Model.Should().Be(personsResponseList);
        }
        #endregion

        #region Create
        [Fact]
        public async Task Create_IfNoModelErrors_ToReturnRedirectToIndex()
        {
            //Arrenge
            PersonAddRequest personAddRequest = _fixture.Create<PersonAddRequest>();
            PersonResponse personResponse = _fixture.Create<PersonResponse>();
            List<CountryResponse> countries = _fixture.Create<List<CountryResponse>>();
            // Mocking the service
            
            _personsAdderServiceMock.Setup(temp => temp.AddPerson(It.IsAny<PersonAddRequest>()))
                .ReturnsAsync(personResponse);

            PersonsController personsController = new PersonsController(_countriesGetterService, _logger, _personsAdderService, _personsGetterService, _personsUpdaterService, _personsDeleterService, _personsSorterService);
            //Act
            IActionResult result = await personsController.Create(personAddRequest);
            //Assert
            RedirectToActionResult redirectResult = Assert.IsType<RedirectToActionResult>(result);
            redirectResult.ActionName.Should().Be("Index");

        }
        #endregion

        #region Edit
        [Fact]
        public async Task Edit_ToReturnEditView()
        {
            //Arrenge
            PersonUpdateRequest personUpdateRequest = _fixture.Create<PersonUpdateRequest>();
            PersonResponse personResponse = _fixture.Build<PersonResponse>().With(temp=>temp.Gender,"Male").Create();
            List<CountryResponse> countries = _fixture.Create<List<CountryResponse>>();
            // Mocking the service
            _countriesGetterServiceMock.Setup(temp => temp.GetAllCountries())
                .ReturnsAsync(countries);
            _personsGetterServiceMock.Setup(temp => temp.GetPersonByPersonID(It.IsAny<Guid>()))
                .ReturnsAsync(personResponse);
            PersonsController personsController = new PersonsController(_countriesGetterService, _logger, _personsAdderService, _personsGetterService, _personsUpdaterService, _personsDeleterService, _personsSorterService);
            IActionResult result = await personsController.Edit(personResponse.PersonID);
            //Assert
            ViewResult viewResult = Assert.IsType<ViewResult>(result);
            viewResult.ViewData.Model.Should().BeAssignableTo<PersonUpdateRequest>();
        }
        [Fact]
        public async Task Edit_IfNoModelErrors_ToReturnRedirectToIndex()
        {
            //Arrenge
            PersonUpdateRequest personUpdateRequest = _fixture.Create<PersonUpdateRequest>();
            PersonResponse personResponse = _fixture.Build<PersonResponse>().With(temp => temp.Gender, "Male").Create();
            List<CountryResponse> countries = _fixture.Create<List<CountryResponse>>();
            // Mocking the service
            _personsGetterServiceMock.Setup(temp => temp.GetPersonByPersonID(It.IsAny<Guid>()))
                .ReturnsAsync(personResponse);
            _personsUpdaterServiceMock.Setup(temp => temp.UpdatePerson(It.IsAny<PersonUpdateRequest>()))
                .ReturnsAsync(personResponse);
            PersonsController personsController = new PersonsController(_countriesGetterService, _logger, _personsAdderService, _personsGetterService, _personsUpdaterService, _personsDeleterService, _personsSorterService);
            IActionResult result = await personsController.Edit(personUpdateRequest);
            //Assert
            RedirectToActionResult redirectResult = Assert.IsType<RedirectToActionResult>(result);
            redirectResult.ActionName.Should().Be("Index");
        }

        #endregion
        #region Delete
        [Fact]
        public async Task Delete_ToReturnDeleteView()
        {
            //Arrenge
            PersonResponse? personResponse = _fixture.Create<PersonResponse>();
            _personsGetterServiceMock.Setup(temp => temp.GetPersonByPersonID(It.IsAny<Guid>()))
                .ReturnsAsync(personResponse);
            PersonsController personsController = new PersonsController(_countriesGetterService, _logger, _personsAdderService, _personsGetterService, _personsUpdaterService, _personsDeleterService, _personsSorterService);
            //Act
            IActionResult result = await personsController.Delete(personResponse.PersonID);
            //Assert
            ViewResult viewResult = Assert.IsType<ViewResult>(result);
            viewResult.ViewData.Model.Should().BeAssignableTo<PersonResponse>();
        }
        [Fact]
        public async Task Delete_ToRedirectToIndex()
        {
            //Arrenge
            PersonUpdateRequest personUpdateRequest = _fixture.Create<PersonUpdateRequest>();
            PersonResponse? personResponse = _fixture.Create<PersonResponse>();
            _personsGetterServiceMock.Setup(temp => temp.GetPersonByPersonID(It.IsAny<Guid>()))
                .ReturnsAsync(personResponse);
            PersonsController personsController = new PersonsController(_countriesGetterService, _logger, _personsAdderService, _personsGetterService, _personsUpdaterService, _personsDeleterService, _personsSorterService);
            //Act
            IActionResult result = await personsController.Delete(personUpdateRequest);
            //Assert
            RedirectToActionResult redirectResult = Assert.IsType<RedirectToActionResult>(result);
            redirectResult.ActionName.Should().Be("Index");
        }
        #endregion
        #region PersonsPDF
        [Fact]
        public async Task PersonsPDF_ToReturnViewPDF()
        {
            //Arrenge
            List<PersonResponse> personResponseList = _fixture.Create<List<PersonResponse>>();
            _personsGetterServiceMock.Setup(temp => temp.GetAllPersons())
                .ReturnsAsync(personResponseList);
            PersonsController personsController = new PersonsController(_countriesGetterService, _logger, _personsAdderService, _personsGetterService, _personsUpdaterService, _personsDeleterService, _personsSorterService);
            //Act
            IActionResult result = await personsController.PersonsPDF();
            //Assert
            ViewAsPdf viewPDF = Assert.IsType<ViewAsPdf>(result);
        }
        #endregion

        #region PersonsCSV
        [Fact]
        public async Task PersonsCSV_ToReturnFileStreamResult()
        {
            //Arrenge
            List<PersonResponse> personResponseList = _fixture.Create<List<PersonResponse>>();
            MemoryStream memoryStream = new MemoryStream();
            _personsGetterServiceMock.Setup(temp => temp.GetPersonsCSV())
                .ReturnsAsync(memoryStream);
            PersonsController personsController = new PersonsController(_countriesGetterService, _logger, _personsAdderService, _personsGetterService, _personsUpdaterService, _personsDeleterService, _personsSorterService);
            //Act
            IActionResult result = await personsController.PersonsCSV();
            //Assert
            FileStreamResult fileResult = Assert.IsType<FileStreamResult>(result);
            fileResult.ContentType.Should().Be("application/octet-stream");
        }
        #endregion

        #region PersonsExcel
        [Fact]
        public async Task PersonsExcel_ToReturnFileStreamResult()
        {
            //Arrenge
            List<PersonResponse> personResponseList = _fixture.Create<List<PersonResponse>>();
            MemoryStream memoryStream = new MemoryStream();
            _personsGetterServiceMock.Setup(temp => temp.GetPersonsExcel())
                .ReturnsAsync(memoryStream);
            PersonsController personsController = new PersonsController(_countriesGetterService, _logger, _personsAdderService, _personsGetterService, _personsUpdaterService, _personsDeleterService, _personsSorterService);
            //Act
            IActionResult result = await personsController.PersonsExcel();
            //Assert
            FileStreamResult fileResult = Assert.IsType<FileStreamResult>(result);
            fileResult.ContentType.Should().Be("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
        }
        #endregion
    }
}
