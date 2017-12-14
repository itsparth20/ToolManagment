using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using ToolsManagement.Models;
namespace ToolsManagement.Controllers
{
    public class CustomersController : Controller
    {
        [HttpGet]
        public ActionResult Index()
        {
            List<Customer> customerList = GetCustomerList().Where(x => x.Deleted == 0).ToList();
            return View(customerList);
        }

        [HttpGet]
        public ActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Create(Customer customer)
        {
            if (ModelState.IsValid)
            {
                List<Customer> customerList = GetCustomerList();
                if (!customerList.Any(x => x.Name == customer.Name && x.Deleted == 0))
                {
                    if (customerList.Count > 0)
                        customer.CustomerId = customerList.LastOrDefault().CustomerId + 1;
                    else
                        customer.CustomerId = 1;

                    customer.Deleted = 0;
                    customerList.Add(customer);
                    WriteCustomerListInFile(customerList);
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

            List<Customer> customerList = GetCustomerList();
            Customer objCustomer = customerList.FirstOrDefault(x => x.Deleted == 0 && x.CustomerId == customerId);
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
        public ActionResult Edit(Customer objcustomer)
        {
            if (ModelState.IsValid)
            {
                List<Customer> customerList = GetCustomerList();

                if (!customerList.Any(x => x.CustomerId != objcustomer.CustomerId && x.Name == objcustomer.Name && x.Deleted == 0))
                {

                    foreach (Customer itemCustomer in customerList)
                    {
                        if (itemCustomer.Deleted == 0 && itemCustomer.CustomerId == objcustomer.CustomerId)
                        {
                            itemCustomer.Name = objcustomer.Name;
                        }
                    }
                }
                else
                {
                    ShowNotification("Error", "Customer Name is already exist.", "warning");
                    return View(objcustomer);
                }
                WriteCustomerListInFile(customerList);
                ShowNotification("Success", "Customer updated successfully.", "success");
                return RedirectToAction("Index");
            }
            return View(objcustomer);
        }

        public ActionResult Delete(int? customerId)
        {
            if (customerId == null)
            {
                ShowNotification("Bad Request", "Failed to complete opration.", "warning");
                return RedirectToAction("Index");
                //return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            List<Customer> customerList = GetCustomerList();

            Customer objCustomer = customerList.FirstOrDefault(x => x.CustomerId == customerId);
            if (objCustomer == null)
            {
                ShowNotification("Not Found", "Customer does not exist.", "warning");
                return RedirectToAction("Index");
                //return HttpNotFound();
            }

            foreach (Customer itemCustomer in customerList)
            {
                if (itemCustomer.CustomerId == objCustomer.CustomerId)
                {
                    itemCustomer.Deleted = 1;
                }
            }

            WriteCustomerListInFile(customerList);
            ShowNotification("Success", "Customer deleted successfully.", "success");
            return RedirectToAction("Index");

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

        public void WriteCustomerListInFile(List<Customer> customerList)
        {
            List<string> strCustomerList = new List<string>();
            foreach (Customer cust in customerList)
                strCustomerList.Add(cust.CustomerId + "; " + cust.Name + "; " + cust.Deleted + ";");

            
            System.IO.File.WriteAllLines(Server.MapPath("~/DBFiles/Customers.txt"), strCustomerList.ToArray());
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