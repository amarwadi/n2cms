﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace N2.Edit.Workflow.Commands
{
    public class EnsurePublishedCommand : CommandBase<CommandContext>
    {
        public override void Process(CommandContext state)
        {
            if (!state.Content.Published.HasValue)
                state.Content.Published = Utility.CurrentTime();

			if (state.Content.Published.Value > Utility.CurrentTime())
			{
				if(state.Content.VersionOf != null || !state.GetDefinedDetails().Contains("Published"))
					// it's a version -> we assume publish date must be changed
					// no detail editor is defined -> make sure it's published
					state.Content.Published = Utility.CurrentTime();
			}
        }
    }
}
