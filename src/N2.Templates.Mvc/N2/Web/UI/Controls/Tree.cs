using System;
using System.Collections.Generic;
using System.Text;
using System.Web;
using System.Web.UI;
using N2.Collections;
using N2.Web;
using N2.Web.UI;
using N2.Web.UI.WebControls;
using System.Security.Principal;
using N2.Engine;
using N2.Edit.Workflow;

namespace N2.Edit.Web.UI.Controls
{
	public class Tree : Control
	{
		ContentItem selectedtItem = null;
		ContentItem rootItem = null;
		ItemFilter filter = null;
		string target = Targets.Preview;
		IEngine engine;

		public Tree()
		{
		}

		public HierarchyNode<ContentItem> Nodes { get; set; }

		public ContentItem SelectedItem
		{
		    get { return selectedtItem ?? (selectedtItem = Find.CurrentPage ?? Find.StartPage); }
		    set { selectedtItem = value; }
		}

		public ContentItem RootNode
		{
		    get { return rootItem ?? Find.RootItem; }
		    set { rootItem = value; }
		}

		public ItemFilter Filter
		{
			get { return filter ?? N2.Context.Current.EditManager.GetEditorFilter(Page.User); }
			set { filter = value; }
		}

		public string Target
		{
			get { return target; }
			set { target = value; }
		}

		public IEngine Engine
		{
			get { return engine ?? (engine = N2.Context.Current); }
			set { engine = value; }
		}

		public override void DataBind()
		{
			EnsureChildControls();
			base.DataBind();
		}

		protected override void CreateChildControls()
		{
			IContentAdapterProvider adapters = Engine.Resolve<IContentAdapterProvider>();

			if (Nodes == null)
				Nodes = new BranchHierarchyBuilder(SelectedItem, RootNode, true)
					.Children((item) => adapters.ResolveAdapter<NodeAdapter>(item.GetType()).GetChildren(item, Interfaces.Managing))
					.Build();

			var tree = new N2.Web.Tree(Nodes)
				.OpenTo(SelectedItem)
				.Filters(Filter)
				.LinkProvider(BuildLink)
				.ToControl();

			AppendExpanderNodeRecursive(tree, Filter, Target);

			Controls.Add(tree);
			
			base.CreateChildControls();
		}

		public static void AppendExpanderNodeRecursive(Control tree, ItemFilter filter, string target)
		{
			TreeNode tn = tree as TreeNode;
			if (tn != null)
			{
				foreach (Control child in tn.Controls)
				{
					AppendExpanderNodeRecursive(child, filter, target);
				}
				if (tn.Controls.Count == 0 && tn.Node.GetChildren(filter).Count > 0)
				{
					AppendExpanderNode(tn, target);
				}
			}
		}

		public static void AppendExpanderNode(TreeNode tn, string target)
		{
			Li li = new Li();
			
//TODO respect EditInterfaceUrl setting
			li.Text = "{url:" + Url.ToAbsolute("~/N2/Content/Navigation/LoadTree.ashx?target=" + target + "&selected=" + HttpUtility.UrlEncode(tn.Node.Path)) + "}";

			tn.UlClass = "ajax";
			tn.Controls.Add(li);
		}

		private ILinkBuilder BuildLink(ContentItem item)
		{
			return BuildLink(item, item.Path == SelectedItem.Path, Target);
		}

		internal static ILinkBuilder BuildLink(ContentItem item, bool selected, string target)
		{
			INode node = item;
			string className = node.ClassNames;
			if (selected)
				className += "selected ";

			ILinkBuilder builder = Link.To(node)
				.Target(target)
				.Class(className)
				.Text("<img src='" + N2.Web.Url.ToAbsolute(node.IconUrl) + "'/>" + node.Contents)
				.Attribute("rel", node.Path)
				.Attribute("data-id", item.ID.ToString())
				.Attribute("data-type", item.GetType().Name)
				.Attribute("data-path", item.Path)
				.Attribute("data-url", item.Url)
				.Attribute("data-page", item.IsPage.ToString().ToLower())
				.Attribute("data-zone", item.ZoneName);

			if (string.IsNullOrEmpty(node.PreviewUrl))
				builder.Href("~/N2/Empty.aspx");
			else
				builder.Href(node.PreviewUrl);

			return builder;
		}
	}
}
