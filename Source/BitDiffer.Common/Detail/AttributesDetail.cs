using System;
using System.Collections.Generic;
using System.Text;

using BitDiffer.Common.Misc;
using BitDiffer.Common.Configuration;

namespace BitDiffer.Common.Model
{
	[Serializable]
	public class AttributesDetail : ParentDetail
	{
		public AttributesDetail(RootDetail parent)
			: base(parent, "Attributes")
		{
		}

		protected override void GetTextDescriptionBriefMembers(StringBuilder sb)
		{
			AppendClauseText(sb, "Assembly attributes changed");
		}

		protected override void GetHtmlChangeDescriptionBriefMembers(StringBuilder sb)
		{
			AppendClauseHtml(sb, false, "Assembly attributes changed");
		}

		protected override void GetMarkdownDescriptionBriefMembers(StringBuilder sb)
		{
			AppendClauseMarkdown(sb, "Assembly attributes changed");
		}

		protected override void ProcessChildChange(RootDetail child, ChangeType change)
		{
			base.ProcessChildChange(child, change);

			if (_changeAllChildren != ChangeType.None)
			{
				_changeAllChildren = ChangeType.MembersChangedNonBreaking;
			}
		}

		protected override int RelativeSortOrder
		{
			get { return -8; }
		}

		protected override string SerializeGetElementName()
		{
			return "Attributes";
		}
	}
}
