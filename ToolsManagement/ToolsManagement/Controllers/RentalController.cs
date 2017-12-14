using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using ToolsManagement.Models;

namespace ToolsManagement.Controllers
{
    public class RentalController : Controller
    {
        [HttpGet]
        public ActionResult Index()
        {
            ViewBag.ToolList = new SelectList(GetToolList().ToList(), "ToolId", "Name");

            List<Rental> rentalList = GetRentalList().OrderByDescending(x => x.Tool.Rented).ThenByDescending(x => DateTime.ParseExact(x.DateOut, "MM-dd-yyyy", null)).ToList(); ;
            return View(rentalList);
        }

        [HttpPost]
        public ActionResult Index(string SearchString, int ToolId = 0)
        {
            List<Rental> rentalList = GetRentalList();

            if (!string.IsNullOrEmpty(SearchString))
                rentalList = rentalList.Where(x => x.Customer.Name.ToLower().Contains(SearchString)).ToList();

            if (ToolId > 0)
                rentalList = rentalList.Where(x => x.ToolId == ToolId).ToList();

            rentalList = rentalList.OrderByDescending(x => x.Tool.Rented).ThenByDescending(x => DateTime.ParseExact(x.DateOut, "MM-dd-yyyy", null)).ToList();
            ViewBag.ToolList = new SelectList(GetToolList().ToList(), "ToolId", "Name");
            return View(rentalList);
        }

        [HttpGet]
        public ActionResult Create()
        {
            ViewBag.CustomerList = new SelectList(GetCustomerList().Where(x => x.Deleted == 0).ToList(), "CustomerId", "Name");
            ViewBag.ToolList = new SelectList(GetToolList().Where(x => x.Rented == 0).ToList(), "ToolId", "Name");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Rental objRental)
        {
            if (ModelState.IsValid)
            {
                List<Rental> rentalList = GetRentalList().OrderBy(x => x.RentalId).ToList();

                if (rentalList.Where(x => x.CustomerId == objRental.CustomerId && string.IsNullOrEmpty(x.DateIn)).ToList().Count < 5)
                {
                    List<int> rentalIds = rentalList.Select(x => x.RentalId).ToList(); 
                    if (rentalIds.Count > 0)
                        objRental.RentalId = rentalIds.Max() + 1;
                    else
                        objRental.RentalId = 1;

                    List<Tool> toolList = GetToolList();
                    foreach (Tool objTool in toolList)
                    {
                        if (objTool.ToolId == objRental.ToolId && objTool.Rented == 0)
                            objTool.Rented = 1;
                    }

                    rentalList.Add(objRental);
                    WriteRentalListInFile(rentalList);
                    WriteToolListInFile(toolList);
                    ShowNotification("Success", "Rental added successfully.", "success");
                    return RedirectToAction("Index");
                }
                else
                {
                    ShowNotification("Info", "At a time customer only rented 5 tools.", "info");
                    ViewBag.CustomerList = new SelectList(GetCustomerList().Where(x => x.Deleted == 0).ToList(), "CustomerId", "Name");
                    ViewBag.ToolList = new SelectList(GetToolList().Where(x => x.Rented == 0).ToList(), "ToolId", "Name");
                    return View();
                }
            }
            ViewBag.CustomerList = new SelectList(GetCustomerList().Where(x => x.Deleted == 0).ToList(), "CustomerId", "Name");
            ViewBag.ToolList = new SelectList(GetToolList().Where(x => x.Rented == 0).ToList(), "ToolId", "Name");
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

            List<Rental> rentalList = GetRentalList();
            Rental objRental = rentalList.FirstOrDefault(x => x.RentalId == rentalId);
            if (objRental == null)
            {
                ShowNotification("Not Found", "Rental does not exist.", "warning");
                return RedirectToAction("Index");
                //return HttpNotFound();
            }

            ViewBag.CustomerList = new SelectList(GetCustomerList().Where(x => x.Deleted == 0 || x.CustomerId == objRental.CustomerId).ToList(), "CustomerId", "Name");
            ViewBag.ToolList = new SelectList(GetToolList().Where(x => x.Rented == 0 || x.ToolId == objRental.ToolId).ToList(), "ToolId", "Name");

            return View(objRental);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(Rental objRental)
        {
            if (ModelState.IsValid)
            {
                List<Rental> rentalList = GetRentalList();
                bool occure = false;
                foreach (Rental itemrental in rentalList)
                {
                    if ((objRental.RentalId == itemrental.RentalId && !string.IsNullOrEmpty(objRental.DateIn)) || (objRental.RentalId == itemrental.RentalId && rentalList.Where(x => x.CustomerId == objRental.CustomerId && string.IsNullOrEmpty(x.DateIn)).ToList().Count < 5))
                    {
                        itemrental.CustomerId = objRental.CustomerId;
                        itemrental.ToolId = objRental.ToolId;
                        itemrental.DateOut = objRental.DateOut;
                        if (!string.IsNullOrEmpty(objRental.DateIn))
                        {
                            itemrental.DateIn = objRental.DateIn;
                            List<Tool> toolList = GetToolList();
                            foreach (Tool objTool in toolList)
                            {
                                if (objTool.ToolId == objRental.ToolId && objTool.Rented == 1)
                                    objTool.Rented = 0;
                            }
                            occure = true;
                            WriteToolListInFile(toolList);
                        }
                    }

                }
                if (!occure)
                {
                    ShowNotification("Info", "At a time customer only rented 5 tools.", "info");
                    ViewBag.CustomerList = new SelectList(GetCustomerList().Where(x => x.Deleted == 0 || x.CustomerId == objRental.CustomerId).ToList(), "CustomerId", "Name");
                    ViewBag.ToolList = new SelectList(GetToolList().Where(x => x.Rented == 0 || x.ToolId == objRental.ToolId).ToList(), "ToolId", "Name");
                    return View(objRental);
                }

                WriteRentalListInFile(rentalList);
                ShowNotification("Success", "Rental Updated successfully.", "success");
                return RedirectToAction("Index");
            }
            return View(objRental);
        }

        public List<Customer> GetCustomerList()
        {
            if (!System.IO.File.Exists(Server.MapPath("~/DBFiles/Customers.txt")))
            {
                using (System.IO.FileStream fs = System.IO.File.Create(Server.MapPath("~/DBFiles/Customers.txt")))
                {
                }
            }
            return System.IO.File.ReadAllLines(Server.MapPath("~/DBFiles/Customers.txt")).Select(x => Customer.FromSemicolonSeparte(x)).ToList();
        }


        public List<Tool> GetToolList()
        {
            if (!System.IO.File.Exists(Server.MapPath("~/DBFiles/Tools.txt")))
            {
                using (System.IO.FileStream fs = System.IO.File.Create(Server.MapPath("~/DBFiles/Tools.txt")))
                {
                }
            }
            return System.IO.File.ReadAllLines(Server.MapPath("~/DBFiles/Tools.txt")).Select(x => Tool.FromSemicolonSeparte(x)).ToList();
        }

        public List<Rental> GetRentalList()
        {
            if (!System.IO.File.Exists(Server.MapPath("~/DBFiles/Rental_data.txt")))
            {
                using (System.IO.FileStream fs = System.IO.File.Create(Server.MapPath("~/DBFiles/Rental_data.txt")))
                {
                }
            }
            return System.IO.File.ReadAllLines(Server.MapPath("~/DBFiles/Rental_data.txt")).Select(x => Rental.FromSemicolonSeparte(x)).ToList();
        }

        public void WriteRentalListInFile(List<Rental> RentalList)
        {
            List<string> strRentalList = new List<string>();
            foreach (Rental rental in RentalList)
                strRentalList.Add(rental.RentalId + "; " + rental.CustomerId + "; " + rental.ToolId + "; " + rental.DateOut + "; " + rental.DateIn + ";");

            System.IO.File.WriteAllLines(Server.MapPath("~/DBFiles/Rental_data.txt"), strRentalList.ToArray());
        }

        public void WriteToolListInFile(List<Tool> ToolList)
        {
            List<string> strToolList = new List<string>();
            foreach (Tool tool in ToolList)
                strToolList.Add(tool.ToolId + "; " + tool.Name + "; " + tool.Rented + ";");

            System.IO.File.WriteAllLines(Server.MapPath("~/DBFiles/Tools.txt"), strToolList.ToArray());
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