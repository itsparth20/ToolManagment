using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace ToolsManagement.Models
{
    public class Tool
    {
        public int ToolId { get; set; }
        [Required(ErrorMessage = "Tool Name is required.")]
        public String Name { get; set; }
        public int Rented { get; set; }

        public static Tool FromSemicolonSeparte(string semicolonLine)
        {
            string[] values = semicolonLine.Split(";".ToCharArray());
            Tool objTool = new Tool();
            objTool.ToolId = Convert.ToInt32(values[0].Trim());
            objTool.Name = values[1].Trim().ToString();
            objTool.Rented = Convert.ToInt16(values[2].Trim());
            return objTool;
        }
    }
}