﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using N2.Engine;
using N2.Persistence;
using N2.Configuration;
using N2.Definitions;
using N2.Plugin;
using System.Diagnostics;

namespace N2.Web
{
	[Service]
	public class MultipleSitesInitializer : IAutoStart
	{
		public MultipleSitesInitializer(IPersister persister, IHost host, ISitesProvider sitesProvider, HostSection config, IDefinitionManager ignored)
		{
			Debug.WriteLine("MultipleSitesInitializer");

			if (config.MultipleSites && config.DynamicSites)
			{
				host.AddSites(sitesProvider.GetSites());
				persister.ItemSaved += delegate(object sender, ItemEventArgs e)
				{
					if (e.AffectedItem is ISitesSource)
					{
						IList<Site> sites = Host.ExtractSites(config);
						sites = Host.Union(sites, sitesProvider.GetSites());

						host.ReplaceSites(host.DefaultSite, sites);
					}
				};
			}
		}

		#region IAutoStart Members

		public void Start()
		{
		}

		public void Stop()
		{
		}

		#endregion
	}
}
