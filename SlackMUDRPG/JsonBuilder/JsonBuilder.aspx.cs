using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;
using SlackMUDRPG.CommandClasses;

namespace SlackMUDRPG.JsonBuilder
{
	public partial class JsonBuilder : System.Web.UI.Page
	{
		protected void Page_Load(object sender, EventArgs e)
		{
			if (!IsPostBack)
			{
				objectType.Items.Add(new ListItem("Please Select"));
				objectType.Items.Add(new ListItem("SMItem", "SlackMUDRPG.CommandClasses.SMItem"));
				objectType.Items.Add(new ListItem("SMRoom", "SlackMUDRPG.CommandClasses.SMRoom"));
			}
		}

		protected void ObjectTypeChanged(object sender, EventArgs e)
		{
			string typeName = objectType.SelectedValue;

			Type t = Type.GetType(typeName);

			PropertyInfo[] props = t.GetProperties();

			string pathToClassJsonBuilderSpec = Utility.FilePathSystem.GetFilePath("JsonBuilder", t.Name);

			string specJson = "";

			if (File.Exists(pathToClassJsonBuilderSpec))
			{
				using (StreamReader reader = new StreamReader(pathToClassJsonBuilderSpec))
				{
					specJson = reader.ReadToEnd();
				}
			}

			Dictionary<string, List<string>> options = new Dictionary<string, List<string>>();

			if (specJson != "")
			{
				options = JsonConvert.DeserializeObject<Dictionary<string, List<string>>>(specJson);
			}

			foreach (PropertyInfo prop in props)
			{
				string propName = prop.Name;
				Type propType = prop.PropertyType;
				bool propReadOnly = !prop.CanWrite;

				if (options.ContainsKey(propName))
				{
					mainFormPlaceholder.Controls.Add(this.GetDropdownListFormGroup(prop, options[propName]));
				}
				else
				{
					mainFormPlaceholder.Controls.Add(this.GetGenericFormGroup(prop));
				}
			}
		}

		// TODO Read only properties

		//DefaultValueAttribute defaultAttr = property.GetCustomAttribute(typeof(DefaultValueAttribute)) as DefaultValueAttribute;

		private HtmlGenericControl GetFormGroup()
		{
			HtmlGenericControl formGroup = new HtmlGenericControl("div");
			formGroup.Attributes.Add("class", "form-group");

			return formGroup;
		}

		private HtmlGenericControl GetLabel(PropertyInfo property)
		{
			HtmlGenericControl label = new HtmlGenericControl("label");
			label.Attributes.Add("class", "control-label col-xs-3");
			label.Attributes.Add("for", property.Name);
			label.InnerText = $"{property.Name}: ({property.PropertyType.Name}) ";

			return label;
		}

		private HtmlGenericControl GetInputWrapper()
		{
			HtmlGenericControl wrapper = new HtmlGenericControl("div");
			wrapper.Attributes.Add("class", "col-xs-8");

			return wrapper;
		}

		private HtmlGenericControl GetGenericInput(PropertyInfo property)
		{
			HtmlGenericControl inputWrapper = this.GetInputWrapper();

			TextBox input = new TextBox();
			input.ID = property.Name;
			input.Attributes.Add("class", "form-control");
			input.Attributes.Add("name", property.Name);

			// Add HTML compatible types to the input
			switch (property.PropertyType.Name)
			{
				case "Int32":
				case "Single":
					input.TextMode = TextBoxMode.Number;
					break;
			}

			inputWrapper.Controls.Add(input);

			return inputWrapper;
		}

		private HtmlGenericControl GetGenericFormGroup(PropertyInfo property)
		{
			HtmlGenericControl group = this.GetFormGroup();
			group.Controls.Add(this.GetLabel(property));
			group.Controls.Add(this.GetGenericInput(property));

			return group;
		}

		private HtmlGenericControl GetDropdownListFormGroup(PropertyInfo property, List<string> options)
		{
			HtmlGenericControl group = this.GetFormGroup();
			group.Controls.Add(this.GetLabel(property));

			HtmlGenericControl inputWrapper = this.GetInputWrapper();

			DropDownList list = new DropDownList();
			list.ID = property.Name;
			list.Attributes.Add("class", "form-control");
			list.Attributes.Add("name", property.Name);
			list.Items.Add(new ListItem("Please Select"));

			foreach (string option in options)
			{
				list.Items.Add(new ListItem(option));
			}

			inputWrapper.Controls.Add(list);
			group.Controls.Add(inputWrapper);

			return group;
		}
	}
}