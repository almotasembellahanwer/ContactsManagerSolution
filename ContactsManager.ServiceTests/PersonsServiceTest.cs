using AutoFixture;
using Entities;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using RepositoryContracts;
using Serilog;
using ServiceContracts;
using ServiceContracts.DTO;
using ServiceContracts.Enums;
using Services;
using System.Linq.Expressions;
using Xunit.Abstractions;

namespace CRUDTests
{
    public class PersonsServiceTest
    {
        private readonly IPersonsAdderService _personsAdderService;
        private readonly IPersonsGetterService _personsGetterService;
        private readonly IPersonsUpdaterService _personsUpdaterService;
        private readonly IPersonsDeleterService _personsDeleterService;
        private readonly IPersonsSorterService _personsSorterService;

        private readonly Mock<IPersonsRepository> _personRepositoryMock;
        private readonly IPersonsRepository _personsRepository;
        private readonly ITestOutputHelper _testOutputHelper;
        private readonly IFixture _fixture;

        public PersonsServiceTest(ITestOutputHelper testOutputHelper)
        {
            _fixture = new Fixture();
            _personRepositoryMock = new Mock<IPersonsRepository>();
            _personsRepository = _personRepositoryMock.Object;
            var loggerMock = new Mock<ILogger<PersonsAdderService>>();
            var diagnosticContextMock = new Mock<IDiagnosticContext>();
            _personsAdderService = new PersonsAdderService(_personsRepository, loggerMock.Object, diagnosticContextMock.Object);
            _personsGetterService = new PersonsGetterService(_personsRepository, loggerMock.Object, diagnosticContextMock.Object);
            _personsUpdaterService = new PersonsUpdaterService(_personsRepository, loggerMock.Object, diagnosticContextMock.Object);
            _personsDeleterService = new PersonsDeleterService(_personsRepository, loggerMock.Object, diagnosticContextMock.Object);
            _personsSorterService = new PersonsSorterService(_personsRepository, loggerMock.Object, diagnosticContextMock.Object);
            _testOutputHelper = testOutputHelper;
        }
        #region AddPerson
        // we we supply null value as PersonAddRequest, it should throw ArgumentNullException
        [Fact]
        public async Task AddPerson_NullPerson_ToBeArgumentNullException()
        {
            // Arrange
            PersonAddRequest? PersonAddRequest = null;
            // Act and Assert
            Func<Task> action = async () =>
                await _personsAdderService.AddPerson(PersonAddRequest);
            await action.Should().ThrowAsync<ArgumentNullException>();
            //await Assert.ThrowsAsync<ArgumentNullException>(async() => await _personService.AddPerson(PersonAddRequest));
        }
        // we we supply null PersonName as PersonAddRequest, it should throw ArgumentException
        [Fact]
        public async Task AddPerson_PersonNameIsNull_ToBeArgumentException()
        {
            // Arrange
            PersonAddRequest? PersonNameAddRequest = _fixture.Build<PersonAddRequest>()
                .With(temp=>temp.PersonName , null as string)
                .Create();
            Person person = PersonNameAddRequest.ToPerson();
            // When personRepository.AddPerson is called,
            // it has to return the same Person object
            _personRepositoryMock.Setup(temp => temp.AddPerson(It.IsAny<Person>())).ReturnsAsync(person);
            Func<Task> action = async () =>
                await _personsAdderService.AddPerson(PersonNameAddRequest);
            // Act and Assert
            await action.Should().ThrowAsync<ArgumentException>();
        }
        //When we supply proper person details,
        //it should insert the person into the persons list;
        //and it should return an object of PersonResponse,
        //which includes with the newly generated person id
        [Fact]
        public async Task AddPerson_FullPersonDetails_ToBeSuccessful()
        {
            // Arrange
            PersonAddRequest? personAddRequest = _fixture.Build<PersonAddRequest>()
                .With(temp=>temp.Email,"someone@gmail.com")
                .Create();
            Person person = personAddRequest.ToPerson();
            PersonResponse personResponseExpected = person.ToPersonResponse();

            // If we supply any argument value to the AddPerson method,
            // it should return the same return value
            _personRepositoryMock.Setup(temp => temp.AddPerson(It.IsAny<Person>()))
                .ReturnsAsync(person);

            // Act
            PersonResponse? person_response_from_add = await
                _personsAdderService.AddPerson(personAddRequest);
            personResponseExpected.PersonID = person_response_from_add!.PersonID;

            //Assert
            person_response_from_add.Should().NotBe(Guid.Empty);
            person_response_from_add.Should().Be(personResponseExpected);


        }
        #endregion

        #region GetPersonByPersonId
        //When we supply null as personId we should supply null as personResponse
        [Fact]
        public async Task GetPersonByPersonId_NullPersonID_ToBeNull()
        {
            //Arrenge
            Guid? personID = null;
            //Act
            PersonResponse? personResponseFromGet = await _personsGetterService.GetPersonByPersonID(personID);
            personResponseFromGet.Should().BeNull();
        }
        // when we supply a valid personId, it should returns the person details
        [Fact]
        public async Task GetPersonByPersonId_WithPersonId_ToBeSuccessful()
        {
            //// Arrenge
            Person? person = _fixture.Build<Person>()
                .With(temp=>temp.Email,"example@gmail.com").Create();
            PersonResponse personResponseExpected = person.ToPersonResponse();

            _personRepositoryMock.Setup(temp => temp.GetPersonByPersonID(It.IsAny<Guid>()))
                .ReturnsAsync(person);
            //Act
            PersonResponse? personResponseFromGet = await _personsGetterService.GetPersonByPersonID(personResponseExpected?.PersonID);
            personResponseFromGet.Should().Be(personResponseExpected);

        }
        #endregion

        #region GetAllPersons
        // When the persons list is empty, it should return an empty list
        [Fact]
        public async Task GetAllPersons_ToBeEmptyList()
        {
            List<Person> persons = new List<Person>();
            _personRepositoryMock.Setup(temp => temp.GetAllPersons())
            .ReturnsAsync(persons);
            //Act
            List<PersonResponse> personsList = await _personsGetterService.GetAllPersons();
            personsList.Should().BeEmpty();
        }
        //First, we will add few persons; and then when we call GetAllPersons(),
        //it should return the same persons that were added
        [Fact]
        public async Task GetAllPersons_WithFewPersons_ToBeSuccessful()
        {
            //Arrenge
            List<Person> persons = new List<Person>()
            {
                _fixture.Build<Person>()
                .With(temp => temp.Email, "someone_1@gmail.com")
                .With(temp=>temp.Country,null as Country).Create(),

            _fixture.Build<Person>()
                .With(temp => temp.Email, "someone_2@gmail.com")
                .With(temp => temp.Country, null as Country).Create(),

             _fixture.Build<Person>()
                .With(temp => temp.Email, "someone_3@gmail.com")
                .With(temp => temp.Country, null as Country).Create()
            };


            List<PersonResponse> expectedPersonsList = persons.Select(temp => temp.ToPersonResponse()).ToList();
            _personRepositoryMock.Setup(temp => temp.GetAllPersons())
            .ReturnsAsync(persons);
            _testOutputHelper.WriteLine("expected :");
            foreach (var expectedPersons in expectedPersonsList)
            {
                _testOutputHelper.WriteLine(expectedPersons.ToString());
            }
            //act
            List<PersonResponse> actualPersonResponseList = await _personsGetterService.GetAllPersons();
            _testOutputHelper.WriteLine("actual :");
            foreach (var actualPersonResponse in actualPersonResponseList)
            {
                _testOutputHelper.WriteLine(actualPersonResponse.ToString());
            }
            //Assert
            actualPersonResponseList.Should().BeEquivalentTo(expectedPersonsList);
        }
        #endregion
        #region GetFilteredPersons
        //If the search text is empty and search by is "PersonName"
        //, it should return all persons
        [Fact]
        public async Task GetFilteredPersons_EmptySearchedText_ToBeSuccessful()
        {
            //Arrenge
            List<Person> persons = new List<Person>()
            {
                _fixture.Build<Person>()
                .With(temp => temp.Email, "someone_1@gmail.com")
                .With(temp=>temp.Country,null as Country).Create(),

            _fixture.Build<Person>()
                .With(temp => temp.Email, "someone_2@gmail.com")
                .With(temp => temp.Country, null as Country).Create(),

             _fixture.Build<Person>()
                .With(temp => temp.Email, "someone_3@gmail.com")
                .With(temp => temp.Country, null as Country).Create()
            };


            List<PersonResponse> expectedPersonsList = persons.Select(temp => temp.ToPersonResponse()).ToList();
            _personRepositoryMock.Setup(temp => temp.GetFilteredPersons(It.IsAny<Expression<Func<Person, bool>>>()))
    .ReturnsAsync(persons);
            _testOutputHelper.WriteLine("expected :");
            foreach (var expectedPersons in expectedPersonsList)
            {
                _testOutputHelper.WriteLine(expectedPersons.ToString());
            }
            List<PersonResponse> actualPersonResponseList = await _personsGetterService.GetFilteredPersons(nameof(PersonResponse.PersonName),"");
            _testOutputHelper.WriteLine("actual :");
            foreach (var actualPersonResponse in actualPersonResponseList)
            {
                _testOutputHelper.WriteLine(actualPersonResponse.ToString());
            }
            //foreach (var expectedPersonItem in expectedPersonsList)
            //{
            //    //Assert
            //    Assert.Contains(expectedPersonItem, actualPersonResponseList);
            //}
            actualPersonResponseList.Should().BeEquivalentTo(expectedPersonsList);
        }

        //First we will add few persons; and then
        //we will search based on person name with some search string.
        //It should return the matching persons
        [Fact]
        public async Task GetFilteredPersons_SearchByPersonName_ToBeSuccessful()
        {
            //Arrenge
            List<Person> persons = new List<Person>()
            {
                _fixture.Build<Person>()
                .With(temp => temp.Email, "someone_1@gmail.com")
                .With(temp=>temp.PersonName,"mohammed")
                .With(temp=>temp.Country,null as Country).Create(),

            _fixture.Build<Person>()
                .With(temp => temp.Email, "someone_2@gmail.com")
                .With(temp=>temp.PersonName,"mohammed")
                .With(temp => temp.Country, null as Country).Create(),

             _fixture.Build<Person>()
                .With(temp => temp.Email, "someone_3@gmail.com")
                .With(temp=>temp.PersonName,"mohammed")
                .With(temp => temp.Country, null as Country).Create()
            };


            List<PersonResponse> expectedPersonsList = persons.Select(temp => temp.ToPersonResponse()).ToList();
            _personRepositoryMock.Setup(temp => temp.GetFilteredPersons(It.IsAny<Expression<Func<Person, bool>>>()))
                .ReturnsAsync(persons);
            _testOutputHelper.WriteLine("expected :");
            foreach (var expectedPersons in expectedPersonsList)
            {
                _testOutputHelper.WriteLine(expectedPersons.ToString());
            }
            List<PersonResponse> actualPersonResponseList = await _personsGetterService.GetFilteredPersons(nameof(PersonResponse.PersonName), "ed");
            _testOutputHelper.WriteLine("actual :");
            foreach (var actualPersonResponse in actualPersonResponseList)
            {
                _testOutputHelper.WriteLine(actualPersonResponse.ToString());
            }

            // Assert
            actualPersonResponseList.Should().OnlyContain(temp => temp.PersonName!.Contains("ed", StringComparison.OrdinalIgnoreCase));
        }
        #endregion
        #region GetSortedPersons
        //When we sort based on PersonName in DESC,
        //it should return persons list in descending on PersonName
        [Fact]
        public async Task GetSortedPersons_ToBeSuccessful()
        {
            //Arrenge
            List<Person> persons = new List<Person>()
            {
                _fixture.Build<Person>()
                .With(temp => temp.Email, "someone_1@gmail.com")
                .With(temp=>temp.Country,null as Country).Create(),

            _fixture.Build<Person>()
                .With(temp => temp.Email, "someone_2@gmail.com")
                .With(temp => temp.Country, null as Country).Create(),

             _fixture.Build<Person>()
                .With(temp => temp.Email, "someone_3@gmail.com")
                .With(temp => temp.Country, null as Country).Create()
            };


            List<PersonResponse> expectedPersonsList = persons.Select(temp => temp.ToPersonResponse()).ToList();

            _personRepositoryMock.Setup(temp => temp.GetAllPersons())
                .ReturnsAsync(persons);
      
            _testOutputHelper.WriteLine("expected :");
            foreach (var expectedPersons in expectedPersonsList)
            {
                _testOutputHelper.WriteLine(expectedPersons.ToString());
            }

            List<PersonResponse> allPersons = await _personsGetterService.GetAllPersons();
            List<PersonResponse> actualPersonResponseListFromSort = await _personsSorterService.GetSortedPersons(allPersons,nameof(PersonResponse.PersonName), SortOrderOptions.DESC);
            //expectedPersonsListFromAdd = expectedPersonsListFromAdd.OrderByDescending(p => p.PersonName).ToList();
            _testOutputHelper.WriteLine("actual :");
            foreach (var actualPersonResponse in actualPersonResponseListFromSort)
            {
                _testOutputHelper.WriteLine(actualPersonResponse.ToString());
            }


            // Assert
            actualPersonResponseListFromSort.Should().BeInDescendingOrder(p => p.PersonName);
        }
        #endregion

        #region UpdatePerson
        //When we supply null as PersonUpdateRequest, it should throw ArgumentNullException
        [Fact]
        public async Task UpdatePerson_NullPerson_ToBeArgumentNullException()
        {
            //Arrenge
            PersonUpdateRequest? expectedPersonUpdate = null;
            Func<Task> action = async () =>
            {
                //Act
                await _personsUpdaterService.UpdatePerson(expectedPersonUpdate);
            };
            //Assert
            await action.Should().ThrowAsync<ArgumentException>();
        }
        //When we supply invalid person id, it should throw ArgumentException
        [Fact]
        public async Task UpdatePerson_InvalidPersonID_ToBeArgumentException()
        {


            //Arrenge
            PersonUpdateRequest? personUpdateID = _fixture.Create<PersonUpdateRequest>();
            Func<Task> action = async () =>
            {
                //Act
                await _personsUpdaterService.UpdatePerson(personUpdateID);
            };
            //Assert
            await action.Should().ThrowAsync<ArgumentException>();
        }

        //When PersonName is null, it should throw ArgumentException
        [Fact]
        public async Task UpdatePerson_PersonNameIsNull_ToBeArgumentException()
        {

            //Act
            Person person = _fixture.Build<Person>()
                .With(temp => temp.PersonName, null as string)
                .With(temp => temp.Email, "someone@gmail.com")
                .With(temp=>temp.Gender,"Male")
                .With(temp => temp.Country, null as Country).Create();
            PersonResponse? personResponseExpected = person.ToPersonResponse();
            //Act
            PersonUpdateRequest? personFromUpdate = personResponseExpected!.ToPersonUpdateRequest();
            Func<Task> action = async () =>
            {
                //Act
                await _personsUpdaterService.UpdatePerson(personFromUpdate);
            };
            //Assert
            await action.Should().ThrowAsync<ArgumentException>();
        }
        //First, add a new person and try to update the person name and email
        [Fact]
        public async Task UpdatePerson_PersonFullDetailsUpdation_ToBeSuccessful()
        {
            //Act
            Person? person = _fixture.Build<Person>()
                .With(temp => temp.Email, "example@gmail.com")
                .With(temp => temp.Gender, "Male")
                .With(temp=>temp.Country,null as Country)
                .Create();
            PersonResponse? personResponseExpected = person.ToPersonResponse();
            PersonUpdateRequest? personFromUpdate = personResponseExpected!.ToPersonUpdateRequest();
            _personRepositoryMock.Setup(temp => temp.UpdatePerson(It.IsAny<Person>()))
                .ReturnsAsync(person);
            _personRepositoryMock.Setup(temp => temp.GetPersonByPersonID(It.IsAny<Guid>()))
                .ReturnsAsync(person);
            PersonResponse? personResponseFromUpdate = await _personsUpdaterService.UpdatePerson(personFromUpdate);
            //Assert
            personResponseFromUpdate.Should().Be(personResponseExpected);
        }
        #endregion

        #region DeletePerson
        [Fact]
        public async Task DeletePerson_ValidPersonID()
        {
            //Arrange
            Person person = _fixture.Build<Person>()
             .With(temp => temp.Email, "someone@example.com")
             .With(temp => temp.Country, null as Country)
             .With(temp => temp.Gender, "Female")
             .Create();


            _personRepositoryMock
             .Setup(temp => temp.DeletePersonByPersonID(It.IsAny<Guid>()))
             .ReturnsAsync(true);

            _personRepositoryMock
             .Setup(temp => temp.GetPersonByPersonID(It.IsAny<Guid>()))
             .ReturnsAsync(person);

            //Act
            bool isDeleted = await _personsDeleterService.DeletePerson(person.PersonID);

            //Assert
            isDeleted.Should().BeTrue();
        }
        [Fact]
        public async Task DeletePerson_InvalidPersonID()
        {
            //Act
            bool isDeleted = await _personsDeleterService.DeletePerson(Guid.NewGuid());

            //Assert
            isDeleted.Should().BeFalse();
        }
        #endregion
    }
}
