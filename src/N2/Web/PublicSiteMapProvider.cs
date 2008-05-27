using System;
using System.Web;
using System.Collections.Generic;
using System.Text;
using N2.Collections;

namespace N2.Web
{
    /// <summary>
	/// A site map provider implementation for N2 CMS content pages.
	/// </summary>
    public class PublicSiteMapProvider : SiteMapProvider
    {
		public override void Initialize(string name, System.Collections.Specialized.NameValueCollection attributes)
		{
			if (attributes["securityTrimmingEnabled"] == null)
				attributes["securityTrimmingEnabled"] = "true";

			base.Initialize(name, attributes);
		}

        protected virtual SiteMapNode Convert(ContentItem item)
        {
            if (item != null)
                return new PublicSiteMapNode(this, item);
            return null;
        }

        public override SiteMapNode FindSiteMapNode(string rawUrl)
        {
            if (string.IsNullOrEmpty(rawUrl)) throw new ArgumentNullException("rawUrl");

            // If the first letter of the url is a number then the rawUrl probably is
            // the key of a previously generated SiteMapNode. This is an odd behaviour 
            // of the site map provider model
            ContentItem item = (rawUrl[0]>'0' && rawUrl[0]<='9') ?
                Context.Persister.Get(int.Parse(rawUrl)) :
                Context.UrlParser.Parse(rawUrl);

            return Convert(item);
        }

        public override SiteMapNodeCollection GetChildNodes(SiteMapNode node)
        {
            SiteMapNodeCollection nodes = new SiteMapNodeCollection();
			ContentItem item = (node != null) ? Context.Persister.Get(int.Parse(node.Key)) : null; 
			
            // Add published nodes that are pages
			if (item != null)
			{
				IEnumerable<ItemFilter> filters = GetFilters();

				foreach (ContentItem child in item.GetChildren(filters))
					nodes.Add(Convert(child));
			}

            return nodes;
        }

		protected virtual IEnumerable<ItemFilter> GetFilters()
		{
			IList<ItemFilter> filters = new List<ItemFilter>();
			filters.Add(new PageFilter());
			filters.Add(new VisibleFilter());
			filters.Add(new PublishedFilter());
			if (SecurityTrimmingEnabled)
				filters.Add(new AccessFilter());
			return filters;
		}

        public override SiteMapNode GetParentNode(SiteMapNode node)
        {
            ContentItem item = Context.Persister.Get(int.Parse(node.Key));
            if(item != null && item.Parent != null && !Context.UrlParser.IsRootOrStartPage(item))
                return Convert((ContentItem)item.Parent);
            else
                return null;
        }

        protected override SiteMapNode GetRootNodeCore()
        {
			return Convert(Context.UrlParser.StartPage);
        }
    }
}