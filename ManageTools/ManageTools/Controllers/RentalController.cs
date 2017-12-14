using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using ManageTools.Models;

namespace ManageTools.Controllers
{
    public class RentalController : Controller
    {
        [HttpGet]
        public ActionResult Index()
        {
            ViewBag.ToolList = new SelectList(GetTools().ToList(), "ToolId", "ToolName");

            List<Rental> rentals = GetRentals().OrderByDescending(x => x.Tool.IsRented).ThenByDescending(x => DateTime.ParseExact(x.DateOut, "MM-dd-yyyy", null)).ToList(); ;
            return View(rentals);
        }

        [HttpPost]
        public ActionResult Index(string SearchString, int ToolId = 0)
        {
            List<Rental> rentals = GetRentals();

            if (!string.IsNullOrEmpty(SearchString))
                rentals = rentals.Where(x => x.Customer.CustomerName.ToLower().Contains(SearchString)).ToList();

            if (ToolId > 0)
                rentals = rentals.Where(x => x.ToolId == ToolId).ToList();

            rentals = rentals.OrderByDescending(x => x.Tool.IsRented).ThenByDescending(x => DateTime.ParseExact(x.DateOut, "MM-dd-yyyy", null)).ToList();
            ViewBag.ToolList = new SelectList(GetTools().ToList(), "ToolId", "ToolName");
            return View(rentals);
        }

        [HttpGet]
        public ActionResult Create()
        {
            ViewBag.CustomerList = new SelectList(GetCustomers().Where(x => x.IsDeleted == 0).ToList(), "CustId", "CustomerName");
            ViewBag.ToolList = new SelectList(GetTools().Where(x => x.IsRented == 0).ToList(), "ToolId", "ToolName");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Rental objRental)
        {
            if (ModelState.IsValid)
            {
                List<Rental> rentals = GetRentals().OrderBy(x => x.RentalId).ToList();
                if (rentals.Where(x => x.CustId == objRental.CustId && string.IsNullOrEmpty(x.DateIn)).ToList().Count < 5)
                {
                    List<int> rentalIds = rentals.Select(x => x.RentalId).ToList();
                    if (rentalIds.Count > 0)
                        objRental.RentalId = rentalIds.Max() + 1;
                    else
                        objRental.RentalId = 1;

                    List<Tool> toolList = GetTools();
                    foreach (Tool objTool in toolList)
                    {
                        if (objTool.ToolId == objRental.ToolId && objTool.IsRented == 0)
                            objTool.IsRented = 1;
                    }

                    rentals.Add(objRental);
                    WriteRentalsInFile(rentals);
                    WriteToolsInFile(toolList);
                    ShowNotification("Success", "Rental added successfully.", "success");
                    return RedirectToAction("Index");
                }
                else
                {
                    ShowNotification("Info", "At a time customer only rented 5 tools.", "info");
                    ViewBag.CustomerList = new SelectList(GetCustomers().Where(x => x.IsDeleted == 0).ToList(), "CustId", "CustomerName");
                    ViewBag.ToolList = new SelectList(GetTools().Where(x => x.IsRented == 0).ToList(), "ToolId", "ToolName");
                    return View();
                }
            }
            ViewBag.CustomerList = new SelectList(GetCustomers().Where(x => x.IsDeleted == 0).ToList(), "CustId", "CustomerName");
            ViewBag.ToolList = new SelectList(GetTools().Where(x => x.IsRented == 0).ToList(), "ToolId", "ToolName");
            return View();
        }

        [HttpGet]
        public ActionResult Edit(int? rentalId)
        {
            if (rentalId == null)
            {
                ShowNotification("Bad Request", "Failed to complete opration.", "warning");
                return RedirectToAction("Index");
                //return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            List<Rental> rentals = GetRentals();
            Rental objRental = rentals.FirstOrDefault(x => x.RentalId == rentalId);
            if (objRental == null)
            {
                ShowNotification("Not Found", "Rental does not exist.", "warning");
                return RedirectToAction("Index");
                //return HttpNotFound();
            }

            ViewBag.CustomerList = new SelectList(GetCustomers().Where(x => x.IsDeleted == 0 || x.CustId == objRental.CustId).ToList(), "CustId", "CustomerName");
            ViewBag.ToolList = new SelectList(GetTools().Where(x => x.IsRented == 0 || x.ToolId == objRental.ToolId).ToList(), "ToolId", "ToolName");

            return View(objRental);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(Rental objRental)
        {
            if (ModelState.IsValid)
            {
                List<Rental> rentals = GetRentals();
                bool occure = false;
                foreach (Rental itemrental in rentals)
                {
                    if ((objRental.RentalId == itemrental.RentalId && !string.IsNullOrEmpty(objRental.DateIn)) || (objRental.RentalId == itemrental.RentalId && rentals.Where(x => x.CustId == objRental.CustId && string.IsNullOrEmpty(x.DateIn)).ToList().Count < 5))
                    {
                        itemrental.CustId = objRental.CustId;
                        itemrental.ToolId = objRental.ToolId;
                        itemrental.DateOut = objRental.DateOut;
                        if (!string.IsNullOrEmpty(objRental.DateIn))
                        {
                            itemrental.DateIn = objRental.DateIn;
                            List<Tool> toolList = GetTools();
                            foreach (Tool objTool in toolList)
                            {
                                if (objTool.ToolId == objRental.ToolId && objTool.IsRented == 1)
                                    objTool.IsRented = 0;
                            }

                            occure = true;
                            WriteToolsInFile(toolList);

                        }
                    }
                }
                if (!occure)
                {
                    ShowNotification("Info", "At a time customer only rented 5 tools.", "info");
                    ViewBag.CustomerList = new SelectList(GetCustomers().Where(x => x.IsDeleted == 0).ToList(), "CustId", "CustomerName");
                    ViewBag.ToolList = new SelectList(GetTools().Where(x => x.IsRented == 0).ToList(), "ToolId", "ToolName");
                    return View(objRental);
                }
                WriteRentalsInFile(rentals);
                ShowNotification("Success", "Rental Updated successfully.", "success");
                return RedirectToAction("Index");
            }
            return View(objRental);
        }

        public List<Customer> GetCustomers()
        {
            if (!System.IO.File.Exists(Server.MapPath("~/DataFiles/Customers.txt")))
            {
                using (System.IO.FileStream fs = System.IO.File.Create(Server.MapPath("~/DataFiles/Customers.txt")))
                {
                }
            }
            return System.IO.File.ReadAllLines(Server.MapPath("~/DataFiles/Customers.txt")).Select(x => Customer.SemicolonSeparte(x)).ToList();
        }


        public List<Tool> GetTools()
        {
            if (!System.IO.File.Exists(Server.MapPath("~/DataFiles/Tools.txt")))
            {
                using (System.IO.FileStream fs = System.IO.File.Create(Server.MapPath("~/DataFiles/Tools.txt")))
                {
                }
            }
            return System.IO.File.ReadAllLines(Server.MapPath("~/DataFiles/Tools.txt")).Select(x => Tool.SemicolonSeparte(x)).ToList();
        }

        public List<Rental> GetRentals()
        {
            if (!System.IO.File.Exists(Server.MapPath("~/DataFiles/Rental_data.txt")))
            {
                using (System.IO.FileStream fs = System.IO.File.Create(Server.MapPath("~/DataFiles/Rental_data.txt")))
                {
                }
            }
            return System.IO.File.ReadAllLines(Server.MapPath("~/DataFiles/Rental_data.txt")).Select(x => Rental.SemicolonSeparte(x)).ToList();
        }

        public void WriteRentalsInFile(List<Rental> RentalList)
        {
            List<string> strRentalList = new List<string>();
            foreach (Rental rental in RentalList)
                strRentalList.Add(rental.RentalId + "; " + rental.CustId + "; " + rental.ToolId + "; " + rental.DateOut + "; " + rental.DateIn + ";");

            System.IO.File.WriteAllLines(Server.MapPath("~/DataFiles/Rental_data.txt"), strRentalList.ToArray());
        }

        public void WriteToolsInFile(List<Tool> ToolList)
        {
            List<string> strToolList = new List<string>();
            foreach (Tool tool in ToolList)
                strToolList.Add(tool.ToolId + "; " + tool.ToolName + "; " + tool.IsRented + ";");

            System.IO.File.WriteAllLines(Server.MapPath("~/DataFiles/Tools.txt"), strToolList.ToArray());
        }

        /// <summary>
        /// To show notification
        /// </summary>
        /// <param name="title"></param>
        /// <param name="text"></param>
        /// <param name="type">success,warning,info</param>
        /// <returns></returns>
        private void ShowNotification(string title, string text, string type)
        {
            TempData["ShowNotification"] = "True";
            TempData["TitleNotification"] = title;
            TempData["TextNotification"] = text;
            TempData["TypeNotification"] = type;

            TempData.Keep("ShowNotification");
            TempData.Keep("TitleNotification");
            TempData.Keep("TextNotification");
            TempData.Keep("IconNotification");
            TempData.Keep("TypeNotification");
        }

    }
}