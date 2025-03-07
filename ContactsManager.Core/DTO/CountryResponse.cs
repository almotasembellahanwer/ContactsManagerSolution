using Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceContracts.DTO
{
    /// <summary>
    /// DTO class that is used to return type for
    /// most of CountriesService methods 
    /// </summary>
    public class CountryResponse
    {
        public Guid CountryID { get; set; }
        public string? CountryName { get; set; }
        public override bool Equals(object? obj)
        {
            if (obj == null)
            {
                return false;
            }
            if(obj.GetType() != this.GetType())
            {
                return false;
            }
            CountryResponse countryResponse = (CountryResponse)obj;
            return countryResponse.CountryID == this.CountryID
                && countryResponse.CountryName == this.CountryName;
        }
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }

    public static class CountryExtension
    {
        public static CountryResponse ToCountryResponse(this Country country)
        {
            return new CountryResponse
            {
                CountryID = country.CountryID,
                CountryName = country.CountryName
            };
        }
    }
    }
