using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HTML_Editor.Models
{
    public class HtmlTxtModel
    {
        public string OpenHTMLFIle { get; set; } = null;
        public string TxtEditorText { get; set; }
        public bool ShowHideVal { get; set; }
        public bool ShowURLTextBox { get; set; }
        public bool ShowMainBodyField { get; set; } = true;
        public bool ShowErrorBox { get; set; }
        public bool ShowEditOption { get; set; } = false;
        public string ShowOriginalFileID { get; set; } = "noneVALX";
        public string ShowNextFileID { get; set; } = "noneVALX";
    }
}