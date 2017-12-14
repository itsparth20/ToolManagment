using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace ManageTools.Models
{
    public class Rental
    {
        public int RentalId { get; set; }

        [DisplayName("Customer")]
        [Required(ErrorMessage ="Customer is required.")]
        public int CustId { get; set; }
        public Customer Customer { get; set; }

        [DisplayName("Tool")]
        [Required(ErrorMessage = "Tool is required.")]
        public int ToolId { get; set; }
        public Tool Tool { get; set; }

        [Required(ErrorMessage = "DateOut is required.")]
        public string DateOut { get; set; }
        public string DateIn { get; set; }


        public static Rental SemicolonSeparte(string semicolonLine)
        {
            string[] values = semicolonLine.Split(";".ToCharArray());
            Rental objRental = new Rental();
            objRental.RentalId = Convert.ToInt32(values[0].Trim());
            objRental.CustId = Convert.ToInt32(values[1].Trim());

            if (!System.IO.File.Exists(HttpContext.Current.Server.MapPath("~/DataFiles/Customers.txt")))
            {
                using (System.IO.FileStream fs = System.IO.File.Create(HttpContext.Current.Server.MapPath("~/DataFiles/Customers.txt")))
                {
                }
            }
            objRental.Customer = System.IO.File.ReadAllLines(HttpContext.Current.Server.MapPath("~/DataFiles/Customers.txt")).Select(x => Customer.SemicolonSeparte(x)).ToList().FirstOrDefault(x => x.CustId == objRental.CustId);

            objRental.ToolId = Convert.ToInt32(values[2].Trim());
            if (!System.IO.File.Exists(HttpContext.Current.Server.MapPath("~/DataFiles/Tools.txt")))
            {
                using (System.IO.FileStream fs = System.IO.File.Create(HttpContext.Current.Server.MapPath("~/DataFiles/Tools.txt")))
                {
                }
            }
            objRental.Tool = System.IO.File.ReadAllLines(HttpContext.Current.Server.MapPath("~/DataFiles/Tools.txt")).Select(x => Tool.SemicolonSeparte(x)).ToList().FirstOrDefault(x => x.ToolId == objRental.ToolId);

            objRental.DateOut = values[3].Trim().ToString();
            objRental.DateIn = values[4].Trim().ToString();
            return objRental;
        }
    }
}