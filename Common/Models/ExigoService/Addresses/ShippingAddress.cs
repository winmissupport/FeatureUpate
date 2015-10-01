using Common;
using System.ComponentModel.DataAnnotations;

namespace ExigoService
{
    public class ShippingAddress : Address
    {
        public ShippingAddress() { }
        public ShippingAddress(Address address)
        {
            base.AddressType = address.AddressType;
            base.Address1 = address.Address1;
            base.Address2 = address.Address2;
            base.City = address.City;
            base.State = address.State;
            base.Zip = address.Zip;
            base.Country = address.Country;
        }

        [Required(ErrorMessage = "First Name is required"), Display(Name = "First Name")]
        public string FirstName { get; set; }
        [Required(ErrorMessage = "Last Name is required"), Display(Name = "Last Name")]
        public string LastName { get; set; }
        public string Company { get; set; }
        [Required, DataType(DataType.PhoneNumber), Display(Name = "Phone Number")]
        public string Phone { get; set; }
        [Required, DataType(DataType.EmailAddress), RegularExpression(GlobalSettings.RegularExpressions.EmailAddresses, ErrorMessage = "This email doesn't look right - can you check it again?"), Display(Name = "Email")]
        public string Email { get; set; }

        public string FullName
        {
            get { return string.Join(" ", this.FirstName, this.LastName); }
        }
    }
}