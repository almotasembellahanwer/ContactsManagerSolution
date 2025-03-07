using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Entities
{
    // what you want to store in the database
    /// <summary>
    /// Person domain model class
    /// </summary>
    public class Person
    {
        [Key]
        public Guid PersonID { get; set; }
        [StringLength(40)]
        public string PersonName { get; set; } = string.Empty;
        [StringLength(40)]
        public string? Email { get; set; }
        [StringLength(40)]
        public DateTime? DateOfBirth { get; set; }
        [StringLength(10)]
        public string? Gender { get; set; }
        //unique identifier for the country
        public Guid? CountryID { get; set; }
        [StringLength(200)]
        public string? Address { get; set; }
        public bool ReceiveNewsLetters { get; set; }
        public string? TIN { get; set; }
        [ForeignKey("CountryID")]
        public Country? Country { get; set; }
        public override string ToString()
        {
            return $"PersonID : {PersonID}, PersonName : {PersonName}, Email : " +
                $"{Email}, DateOfBirth : {DateOfBirth}, Gender : {Gender}, CountryName : {Country!.CountryName}, Address : {Address}, ReceiveNewsLetters : {ReceiveNewsLetters}{TIN} ";
        }
    }
}
