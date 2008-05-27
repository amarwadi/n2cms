using System;
using System.Security.Principal;
using System.Web;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using N2.Definitions;
using N2.Security;

namespace N2.Details
{
	/// <summary>
	/// Basic implementation of the <see cref="IEditable"/>. This 
	/// class implements properties, provides comparison and equality but does
	/// not add any controls.
	/// </summary>
	public abstract class AbstractEditableAttribute : Attribute, IEditable, ISecurable
	{
		#region Fields
		private string[] authorizedRoles;
		private string containerName = null;
		private string name;
		private bool required = false;
		private string requiredMessage = null;
		private string requiredText = "*";
		private int sortOrder;
		private string title;
		private bool validate = false;
		private string validationExpression = null;
		private string validationMessage = null;
		private string validationText = "*";
		private string helpTitle;
		private string helpText;
		private string localizationClassKey = "Editables";

		#endregion

		#region Properties

		/// <summary>Gets or sets whether a required field validator should be appended.</summary>
		public bool Required
		{
			get { return required; }
			set { required = value; }
		}
			
		/// <summary>Gets or sets the validation expression for a regular expression validator.</summary>
		public string ValidationExpression
		{
			get { return validationExpression; }
			set { validationExpression = value; }
		}

		/// <summary>Gets or sets whether a regular expression validator should be added.</summary>
		public bool Validate
		{
			get { return validate; }
			set { validate = value; }
		}

		/// <summary>Gets or sets the message for the regular expression validator.</summary>
		public string ValidationMessage
		{
			get { return validationMessage ?? Title + " is invalid."; }
			set { validationMessage = value; }
		}

		/// <summary>Gets or sets the message for the required field validator.</summary>
		public string RequiredMessage
		{
			get { return requiredMessage ?? Title + " is required."; }
			set { requiredMessage = value; }
		}

		/// <summary>Gets or sets the text for the required field validator.</summary>
		public string RequiredText
		{
			get { return requiredText; }
			set { requiredText = value; }
		}

		/// <summary>Gets or sets the text for the regular expression validator.</summary>
		public string ValidationText
		{
			get { return validationText; }
			set { validationText = value; }
		}

		#endregion

		#region Constructors

		/// <summary>Default/empty constructor.</summary>
		public AbstractEditableAttribute()
		{
		}

		/// <summary>Initializes a new instance of the AbstractEditableAttribute.</summary>
		/// <param name="title">The label displayed to editors</param>
		/// <param name="name">The name used for equality comparison and reference.</param>
		/// <param name="sortOrder">The order of this editor</param>
		public AbstractEditableAttribute(string title, int sortOrder)
		{
			Title = title;
			SortOrder = sortOrder;
		}

		/// <summary>Initializes a new instance of the AbstractEditableAttribute.</summary>
		/// <param name="title">The label displayed to editors</param>
		/// <param name="name">The name used for equality comparison and reference.</param>
		/// <param name="sortOrder">The order of this editor</param>
		public AbstractEditableAttribute(string title, string name, int sortOrder)
		{
			Title = title;
			Name = name;
			SortOrder = sortOrder;
		}

		#endregion

		#region IEditable Members

		/// <summary>Gets or sets the label used for presentation.</summary>
		public string Title
		{
			get { return title; }
			set { title = value; }
		}

		/// <summary>Updates the object with the values from the editor.</summary>
		/// <param name="item">The object to update.</param>
		/// <param name="editor">The editor contorl whose values to update the object with.</param>
		/// <returns>True if the item was changed (and needs to be saved).</returns>
		public abstract bool UpdateItem(ContentItem item, Control editor);

		public abstract void UpdateEditor(ContentItem item, Control editor);

		/// <summary>Gets or sets the name of the detail (property) on the content item's object.</summary>
		public string Name
		{
			get { return name; }
			set { name = value; }
		}

		/// <summary>Gets or sets the container name associated with this editor. The name must match a container attribute defined on the item class.</summary>
		public string ContainerName
		{
			get { return containerName; }
			set { containerName = value; }
		}

		/// <summary>Gets or sets the order of the associated control</summary>
		public int SortOrder
		{
			get { return sortOrder; }
			set { sortOrder = value; }
		}

		/// <summary>Adds a label and an editor to a panel.</summary>
		/// <param name="container"The container onto which the panel is added.></param>
		/// <returns>A reference to the addeed editor.</returns>
		/// <remarks>Please note that this method was abstract before version 1.3.1. It's now recommended to add the editor through the <see cref="AddEditor"/> method.</remarks>
		public virtual Control AddTo(Control container)
		{
			Control panel = AddPanel(container);
			Label label = AddLabel(panel);
			Control editor = AddEditor(panel);
			if (label != null && editor != null && !string.IsNullOrEmpty(editor.ID))
				label.AssociatedControlID = editor.ID;

			if (Required)
				AddRequiredFieldValidator(panel, editor);
			if (Validate)
				AddRegularExpressionValidator(panel, editor);

			AddHelp(panel);

			return editor;
		}

		protected virtual Control AddHelp(Control container)
		{
			string text = GetLocalizedText("HelpText") ?? HelpText;
			string title = GetLocalizedText("HelpTitle") ?? HelpTitle;

			if (!string.IsNullOrEmpty(text))
			{
				HtmlGenericControl helpPanel = new HtmlGenericControl("span");
				helpPanel.ID = "hp_" + Name;
				helpPanel.Attributes["class"] = "helpPanel";
				container.Controls.Add(helpPanel);

				AddHelpButton(helpPanel, title);

				HtmlGenericControl div = new HtmlGenericControl("span");
				div.ID = "hd_" + Name;
				div.Attributes["class"] = "helpText";
				helpPanel.Controls.Add(div);

				HtmlGenericControl header = new HtmlGenericControl("b");
				header.InnerHtml = title;
				div.Controls.Add(header);

				HtmlGenericControl span = new HtmlGenericControl("span");
				span.InnerHtml = text;
				div.Controls.Add(span);

			}
			else if (!string.IsNullOrEmpty(title))
			{
				return AddHelpButton(container, title);
			}

			return null;
		}

		private HtmlImage AddHelpButton(Control container, string tooltip)
		{
			HtmlImage img = new HtmlImage();
			img.ID = "hi_" + Name;
			img.Attributes["class"] = "help";
			img.Attributes["title"] = tooltip;
			img.Src = Utility.ToAbsolute("~/Edit/Img/Ico/Png/help.png");
			container.Controls.Add(img);
			return img;
		}

		/// <summary>Gets a localized resource string from the global resource with the name denoted by <see cref="LocalizationClassKey"/>. The resource key follows the pattern <see cref="Name"/>.key where the name is the name of the detail and the key is the supplied parameter.</summary>
		/// <param name="key">A part of the resource key used for finding the localized resource.</param>
		/// <returns>A localized string if found, or null.</returns>
		protected virtual string GetLocalizedText(string key)
		{
			try
			{
				return Utility.GetGlobalResourceString(LocalizationClassKey, Name + "." + key) as string;
			}
			catch
			{
				return null; // it's okay to use default text
			}
		}

		/// <summary>Find out whether a user has permission to edit this detail.</summary>
		/// <param name="user">The user to check.</param>
		/// <returns>True if the user has the required permissions.</returns>
		public virtual bool IsAuthorized(IPrincipal user)
		{
			if (authorizedRoles == null)
				return true;
			else if (user == null)
				return false;

			foreach (string role in AuthorizedRoles)
				if (string.Equals(user.Identity.Name, role, StringComparison.OrdinalIgnoreCase) || user.IsInRole(role))
					return true;
			return false;
		}

		int IComparable<IContainable>.CompareTo(IContainable other)
		{
			int delta = SortOrder - other.SortOrder;
			return delta != 0 ? delta : Name.CompareTo(other.Name);
		}

		/// <summary>Compares the sort order of editable attributes.</summary>
		public int CompareTo(IEditable other)
		{
			if (SortOrder != other.SortOrder)
				return SortOrder - other.SortOrder;
			else if (Title != null && other.Title != null)
				return Title.CompareTo(other.Title);
			else if (Title != null)
				return -1;
			else if (other.Title != null)
				return 1;
			else
				return 0;
		}

		#endregion

		#region ISecurable Members

		/// <summary>Gets or sets roles allowed to edit this detail. This property can be set by the DetailAuthorizedRolesAttribute.</summary>
		public string[] AuthorizedRoles
		{
			get { return authorizedRoles; }
			set { authorizedRoles = value; }
		}

		public string HelpTitle
		{
			get { return helpTitle; }
			set { helpTitle = value; }
		}

		public string HelpText
		{
			get { return helpText; }
			set { helpText = value; }
		}

		public string LocalizationClassKey
		{
			get { return localizationClassKey; }
			set { localizationClassKey = value; }
		}

		#region Equals & GetHashCode

		/// <summary>Checks another object for equality.</summary>
		/// <param name="obj">The other object to check.</param>
		/// <returns>True if the items are of the same type and have the same name.</returns>
		public override bool Equals(object obj)
		{
			IUniquelyNamed other = obj as IUniquelyNamed;
			if (other == null)
				return false;
			return (Name == other.Name);
		}

		/// <summary>Gets a hash code based on the attribute's name.</summary>
		/// <returns>A hash code.</returns>
		public override int GetHashCode()
		{
			return Name.GetHashCode();
		}

		#endregion

		#region Methods

		/// <summary>Adds the panel to the container. Creating this panel and adding labels and editors to it will help to avoid web controls from interfere with each other.</summary>
		/// <param name="container">The container onto which add the panel.</param>
		/// <returns>A panel that can be used to add editor and label.</returns>
		protected virtual Control AddPanel(Control container)
		{
			HtmlGenericControl detailContainer = new HtmlGenericControl("div");
			detailContainer.Attributes["class"] = "editDetail";
			container.Controls.Add(detailContainer);
			return detailContainer;
		}

		/// <summary>Adds a label with the text set to the current Title to the container.</summary>
		/// <param name="container">The container control for the label.</param>
		protected virtual Label AddLabel(Control container)
		{
			Label label = new Label();
			label.ID = "lbl" + Name;
			label.Text = GetLocalizedText("Title") ?? Title;
			label.CssClass = "editorLabel";
			container.Controls.Add(label);
			return label;
		}

		/// <summary>Compares two values regarding null values as equal.</summary>
		protected bool AreEqual(object editorValue, object itemValue)
		{
			return (editorValue == null && itemValue == null)
			       || (editorValue != null && editorValue.Equals(itemValue))
			       || (itemValue != null && itemValue.Equals(editorValue));
		}

		#endregion

		#endregion

		/// <summary>Adds a regular expression validator.</summary>
		/// <param name="container">The container control for this validator.</param>
		/// <param name="editor">The editor control to validate.</param>
		protected virtual Control AddRegularExpressionValidator(Control container, Control editor)
		{
			RegularExpressionValidator rev = new RegularExpressionValidator();
			rev.ID = Name + "_rev";
			rev.ControlToValidate = editor.ID;
			rev.ValidationExpression = GetLocalizedText("ValidationExpression") ?? ValidationExpression;
			rev.Display = ValidatorDisplay.Dynamic;
			rev.Text = GetLocalizedText("ValidationText") ?? ValidationText;
			rev.ErrorMessage = GetLocalizedText("ValidationMessage") ?? ValidationMessage;
			container.Controls.Add(rev);

			return rev;
		}

		/// <summary>Adds a required field validator.</summary>
		/// <param name="container">The container control for this validator.</param>
		/// <param name="editor">The editor control to validate.</param>
		protected virtual Control AddRequiredFieldValidator(Control container, Control editor)
		{
			RequiredFieldValidator rfv = new RequiredFieldValidator();
			rfv.ID = Name + "_rfv";
			rfv.ControlToValidate = editor.ID;
			rfv.Display = ValidatorDisplay.Dynamic;
			rfv.Text = GetLocalizedText("RequiredText") ?? RequiredText;
			rfv.ErrorMessage = GetLocalizedText("RequiredMessage") ?? RequiredMessage;
			container.Controls.Add(rfv);

			return rfv;
		}

		/// <summary>Adds the editor control to the edit panel. This method is invoked by <see cref="AddTo"/> and the editor is prepended a label and wrapped in a panel. To remove these controls also override the <see cref="AddTo"/> method.</summary>
		/// <param name="container">The container onto which to add the editor.</param>
		/// <returns>A reference to the addeed editor.</returns>
		/// <remarks>Please note that this is a breaking change. This method was added after version 1.3.1 to reduce duplicated code induced by having <see cref="AddTo"/> abstract.</remarks>
		protected abstract Control AddEditor(Control container);
	}
}