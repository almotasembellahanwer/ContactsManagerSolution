using Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RepositoryContracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Repositories
{
    public class PersonsRepository : IPersonsRepository
    {
        private readonly ApplicationDbContext _db;
        private readonly ILogger<PersonsRepository> _logger;

        public PersonsRepository(ApplicationDbContext db, ILogger<PersonsRepository> logger)
        {
            _db = db;
            _logger = logger;
        }

        public async Task<Person> AddPerson(Person person)
        {
            _db.Persons.Add(person);
            await _db.SaveChangesAsync();
            return person;
        }

        public async Task<bool> DeletePersonByPersonID(Guid? personID)
        {
            _db.Persons.RemoveRange(_db.Persons.Where(temp => temp.PersonID == personID));
            int rowsAffected = await _db.SaveChangesAsync();
            return rowsAffected > 0;
        }

        public async Task<List<Person>> GetAllPersons()
        {
            _logger.LogInformation("GetAllPersons of PersonsRepository");

            return await _db.Persons.Include("Country").ToListAsync();
        }

        public Task<List<Person>> GetFilteredPersons(Expression<Func<Person, bool>> predicate)
        {
            return _db.Persons.Where(predicate).Include("Country").ToListAsync();
        }

        public Task<Person?> GetPersonByPersonID(Guid? personID)
        {
            return _db.Persons.FirstOrDefaultAsync(temp => temp.PersonID == personID);
        }

        public async Task<Person> UpdatePerson(Person person)
        {
            Person? matchingPerson = await _db.Persons.FirstOrDefaultAsync(temp => temp.PersonID == person.PersonID);
            if(matchingPerson == null)
            {
                return person;
            }
            matchingPerson.PersonName = person.PersonName;
            matchingPerson.Email = person.Email;
            matchingPerson.DateOfBirth = person.DateOfBirth;
            matchingPerson.Gender = person.Gender;
            matchingPerson.CountryID = person.CountryID;
            matchingPerson.Address = person.Address;
            matchingPerson.ReceiveNewsLetters = person.ReceiveNewsLetters;
            int countUpdated = await _db.SaveChangesAsync();
            return matchingPerson;
        }
    }
}
