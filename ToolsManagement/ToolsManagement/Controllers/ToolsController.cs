using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using ToolsManagement.Models;
namespace ToolsManagement.Controllers
{
    public class ToolsController : Controller
    {
        [HttpGet]
        public ActionResult Index()
        {
            List<Tool> toolList = GetToolList().OrderByDescending(x => x.Rented).ThenBy(x => x.ToolId).ToList();
            return View(toolList);
        }

        [HttpGet]
        public ActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Create(Tool objTool)
        {
            if (ModelState.IsValid)
            {
                List<Tool> toolList = GetToolList();

                if (!toolList.Any(x => x.Name == objTool.Name))
                {
                    if (toolList.Count > 0)
                        objTool.ToolId = toolList.LastOrDefault().ToolId + 1;
                    else
                        objTool.ToolId = 1;

                    objTool.Rented = 0;
                    toolList.Add(objTool);
                    WriteToolListInFile(toolList);
                    ShowNotification("Success", "Tool added successfully.", "success");
                    return RedirectToAction("Index");
                }
                else
                {
                    ShowNotification("Error", "Tool Name is already exist.", "warning");
                    return View();
                }
            }
            return View();
        }

        [HttpGet]
        public ActionResult Edit(int? toolId)
        {
            if (toolId == null)
            {
                ShowNotification("Bad Request", "Failed to complete opration.", "warning");
                return RedirectToAction("Index");
                //return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            List<Tool> toolList = GetToolList();
            Tool objTool = toolList.FirstOrDefault(x => x.ToolId == toolId);
            if (objTool == null)
            {
                ShowNotification("Not Found", "Tool does not exist.", "warning");
                return RedirectToAction("Index");
                //return HttpNotFound();
            }
            return View(objTool);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(Tool objTool)
        {
            if (ModelState.IsValid)
            {
                List<Tool> toolList = GetToolList();

                if (!toolList.Any(x => x.ToolId != objTool.ToolId && x.Name == objTool.Name))
                {
                    foreach (Tool itemTool in toolList)
                    {
                        if (objTool.ToolId == itemTool.ToolId)
                        {
                            itemTool.Name = objTool.Name;
                        }
                    }
                }
                else
                {
                    ShowNotification("Error", "Tool Name is already exist.", "warning");
                    return View(objTool);
                }
                WriteToolListInFile(toolList);
                ShowNotification("Success", "Tool updated successfully.", "success");
                return RedirectToAction("Index");
            }
            return View(objTool);
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