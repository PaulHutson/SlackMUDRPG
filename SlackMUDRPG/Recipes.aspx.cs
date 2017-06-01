using SlackMUDRPG.CommandClasses;
using SlackMUDRPG.Utility;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace SlackMUDRPG
{
	public partial class Receipes : System.Web.UI.Page
	{
		protected void Page_Load(object sender, EventArgs e)
		{
			recipesTypesList.Text = this.GetRecipesTypeListHtml();

			recipeListings.Text = this.GetRecipeListingsHtml();
		}

		/// <summary>
		/// Gets the HTML for a list of recipe type navigation links
		/// </summary>
		/// <returns>String of HTML li elements</returns>
		private string GetRecipesTypeListHtml()
		{
			string html = String.Empty;

			foreach (string type in this.GetRecipeTypes())
			{
				html += $"<li><a href=\"#\" data-group=\"recipes-{type.ToLower()}\">{Utils.SplitCamelCase(type)}</a></li>";
			}

			return html;
		}

		/// <summary>
		/// Gets the HTML for the recipe listings for all types
		/// </summary>
		/// <returns>String of HTML recipe entrires</returns>
		private string GetRecipeListingsHtml()
		{
			string html = String.Empty;

			List<string> types = this.GetRecipeTypes();

			foreach (string type in types)
			{
				html += $"<div class=\"info-group\" id=\"recipes-{type.ToLower()}\">";
				html += this.GetRecipeTypeListingHtml(type);
				html += $"</div>";
			}

			return html;
		}

		/// <summary>
		/// Gets the HTML of recipe listings for a given recipe type
		/// </summary>
		/// <param name="family">The recipe type to get the listing for</param>
		/// <returns>String of HTML recipe entrires</returns>
		private string GetRecipeTypeListingHtml(string type)
		{
			string html = String.Empty;

			List<SMReceipe> recipes = this.GetRecipiesForType(type);

			if (recipes != null)
			{
				foreach (SMReceipe recipe in recipes)
				{
					string guid = Guid.NewGuid().ToString();
					string template = Utils.GetHtmlTemplate("RecipeTemplate");

					template = template.Replace("{panelId}", guid);
					template = template.Replace("{Name}", recipe.Name);
					template = template.Replace("{Description}", recipe.Description);
					template = template.Replace("{RecipeOutput}", recipe.GetProducedOutputString());
					template = template.Replace("{RequiredSkills}", Utils.noneIfNull(recipe.GetRequiredSkillsString()));
					template = template.Replace("{RequiredMaterials}", recipe.GetRequiredMaterialsString());
					template = template.Replace("{NeedToLearn}", this.GetNeedToLearnString(recipe));

					html += template;
				}
			}

			return html;
		}

		/// <summary>
		/// Gets a list of recipe types based on the produces field of those loaded in the current application
		/// </summary>
		/// <returns>A string list of recipe types</returns>
		private List<string> GetRecipeTypes()
		{
			List<SMReceipe> smrl = (List<SMReceipe>)Application["SMReceipes"];

			List<string> types = smrl.Select(recipe => recipe.Produces.Split('.')[0]).Distinct().ToList();

			return types.OrderBy(type => type).ToList();
		}

		/// <summary>
		/// Gets a list of recipes of a given type based on the produces field of those loadin in the current application
		/// </summary>
		/// <param name="recipeType">Recipe type to filter on</param>
		/// <returns>A filtered list of SMRecipe objects</returns>
		private List<SMReceipe> GetRecipiesForType(string recipeType)
		{
			List<SMReceipe> smrl = (List<SMReceipe>)Application["SMReceipes"];

			List<SMReceipe> filtered = smrl.Where(recipe => recipe.Produces.Split('.')[0] == recipeType).ToList();

			return filtered.OrderBy(recipe => recipe.Name).ToList();
		}

		/// <summary>
		/// Gets an extra HTML string to add to a recipies entry if your are required to learn it first
		/// </summary>
		/// <param name="recipe">The recipe to check if you need to learn</param>
		/// <returns>HTML string explaining the recipes need to learn</returns>
		private string GetNeedToLearnString(SMReceipe recipe)
		{
			string ret = String.Empty;

			if (recipe.NeedToLearn == true)
			{
				ret += "<br/>";
				ret += "<br/>";
				ret += "<span class=\"note\"><strong>Note:</strong> you must learn this recipe before you can make it.</span>";
			}

			return ret;
		}
	}
}