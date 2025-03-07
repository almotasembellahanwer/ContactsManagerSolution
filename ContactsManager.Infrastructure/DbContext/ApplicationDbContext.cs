using ContactsManager.Core.Domain.IdentityEntities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Entities
{
    public class ApplicationDbContext
        : IdentityDbContext<ApplicationUser,ApplicationRole,Guid>
    {

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {

        }
        public virtual DbSet<Country> Countries { get; set; }
        public virtual DbSet<Person> Persons { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<Country>().ToTable("Countries");
            modelBuilder.Entity<Person>().ToTable("Persons");
            //Seed countries
            string countriesJson = File.ReadAllText("countries.json");
            List<Country>? countries = JsonSerializer.Deserialize<List<Country>>(countriesJson);
            if (countries == null)
            {
                throw new ArgumentNullException(nameof(countries));
            }
            foreach (var country in countries)
            {
                modelBuilder.Entity<Country>().HasData(country!);
            }
            //Seed persons
            string personsJson = File.ReadAllText("persons.json");
            List<Person>? persons = JsonSerializer.Deserialize<List<Person>>(personsJson);
            if (persons == null)
            {
                throw new ArgumentNullException(nameof(persons));
            }
            foreach (var person in persons)
            {
                modelBuilder.Entity<Person>().HasData(person);
            }
            modelBuilder.Entity<Person>().Property(temp => temp.TIN)
                .HasColumnName("TaxidentificationNumber")
                .HasColumnType("varchar(8)")
                .HasDefaultValue("AB123456");
            //modelBuilder.Entity<Person>()
            //    .HasIndex(temp => temp.TIN).IsUnique();
            modelBuilder.Entity<Person>()
                .ToTable(t => t.HasCheckConstraint("CK_TIN", "len([TaxidentificationNumber]) = 8"));
                
        }
        public List<Person> sp_GetAllPersons()
        {
            return Persons.FromSqlRaw("EXECUTE [dbo].[GetAllPersons]").ToList();
        }
        public int sp_InsertPerson(Person person)
        {
            SqlParameter[] parameters = new SqlParameter[]
            {
                new SqlParameter("@PersonID", person.PersonID),
                new SqlParameter("@PersonName", person.PersonName),
                new SqlParameter("@Email", person.Email),
                new SqlParameter("@DateOfBirth", person.DateOfBirth),
                new SqlParameter("@Gender",person.Gender),
                new SqlParameter("@CountryID",person.CountryID),
                new SqlParameter("@Address",person.Address),
                new SqlParameter("@ReceiveNewsLetters",person.ReceiveNewsLetters),

            };
            return Database.ExecuteSqlRaw("EXECUTE [dbo].[InsertPerson] @PersonID,@PersonName,@Email,@DateOfBirth,@Gender,@CountryID,@Address,@ReceiveNewsLetters", parameters);
        }
    }
}
