using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace VCM.Models
{
    public class ContactResponse
    {
        [Required(ErrorMessage = "Please enter your name")]
        [StringLength(100, ErrorMessage = "First and Last Name are too long!")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Please enter your email address")]
        [RegularExpression(".+\\@.+\\..+", ErrorMessage = "Please enter a valid email address")]
        [StringLength(60, ErrorMessage = "Email is too long!")]
        public string Email { get; set; }

        [RegularExpression(@"^\(?([0-9]{3})\)?[-. ]?([0-9]{3})[-. ]?([0-9]{4})$", ErrorMessage = "Not a valid phone number")]
        [StringLength(15, ErrorMessage = "Phone Number is too long!")]
        public string Phone { get; set; }


        [Required(ErrorMessage = "Please enter a subject")]
        [StringLength(50, ErrorMessage = "Subject is too long!")]
        public string Subject { get; set; }

        [Required(ErrorMessage = "Please enter a message")]
        [StringLength(1500, ErrorMessage = "Message is too long!")]
        public string Message { get; set; }
    }
}