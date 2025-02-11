﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Northwind.Data.Entities
{
    public class Employee
    {
        public int EmployeeId { get; set; }
        [Required]
        public string LastName { get; set; }
        [Required]
        public string FirstName { get; set; }
        public string Title { get; set; }
        public string TitleOfCourtesy { get; set; }
        public DateTime BirthDate { get; set; }
        public DateTime HireDate { get; set; }
        public string Notes { get; set; }
        public int? ReportsTo { get; set; }

        // Navigation property
        public ICollection<Order> Orders { get; set; }

        // TODO: Refactor: Move address fields in a new class
        public string Address { get; set; }
        public string City { get; set; }
        public string Region { get; set; }
        public string PostalCode { get; set; }
        public string Country { get; set; }
        public string HomePhone { get; set; } // Rename to Phone
        public string Extension { get; set; }

        public Employee()
        {
            Orders = new List<Order>();
        }
    }
}