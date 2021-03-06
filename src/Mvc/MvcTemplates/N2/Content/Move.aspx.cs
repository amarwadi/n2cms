using System;
using N2.Definitions;
using N2.Edit.Web;
using N2.Integrity;
using N2.Web;
using N2.Security;

namespace N2.Edit
{
	[NavigationSeparatorPlugin("copyPasteSeparator", 40)]
    [NavigationLinkPlugin("Cut", "move", "javascript:n2nav.memorize('{selected}','move');", "", "{ManagementUrl}/Resources/icons/cut.png", 42,
		GlobalResourceClassName = "Navigation",
		RequiredPermission = Permission.Publish)]
    [ToolbarPlugin("CUT", "move", "javascript:n2.memorize('{selected}','move');", ToolbarArea.Operations, "", "{ManagementUrl}/Resources/icons/cut.png", 30, ToolTip = "move",
		GlobalResourceClassName = "Toolbar",
		RequiredPermission = Permission.Publish)]
	public partial class Move : EditPage
	{
		protected void Page_Load(object sender, EventArgs e)
		{
            btnCancel.NavigateUrl = Selection.SelectedItem.FindPath(PathData.DefaultAction).RewrittenUrl;

			ContentItem toMove = Selection.MemorizedItem;
			if (toMove == null)
				return;
            
            if (!IsPostBack)
			{
                pnlNewName.Visible = false;

                try
                {
					PerformMove(toMove);
                }
                catch (NameOccupiedException ex)
                {
                    SetErrorMessage(cvMove, ex);
                    pnlNewName.Visible = true;
                }
                catch (DestinationOnOrBelowItselfException ex)
                {
					SetErrorMessage(cvMove, ex);
					btnMove.Enabled = false;
                }
                catch (PermissionDeniedException ex)
                {
					SetErrorMessage(cvMove, ex);
					btnMove.Enabled = false;
                }
                catch (NotAllowedParentException ex)
                {
					SetErrorMessage(cvMove, ex);
					btnMove.Enabled = false;
                }
				catch(NullReferenceException ex)
				{
					System.Diagnostics.Trace.WriteLine(ex);
					SetErrorMessage(cvException, "Nothing to move");
				}
                catch (Exception ex)
                {
                    SetErrorMessage(cvMove, ex);
                }

				txtNewName.Text = toMove.Name;
			}

			LoadDefaultsAndInfo(toMove, Selection.SelectedItem);
		}

		private void PerformMove(ContentItem toMove)
		{
			EnsureAuthorization(Permission.Write);
			EnsureAuthorization(toMove, toMove.IsPublished() ? Permission.Publish : Permission.Write);

			Engine.Persister.Move(toMove, Selection.SelectedItem);
			Refresh(toMove, ToolbarArea.Both);
		}

		private void LoadDefaultsAndInfo(ContentItem moved, ContentItem destination)
		{
            Title = string.Format(GetLocalResourceString("MovePage.TitleFormat"),
                                  moved.Title,
                                  destination.Title);

            from.Text = string.Format(GetLocalResourceString("from.TextFormat"),
                                      moved.Parent != null ? moved.Parent.Path : "",
                                      moved.Name);

            to.Text = string.Format(GetLocalResourceString("to.TextFormat"),
                                    destination.Path,
                                    moved.Name);

            itemsToMove.CurrentItem = moved;
            itemsToMove.DataBind();
		}

		protected void OnMoveClick(object sender, EventArgs e)
		{
			try
			{
				var movedItem = Selection.MemorizedItem;
                movedItem.Name = txtNewName.Text;
				PerformMove(movedItem);
			}
			catch (NameOccupiedException ex)
			{
				SetErrorMessage(cvMove, ex);
				pnlNewName.Visible = true;
			}
			catch (DestinationOnOrBelowItselfException ex)
			{
				SetErrorMessage(cvMove, ex);
			}
			catch (NotAllowedParentException ex)
			{
				SetErrorMessage(cvMove, ex);
			}
			catch (N2Exception ex)
			{
				SetErrorMessage(cvMove, ex);
			}
		}
	}
}