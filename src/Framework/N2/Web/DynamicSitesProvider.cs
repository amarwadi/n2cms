using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using N2.Collections;
using N2.Configuration;
using N2.Engine;
using N2.Definitions;
using N2.Persistence;
using N2.Persistence.Finder;

namespace N2.Web
{
	/// <summary>
	/// Finds available sites by looking for content items implementing the 
	/// <see cref="ISitesSource"/> interface.
	/// </summary>
	[Service(typeof(ISitesProvider))]
	public class DynamicSitesProvider : N2.Web.ISitesProvider
	{
		#region Private Fields
		readonly IPersister persister;
		readonly DescendantItemFinder finder;
		readonly IHost host;
		#endregion

		#region Constructors
		public DynamicSitesProvider(DescendantItemFinder finder, IPersister persister, IHost host)
		{
			this.persister = persister;
			this.finder = finder;
			this.host = host;
		}
		#endregion

        public virtual IEnumerable<Site> GetSites()
		{
            List<Site> foundSites = new List<Site>();

            try
            {
				foreach (ISitesSource source in finder.Find<ISitesSource>(persister.Get(host.DefaultSite.RootItemID)))
				{
					foreach (Site s in source.GetSites())
					{
						foundSites.Add(s);
					}
				}
            }
            catch (Exception ex)
            {
                Trace.TraceWarning("DynamicSitesProvider.GetSites:" + ex);
            }

		    return foundSites;
		}
	}
}
