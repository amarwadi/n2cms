﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace N2.Definitions
{
	/// <summary>
	/// Marks an item that can be commented.
	/// </summary>
	public interface ICommentable
	{
		/// <summary>The title of the commented item.</summary>
		string Title { get; }
	}
}
