using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ManageTools.Models;
namespace ManageTools.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Index(HttpPostedFileWrapper CustomerFile, HttpPostedFileWrapper ToolFile, HttpPostedFileWrapper RentalFile)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    if (CustomerFile != null && CustomerFile.ContentLength > 0 && ToolFile != null && ToolFile.ContentLength > 0 && RentalFile != null && RentalFile.ContentLength > 0)
                    {
                        CustomerFile.SaveAs(Server.MapPath("~/DataFiles/Customers.txt"));
                        ToolFile.SaveAs(Server.MapPath("~/DataFiles/Tools.txt"));
                        RentalFile.SaveAs(Server.MapPath("~/DataFiles/Rental_data.txt"));
                        ShowNotification("Success", "DataFiles Saved successfully. Use Other Modules with changed data.", "success");
                        return View();
                    }
                    else
                    {
                        ShowNotification("Error", "Doesn't upload any file OR Upload Empty File", "warning");
                        return View();
                    }
                }
                return View();
            }
            catch
            {
                ShowNotification("Error", "Error while saving Files.", "warning");
                return View();
            }           
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