using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Core.Concretes.Enums;

namespace Core.Concretes.DTOs
{
    /// <summary>
    /// Customer profile update form — sent from dashboard.
    /// Only includes editable fields.
    /// </summary>
    public class CustomerProfileUpdateDTO
    {
        [Phone]
        public string? PhoneNumber { get; set; }

        [MaxLength(200)]
        public string? Address { get; set; }

        [MaxLength(100)]
        public string? City { get; set; }

        [MaxLength(1000)]
        public string? Bio { get; set; }

        public FamilyStatus? FamilyStatus { get; set; }

        public bool? HasPets { get; set; }

        public int? NumberOfPets { get; set; }
    }
}