using System;
using MyAPIv3.Models;

namespace MyAPIv3.DTOs
{
    public class PersonDto
    {
        public long Id { get; set; }
        public string FirstName { get; set; } = null!;
        public string? SecondName { get; set; }
        public string ThirdWithLastname { get; set; } = null!;
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Address { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public bool IsActive { get; set; }
        public string PersonType { get; set; }
        //public PersonTypeEnum PersonType { get; set; }
    }

    public class CreatePersonDto
    {
        public string FirstName { get; set; } = null!;
        public string? SecondName { get; set; }
        public string ThirdWithLastname { get; set; } = null!;
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Address { get; set; }
        public bool IsActive { get; set; } = true;
        public string PersonType { get; set; }
        //public PersonTypeEnum PersonType { get; set; } = PersonTypeEnum.Other;
    }

    public class UpdatePersonDto
    {
        public string FirstName { get; set; } = null!;
        public string? SecondName { get; set; }
        public string ThirdWithLastname { get; set; } = null!;
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Address { get; set; }
        public bool IsActive { get; set; }
        public string PersonType { get; set; }
        //public PersonTypeEnum PersonType { get; set; }
    }

    // DTO for updating contact info only
    public class UpdateContactDto
    {
        public string? PhoneNumber { get; set; }
        public string? Email { get; set; }
    }
}

