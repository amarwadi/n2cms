#region License
/* Copyright (C) 2006-2007 Cristian Libardo
 *
 * This program is free software; you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation; either version 2 of the License, or
 * (at your option) any later version.
 */
#endregion

using System;
using N2;

namespace N2.Integrity
{
    /// <summary>
	/// A class that encapsulates integrity checks performed when content 
	/// items are manipulated through <see cref="Context"/>. This class' 
	/// responsibilities include maintaining system integrity integrity by 
	/// not allowing recusive parent-child relationships, siblings with the 
	/// same name and un-allowed parent-child combinations.
	/// </summary>
    public class DefaultIntegrityManager : IIntegrityManager
    {
		#region Private Fields
		private readonly Web.IUrlParser urlParser;
		private readonly Definitions.IDefinitionManager definitions;
		#endregion

		#region Constructor
		/// <summary>Creates a new instance of the <see cref="DefaultIntegrityManager"/>.</summary>
		/// <param name="definitions">The definition manager.</param>
		/// <param name="urlParser"></param>
		public DefaultIntegrityManager(Definitions.IDefinitionManager definitions, Web.IUrlParser urlParser)
		{
			this.definitions = definitions;
			this.urlParser = urlParser;
		}

		#endregion

        #region Methods

		/// <summary>Check if an item can be moved to a destination.</summary>
		/// <param name="source">The item to move.</param>
		/// <param name="destination">The destination below which the item should be moved to.</param>
		/// <returns>True if the item can be moved.</returns>
		public bool CanMove(ContentItem source, ContentItem destination)
		{
			return null == GetMoveException(source, destination);
		}

		/// <summary>Check if an item can be copied to a destination.</summary>
		/// <param name="source">The item to copy.</param>
		/// <param name="destination">The destination below which the item should be copied to.</param>
		/// <returns>True if the item can be copied.</returns>
		public bool CanCopy(ContentItem source, ContentItem destination)
		{
			return null == GetCopyException(source, destination);
		}

		/// <summary>Check the current state of an item to see if it ca be deleted.</summary>
		/// <param name="item">The item that should be deleted.</param>
		/// <returns>True if the item can be deleted.</returns>
		public bool CanDelete(ContentItem item)
		{
			return null == GetDeleteException(item);
		}

		/// <summary>Check the current state of an item to see if it ca be saved.</summary>
		/// <param name="item">The item that should be saved.</param>
		/// <returns>True if the item can be saved.</returns>
		public bool CanSave(ContentItem item)
		{
			return null == GetSaveException(item);
		}
		#endregion

		#region Helper Methods
		/// <summary>Checks if an item can be moved to a destination.</summary>
		/// <param name="source">The item that is to be moved.</param>
		/// <param name="destination">The destination onto which the item is to be moved.</param>
		/// <returns>Null if the item can be moved or an exception if the item can't be moved.</returns>
		public virtual N2Exception GetMoveException(ContentItem source, ContentItem destination)
        {
			if (IsDestinationBelowSource(source, destination))
				return new DestinationOnOrBelowItselfException(source, destination);

			if (IsNameOccupiedOnDestination(source, destination))
				return new NameOccupiedException(source, destination);

			if (!IsTypeAllowedBelowDestination(source, destination))
				return new Definitions.NotAllowedParentException(definitions.GetDefinition(source.GetType()), destination.GetType());

			return null;
        }

		/// <summary>Check if an item can be copied.</summary>
		/// <exception cref="NameOccupiedException"></exception>
		/// <exception cref="N2Exception"></exception>
		public virtual N2Exception GetCopyException(ContentItem source, ContentItem destination)
        {
			if (IsNameOccupiedOnDestination(source, destination))
                return new NameOccupiedException(source, destination);

			if (!IsTypeAllowedBelowDestination(source, destination))
				return new Definitions.NotAllowedParentException(definitions.GetDefinition(source.GetType()), destination.GetType());

			return null;
		}
        
		/// <summary>Check if an item can be deleted.</summary>
		/// <exception cref="N2Exception"></exception>
		public virtual N2Exception GetDeleteException(ContentItem item)
        {
			if (urlParser.IsRootOrStartPage(item))
				return new CannotDeleteRootException();

			return null;
		}

		/// <summary>Check if an item can be saved.</summary>
		/// <exception cref="NameOccupiedException"></exception>
		/// <exception cref="N2Exception"></exception>
		public virtual N2Exception GetSaveException(ContentItem item)
        {
            if (!IsLocallyUnique(item.Name, item))
                return new NameOccupiedException(item, item.Parent);

			if (!IsTypeAllowedBelowDestination(item, item.Parent))
				return new Definitions.NotAllowedParentException(definitions.GetDefinition(item.GetType()), item.Parent.GetType());

			return null;
		}

		/// <summary>Checks that destination have no child item with the same name.</summary>
		public virtual bool IsNameOccupiedOnDestination(ContentItem source, ContentItem destination)
		{
			if (source == null) throw new ArgumentNullException("source");
			if (destination == null) throw new ArgumentNullException("destination");

			ContentItem existingItem = destination.GetChild(source.Name);
			return existingItem != null && existingItem != source;
		}

		/// <summary>Checks that the destination isn't below the source.</summary>
		public virtual bool IsDestinationBelowSource(ContentItem source, ContentItem destination)
		{
			for (ContentItem parent = destination; parent != null; parent = parent.Parent)
				if (parent == source)
					return true;
			return false;
		}

		/// <summary>Find out if an item name is occupied.</summary>
		/// <param name="name">The name to check.</param>
		/// <param name="item">The item whose siblings (other items with the same parent) might be have a clashing name.</param>
		/// <returns>True if the name is unique.</returns>
        public virtual bool IsLocallyUnique(string name, ContentItem item)
        {
            ContentItem parentItem = item.Parent;
            if (parentItem != null)
            {
				foreach (ContentItem potentiallyClashingItem in parentItem.Children)
				{
					if (potentiallyClashingItem.Name.Equals(name, StringComparison.InvariantCultureIgnoreCase)
						&& !potentiallyClashingItem.Equals(item))
					{
						return false;
					}
				}
            }
            return true;
        }

		/// <summary>Check that the source item type is allowed below the destination. Throws an exception if the item isn't allowed.</summary>
		/// <param name="source">The child item</param>
		/// <param name="destination">The parent item</param>
		public virtual bool IsTypeAllowedBelowDestination(ContentItem source, ContentItem destination)
		{
			if (destination != null)
			{
				Definitions.ItemDefinition sourceDefinition = definitions.GetDefinition(source.GetType());
				Definitions.ItemDefinition destinationDefinition = definitions.GetDefinition(destination.GetType());

				return destinationDefinition.IsChildAllowed(sourceDefinition);
			}
			return true;
		}
        #endregion
	}
}
