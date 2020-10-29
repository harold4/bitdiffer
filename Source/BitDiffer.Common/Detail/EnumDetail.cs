using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;

using BitDiffer.Common.Utility;
using BitDiffer.Common.Misc;
using BitDiffer.Common.Configuration;

namespace BitDiffer.Common.Model
{
	[Serializable]
	public class EnumDetail : TypeDetail
	{
		public EnumDetail()
		{
		}

		public EnumDetail(RootDetail parent, Type type)
			: base(parent, type)
		{
			_visibility = VisibilityUtil.GetVisibilityFor(type);
			_category = "enum";

			foreach (string name in Enum.GetNames(type))
			{
				_children.Add(
					new EnumItemDetail(
						this,
						name,
						Convert.ToInt64(
							type.GetField(name, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static)
								.GetRawConstantValue()),
						_visibility));
			}

			CodeStringBuilder csb = new CodeStringBuilder();

			AppendAttributesDeclaration(csb);

			csb.Mode = AppendMode.NonText;
			csb.AppendVisibility(_visibility);
			csb.AppendText(" ");
			csb.Mode = AppendMode.All;

			csb.AppendKeyword("enum ");
			csb.AppendText(type.Name);

			csb.Mode = AppendMode.NonText;
			csb.AppendNewline();
			csb.AppendText("{");
			csb.AppendNewline();

			foreach (EnumItemDetail eid in FilterChildren<EnumItemDetail>())
			{
				csb.AppendIndent();
				csb.AppendText(eid.GetHtmlDeclaration());
				csb.AppendText(",");
				csb.AppendNewline();
			}

			csb.RemoveCharsFromEnd("<br>".Length);
			csb.RemoveCharsFromEnd(",".Length);
			csb.AppendNewline();
			csb.AppendText("}");
			csb.Mode = AppendMode.All;

			_declaration = csb.ToString();
			_declarationHtml = csb.ToHtmlString();
			_declarationMarkdown = csb.ToMarkdownString();
		}

		public override bool CollapseChildren
		{
			get { return true; }
		}

		protected override string SerializeGetElementName()
		{
			return "Enum";
		}
	}
}
