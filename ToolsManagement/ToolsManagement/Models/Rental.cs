using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace ToolsManagement.Models
{
    public class Rental
    {
        public int RentalId { get; set; }

        [DisplayName("Customer")]
        [Required(ErrorMessage ="Customer is required.")]
        public int CustomerId { get; set; }
        public Customer Customer { get; set; }

        [DisplayName("Tool")]
        [Required(ErrorMessage = "Tool is required.")]
        public int ToolId { get; set; }
        public Tool Tool { get; set; }

        [Required(ErrorMessage = "DateOut is required.")]
        public string DateOut { get; set; }
        public string DateIn { get; set; }


        public static Rental FromSemicolonSeparte(string semicolonLine)
        {
            string[] values = semicolonLine.Split(";".ToCharArray());
            Rental objRental = new Rental();
            objRental.RentalId = Convert.ToInt32(values[0].Trim());
            objRental.CustomerId = Convert.ToInt32(values[1].Trim());

            if (!System.IO.File.Exists(HttpContext.Current.Server.MapPath("~/DBFiles/Customers.txt")))
            {
                using (System.IO.FileStream fs = System.IO.File.Create(HttpContext.Current.Server.MapPath("~/DBFiles/Customers.txt")))
                {
                }
            }
            objRental.Customer = System.IO.File.ReadAllLines(HttpContext.Current.Server.MapPath("~/DBFiles/Customers.txt")).Select(x => Customer.FromSemicolonSeparte(x)).ToList().FirstOrDefault(x => x.CustomerId == objRental.CustomerId);

            objRental.ToolId = Convert.ToInt32(values[2].Trim());
            if (!System.IO.File.Exists(HttpContext.Current.Server.MapPath("~/DBFiles/Tools.txt")))
            {
                using (System.IO.FileStream fs = System.IO.File.Create(HttpContext.Current.Server.MapPath("~/DBFiles/Tools.txt")))
                {
                }
            }
            objRental.Tool = System.IO.File.ReadAllLines(HttpContext.Current.Server.MapPath("~/DBFiles/Tools.txt")).Select(x => Tool.FromSemicolonSeparte(x)).ToList().FirstOrDefault(x => x.ToolId == objRental.ToolId);

            objRental.DateOut = values[3].Trim().ToString();
            objRental.DateIn = values[4].Trim().ToString();
            return objRental;
        }
    }
}