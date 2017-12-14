using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace ManageTools.Models
{
    public class Tool
    {
        public int ToolId { get; set; }
        [Required(ErrorMessage = "Tool Name is required.")]
        [DisplayName("Tool Name")]
        public String ToolName { get; set; }

        [DisplayName("Is Rented")]
        public int IsRented { get; set; }

        public static Tool SemicolonSeparte(string semicolonLine)
        {
            string[] values = semicolonLine.Split(";".ToCharArray());
            Tool objTool = new Tool();
            objTool.ToolId = Convert.ToInt32(values[0].Trim());
            objTool.ToolName = values[1].Trim().ToString();
            objTool.IsRented = Convert.ToInt16(values[2].Trim());
            return objTool;
        }
    }
}