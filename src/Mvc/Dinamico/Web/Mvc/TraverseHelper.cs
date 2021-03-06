﻿using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using N2.Collections;
using N2.Definitions;
using N2.Engine.Globalization;
using N2.Persistence.Finder;
using N2.Web.Mvc.Html;
using N2.Persistence.NH;
using System;

namespace N2.Web.Mvc
{
	public class TraverseHelper
	{
		HtmlHelper html;

		public TraverseHelper(HtmlHelper html)
		{
			this.html = html;
		}

		public ContentItem CurrentItem
		{
			get { return html.CurrentItem(); }
		}

		public ContentItem CurrentPage
		{
			get { return html.CurrentPage(); }
		}

		public ILanguage CurrentLanguage
		{
			get { return html.ResolveService<ILanguageGateway>().GetLanguage(CurrentPage); }
		}

		public IEnumerable<ILanguage> Translations()
		{
			var lg = html.ResolveService<ILanguageGateway>();
			return lg.FindTranslations(CurrentPage).Select(i => lg.GetLanguage(i));
		}

		public ContentItem StartPage
		{
			get { return N2.Find.ClosestOf<IStartPage>(CurrentItem) ?? N2.Find.StartPage; }
		}

		public ContentItem RootPage
		{
			get { return N2.Find.ClosestOf<IRootPage>(CurrentItem) ?? N2.Find.RootItem; }
		}

		public virtual ItemFilter DefaultFilter()
		{
			return N2.Filter.Is.Accessible();
		}

		public IEnumerable<ContentItem> Ancestors(ContentItem item = null, ItemFilter filter = null)
		{
			return (filter ?? DefaultFilter()).Pipe(N2.Find.EnumerateParents(item ?? CurrentItem, StartPage, true));
		}

		public IEnumerable<ContentItem> AncestorsBetween(int startLevel = 0, int stopLevel = 5)
		{
			var ancestors = N2.Find.EnumerateParents(CurrentItem, StartPage, true).ToList();
			ancestors.Reverse();
			if (stopLevel < 0)
				stopLevel = ancestors.Count + stopLevel;

			if (startLevel < stopLevel)
				for (int i = startLevel; i < stopLevel && i < ancestors.Count; i++)
					yield return ancestors[i];
			else
				for (int i = Math.Min(stopLevel, ancestors.Count - 1); i >= startLevel; i--)
					yield return ancestors[i];
		}

		public IEnumerable<ContentItem> Children(ItemFilter filter = null)
		{
			return Children(CurrentItem, filter ?? DefaultFilter());
		}

		public IEnumerable<ContentItem> Children(ContentItem item, ItemFilter filter = null)
		{
			if (item == null) return Enumerable.Empty<ContentItem>();
			
			return item.GetChildren(filter ?? DefaultFilter());
		}

		public IEnumerable<ContentItem> Descendants(ContentItem item, ItemFilter filter = null)
		{
			return N2.Find.EnumerateChildren(item).Where((filter ?? DefaultFilter()).Match);
		}

		public IEnumerable<ContentItem> DescendantPages(ContentItem item, ItemFilter filter = null)
		{
			return N2.Find.EnumerateChildren(item).Where(p => p.IsPage).Where((filter ?? DefaultFilter()).Match);
		}

		public IEnumerable<ContentItem> Siblings(ContentItem item = null)
		{
			return Siblings(item, null);
		}

		public IEnumerable<ContentItem> Siblings(ItemFilter filter = null)
		{
			return Siblings(null, filter);
		}

		public IEnumerable<ContentItem> Siblings(ContentItem item = null, ItemFilter filter = null)
		{
			if (item == null) item = CurrentItem;
			if (item.Parent == null) return Enumerable.Empty<ContentItem>();

			return item.Parent.GetChildren(filter ?? DefaultFilter());
		}

		public ContentItem PreviousSibling(ContentItem item = null)
		{
			if (item == null) item = CurrentItem;

			ContentItem previous = null;
			foreach (var sibling in Siblings(item))
			{
				if (sibling == item)
					return previous;
				
				previous = sibling;
			}
			return null;
		}

		public ContentItem NextSibling(ContentItem item = null)
		{
			if (item == null) item = CurrentItem;

			bool next = false;
			foreach (var sibling in Siblings(item))
			{
				if (next)
					return sibling;
				if (sibling == item)
					next = true;
			}
			return null;
		}

		public int LevelOf(ContentItem item = null)
		{
			return Ancestors(item).Count();
		}

		public ContentItem AncestorAtLevel(int level)
		{
			return Ancestors().Reverse().Skip(level).FirstOrDefault();
		}

		public ContentItem Parent(ContentItem item = null)
		{
			if (item == null) item = CurrentItem;
			if (item == StartPage) return null;

			return item.Parent;
		}

		public ContentItem Path(string path, ContentItem startItem = null)
		{
			return (startItem ?? StartPage).FindPath(path).CurrentItem;
		}
	}
}