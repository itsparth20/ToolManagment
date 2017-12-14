using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace ToolsManagement.Models
{
    public class Customer
    {
        public int CustomerId { get; set; }
        [Required(ErrorMessage = "Customer Name is required.")]
        public String Name { get; set; }
        public int Deleted { get; set; }
        //public List<Rental> Rentals { get; set; }

        public static Customer FromSemicolonSeparte(string semicolonLine)
        {
            string[] values = semicolonLine.Split(";".ToCharArray());
            Customer objCustomer = new Customer();
            objCustomer.CustomerId = Convert.ToInt32(values[0].Trim());
            objCustomer.Name = values[1].Trim().ToString();
            objCustomer.Deleted = Convert.ToInt16(values[2].Trim());

            //if (!System.IO.File.Exists(HttpContext.Current.Server.MapPath("~/DBFiles/Rental_data.txt")))
            //{
            //    using (System.IO.FileStream fs = System.IO.File.Create(HttpContext.Current.Server.MapPath("~/DBFiles/Rental_data.txt")))
            //    {
            //    }
            //}
            //objCustomer.Rentals = System.IO.File.ReadAllLines(HttpContext.Current.Server.MapPath("~/DBFiles/Rental_data.txt")).Select(x => Rental.FromSemicolonSeparte(x)).Where(x=> x.CustomerId == objCustomer.CustomerId).ToList();

            return objCustomer;
        }
    }
}