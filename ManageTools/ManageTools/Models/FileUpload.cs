using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace ManageTools.Models
{
    public class FileUpload
    {
        [Required(ErrorMessage = "Customer File required.")]
        public HttpPostedFileWrapper CustomerFile { get; set; }

        [Required(ErrorMessage = "Tool File required.")]
        public HttpPostedFileWrapper ToolFile { get; set; }

        [Required(ErrorMessage = "Rental File required.")]
        public HttpPostedFileWrapper RentalFile { get; set; }
    }
}