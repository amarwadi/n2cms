using N2.Details;
using N2.Integrity;
using N2.Web.Mvc;
using N2.Web.UI.WebControls;
using N2.Web.UI;
using N2.Definitions;
using N2.Templates.Mvc.Models.Pages;

namespace N2.Templates.Mvc.Models.Pages
{
	/// <summary>
	/// Redirects to somewhere else. Used as a placeholder in the menu.
	/// </summary>
	[PageDefinition("Redirect",
		Description = "Redirects to another page or an external address.",
		SortOrder = 40,
		IconUrl = "~/Content/Img/page_go.png")]
	[WithEditableTitle("Title", 10, Focus = true, ContainerName = Tabs.Content),
	 WithEditableName("Name", 20, ContainerName = Tabs.Content),
	 WithEditablePublishedRange("Published Between", 30, ContainerName = Tabs.Advanced, BetweenText = " and ")]
	[TabContainer(Tabs.Advanced, "Advanced", 100)]
	[RestrictParents(typeof (IStructuralPage))]
	public class Redirect : PageBase, IStructuralPage
	{
		public override string Url
		{
			get { return N2.Web.Url.ToAbsolute(RedirectUrl); }
		}

		[EditableUrl("Redirect to", 40, ContainerName = Tabs.Content, Required = true)]
		public virtual string RedirectUrl
		{
			get { return (string) base.GetDetail("RedirectUrl"); }
			set { base.SetDetail("RedirectUrl", value); }
		}

		[EditableCheckBox("301 (permanent) redirect", 100, ContainerName = Tabs.Advanced)]
		public virtual bool Redirect301
		{
			get { return (bool)(GetDetail("Redirect301") ?? false); }
			set { SetDetail("Redirect301", value, false); }
		}

		[EditableCheckBox("Visible", 40, ContainerName = Tabs.Advanced)]
		public override bool Visible
		{
			get { return base.Visible; }
			set { base.Visible = value; }
		}

		#region IBreadcrumbAppearance Members

		public bool VisibleInBreadcrumb
		{
			get { return false; }
		}

		#endregion
	}
}