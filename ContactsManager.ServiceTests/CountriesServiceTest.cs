using AutoFixture;
using Entities;
using FluentAssertions;
using Moq;
using RepositoryContracts;
using ServiceContracts;
using ServiceContracts.DTO;
using Services;
using System.Diagnostics.Metrics;

namespace CRUDTests
{
    public class CountriesServiceTest
    {
        private readonly ICountriesAdderService _countriesAdderService;
        private readonly ICountriesGetterService _countriesGetterService;
        private readonly ICountriesUploaderService _countriesUploaderService;

        private readonly IFixture _fixture;
        private readonly Mock<ICountriesRepository> _countriesRepositoryMock;
        private readonly ICountriesRepository _countriesRepository;
        public CountriesServiceTest()
        {
            _fixture = new Fixture();
            _countriesRepositoryMock = new Mock<ICountriesRepository>();
            _countriesRepository = _countriesRepositoryMock.Object;
            _countriesAdderService = new CountriesAdderService(_countriesRepository);
            _countriesGetterService = new CountriesGetterService(_countriesRepository);
            _countriesUploaderService = new CountriesUploaderService(_countriesRepository);

        }
        #region AddCountry
        //When CountryAddRequest is null, it should throw ArgumentNullException
        [Fact]
        public async Task AddCountry_NullCountry_ToBeArgumentException()
        {
            // Arrange
            CountryAddRequest? request = null;
            Func<Task> action = async () =>
            // Acts
            await _countriesAdderService.AddCountry(request);
            //Assert
            await action.Should().ThrowAsync<ArgumentException>();
        }

        //When the CountryName is null, it should throw ArgumentException
        [Fact]
        public async Task AddCountry_CountryNameIsNull_ToBeArgumentException()
        {
            // Arrange
            CountryAddRequest countryAddRequest = _fixture.Build<CountryAddRequest>()
                .With(temp => temp.CountryName, null as string)
                .Create();
            Country country = countryAddRequest.ToCountry();
            _countriesRepositoryMock.Setup(temp => temp.AddCountry(It.IsAny<Country>()))
                .ReturnsAsync(country);
            Func<Task> action = async () =>
            {
                // Acts
                await _countriesAdderService.AddCountry(countryAddRequest);
            };
            //Assert
            await action.Should().ThrowAsync<ArgumentException>();
        }

        //When the CountryName is duplicate, it should throw ArgumentException
        [Fact]
        public async Task AddCountry_DuplicateCountryName_ToBeArgumentException()
        {
            // Arrange
            CountryAddRequest countryAddRequest1 = _fixture.Build<CountryAddRequest>()
                    .With(temp => temp.CountryName, "India")
                    .Create();
            CountryAddRequest countryAddRequest2 = _fixture.Build<CountryAddRequest>()
                    .With(temp => temp.CountryName, "India")
                    .Create();
            Country country = countryAddRequest1.ToCountry();
            _countriesRepositoryMock.Setup(temp => temp.AddCountry(It.IsAny<Country>()))
                .ReturnsAsync(country);
            _countriesRepositoryMock.Setup(temp => temp.GetCountryByCountryName(It.IsAny<string>()))
                .ReturnsAsync(country);
            Func<Task> action = async () =>
            {
                //Acts
                await _countriesAdderService.AddCountry(countryAddRequest1);
                await _countriesAdderService.AddCountry(countryAddRequest2);

            };
            //Assert
           await action.Should().ThrowAsync<ArgumentException>();
        }

        //When you supply proper country name, it should insert (add) the country to the existing list of countries
        [Fact]
        public async Task AddCountry_FullCountry_ToBeSuccessful()
        {
            //Arrange
            CountryAddRequest? countryAddRequest = _fixture.Create<CountryAddRequest>();
            Country country = countryAddRequest.ToCountry();
            CountryResponse countryResponseExpected = country.ToCountryResponse();
            _countriesRepositoryMock.Setup(temp => temp.AddCountry(It.IsAny<Country>()))
                .ReturnsAsync(country);
            _countriesRepositoryMock.Setup(temp => temp.GetCountryByCountryName(It.IsAny<string>()))
                .ReturnsAsync(null as Country);
            // acts
            CountryResponse countryResponseFromAdd = await _countriesAdderService.AddCountry(countryAddRequest);
            country.CountryID = countryResponseFromAdd.CountryID;
            countryResponseExpected.CountryID = countryResponseFromAdd.CountryID;

            //Assert
            countryResponseFromAdd.CountryID.Should().NotBe(Guid.Empty);
            countryResponseFromAdd.Should().BeEquivalentTo(countryResponseExpected);
        }
        #endregion
        #region GetAllCountries
        //When there are no countries, it should return an empty list
        [Fact]
        public async Task GetAllCountries_EmptyList_ToBeSuccessful()
        {
            List<Country> countries = new List<Country>();
            _countriesRepositoryMock.Setup(temp => temp.GetAllCountries())
                .ReturnsAsync(countries);
            //Acts
            List<CountryResponse> actualCountryResponseList = await _countriesGetterService.GetAllCountries();
            //Assert
            actualCountryResponseList.Should().BeEmpty();
        }
        [Fact]
        // When there are countries, it should return the list of countries
        public async Task GetAllCountryDetails_FewCountries_ToBeSuccessful() {
            //Arrenge
            List<CountryAddRequest> countryAddRequestList = _fixture.CreateMany<CountryAddRequest>(3).ToList();
            List<Country> countries = countryAddRequestList.Select(temp => temp.ToCountry()).ToList();
            //Acts
            List<CountryResponse> countriesListFromAddCountry = countries.Select(temp=>temp.ToCountryResponse())
                .ToList();
            _countriesRepositoryMock.Setup(temp => temp.GetAllCountries())
                .ReturnsAsync(countries);
            List<CountryResponse> actualCountryResponceList =
                await _countriesGetterService.GetAllCountries();

            // read each element from countriesListFromAddCountry
            //Assert
            actualCountryResponceList.Should().BeEquivalentTo(countriesListFromAddCountry);
        }
        #endregion

        #region GetCountryByCountryID
        //When we supply null as countryId , it should return null as CountryResponse
        [Fact]
        public async Task GetCountryByCountryID_NullCountryID_ToBeSuccessful()
        {
            //Arrange
            Guid? countryID = null;
            //Acts
            CountryResponse? actualCountryResponse = await _countriesGetterService.GetCountryByCountryID(countryID);
            //Assert
            actualCountryResponse.Should().BeNull();
        }
        //When we supply a valid countryId, it should return the country details as CountryResponse
        [Fact]
        public async Task GetCountryByCountryID_ValidCountryID_ToBeSuccessful()
        {
            //Arrange
            CountryAddRequest countryAddRequest = _fixture.Create<CountryAddRequest>();
            Country country = countryAddRequest.ToCountry();
            CountryResponse countryResponseExpected = country.ToCountryResponse();
            _countriesRepositoryMock.Setup(temp => temp.GetCountryByCountryID(It.IsAny<Guid>()))
                .ReturnsAsync(country);
            //Acts
            CountryResponse? countryResponseFromGet = await _countriesGetterService.GetCountryByCountryID(countryResponseExpected.CountryID);
            //Assert
            Assert.Equal(countryResponseExpected, countryResponseFromGet);
            countryResponseFromGet.Should().Be(countryResponseExpected);
        }
        #endregion
    }
}
