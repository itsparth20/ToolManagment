using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace ManageTools.Models
{
    public class Customer
    {
        public int CustId { get; set; }
        [Required(ErrorMessage = "Customer Name is required.")]
        [DisplayName("Customer Name")]
        public String CustomerName { get; set; }
        public int IsDeleted { get; set; }

        public static Customer SemicolonSeparte(string semicolonLine)
        {
            string[] values = semicolonLine.Split(";".ToCharArray());
            Customer objCustomer = new Customer();
            objCustomer.CustId = Convert.ToInt32(values[0].Trim());
            objCustomer.CustomerName = values[1].Trim().ToString();
            objCustomer.IsDeleted = Convert.ToInt16(values[2].Trim());
            return objCustomer;
        }
    }
}