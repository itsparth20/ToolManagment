using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using ManageTools.Models;
namespace ManageTools.Controllers
{
    public class CustomersController : Controller
    {
        [HttpGet]
        public ActionResult Index()
        {
            List<Customer> customers = GetCustomers().Where(x => x.IsDeleted == 0).ToList();
            return View(customers);
        }

        [HttpGet]
        public ActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Create(Customer objCustomer)
        {
            if (ModelState.IsValid)
            {
                List<Customer> customers = GetCustomers();
                if (!customers.Any(x => x.CustomerName == objCustomer.CustomerName && x.IsDeleted == 0))
                {
                    if (customers.Count > 0)
                        objCustomer.CustId = customers.LastOrDefault().CustId + 1;
                    else
                        objCustomer.CustId = 1;

                    objCustomer.IsDeleted = 0;
                    customers.Add(objCustomer);
                    WriteCustomersInFile(customers);
                    ShowNotification("Success", "Customer added successfully.", "success");
                    return RedirectToAction("Index");
                }
                else
                {
                    ShowNotification("Error", "Customer Name is already exist.", "warning");
                    return View();
                }
            }
            else
                return View();
        }

        [HttpGet]
        public ActionResult Edit(int? customerId)
        {
            if (customerId == null)
            {
                ShowNotification("Bad Request", "Failed to complete opration.", "warning");
                return RedirectToAction("Index");
                //return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            List<Customer> customers = GetCustomers();
            Customer objCustomer = customers.FirstOrDefault(x => x.IsDeleted == 0 && x.CustId == customerId);
            if (objCustomer == null)
            {
                ShowNotification("Not Found", "Customer does not exist.", "warning");
                return RedirectToAction("Index");
                //return HttpNotFound();
            }
            return View(objCustomer);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(Customer customer)
        {
            if (ModelState.IsValid)
            {
                List<Customer> customers = GetCustomers();

                if (!customers.Any(x => x.CustId != customer.CustId && x.CustomerName == customer.CustomerName && x.IsDeleted == 0))
                {

                    foreach (Customer itemCustomer in customers)
                    {
                        if (itemCustomer.IsDeleted == 0 && itemCustomer.CustId == customer.CustId)
                        {
                            itemCustomer.CustomerName = customer.CustomerName;
                        }
                    }
                }
                else
                {
                    ShowNotification("Error", "Customer Name is already exist.", "warning");
                    return View(customer);
                }
                WriteCustomersInFile(customers);
                ShowNotification("Success", "Customer updated successfully.", "success");
                return RedirectToAction("Index");
            }
            return View(customer);
        }

        public ActionResult Delete(int? customerId)
        {
            if (customerId == null)
            {
                ShowNotification("Bad Request", "Failed to complete opration.", "warning");
                return RedirectToAction("Index");
                //return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            List<Customer> customers = GetCustomers();

            Customer objCustomer = customers.FirstOrDefault(x => x.CustId == customerId);
            if (objCustomer == null)
            {
                ShowNotification("Not Found", "Customer does not exist.", "warning");
                return RedirectToAction("Index");
                //return HttpNotFound();
            }

            foreach (Customer itemCustomer in customers)
            {
                if (itemCustomer.CustId == objCustomer.CustId)
                {
                    itemCustomer.IsDeleted = 1;
                }
            }

            WriteCustomersInFile(customers);
            ShowNotification("Success", "Customer deleted successfully.", "success");
            return RedirectToAction("Index");

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

        public void WriteCustomersInFile(List<Customer> customerList)
        {
            List<string> strCustomerList = new List<string>();
            foreach (Customer cust in customerList)
                strCustomerList.Add(cust.CustId + "; " + cust.CustomerName + "; " + cust.IsDeleted + ";");

            
            System.IO.File.WriteAllLines(Server.MapPath("~/DataFiles/Customers.txt"), strCustomerList.ToArray());
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