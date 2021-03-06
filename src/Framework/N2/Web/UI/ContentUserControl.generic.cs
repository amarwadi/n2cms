using System;
using System.Collections.Generic;
using System.Text;
using System.Web.UI;

namespace N2.Web.UI
{
	/// <summary>
	/// A user control base used to for quick access to content data.
	/// </summary>
	/// <typeparam name="TPage">The type of page item this user control will have to deal with.</typeparam>
    public abstract class ContentUserControl<TPage> : System.Web.UI.UserControl, IItemContainer
		where TPage : N2.ContentItem
	{
		private TPage currentPage = null;

		/// <summary>Gets the current CMS Engine.</summary>
		public N2.Engine.IEngine Engine
		{
			get { return N2.Context.Current; }
		}

		/// <summary>Gets the current page item.</summary>
		public virtual TPage CurrentPage
		{
			get
			{
				if (currentPage == null)
				{
					IItemContainer page = Page as IItemContainer;
					ContentItem item = (page != null) ? page.CurrentItem : N2.Context.CurrentPage;
					currentPage = ItemUtility.EnsureType<TPage>(item);
				}
				return currentPage;
			}
		}

		/// <summary>This is most likely the same as CurrentPage unles you're in a user control dynamically added as a part.</summary>
		public virtual TPage CurrentItem
		{
			get { return CurrentPage; }
		}

		ContentItem IItemContainer.CurrentItem
		{
			get { return CurrentPage; }
		}
	}
}
