using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using SlackMUDRPG.Utility;
using System.IO;
using SlackMUDRPG.CommandClasses;
using Newtonsoft.Json;

namespace SlackMUDRPG
{
    public partial class RebuildCharNameList : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            new SMAccountHelper().RebuildCharacterJSONFile();
        }
    }
}