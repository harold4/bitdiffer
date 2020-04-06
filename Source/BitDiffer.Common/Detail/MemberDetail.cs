using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using System.ComponentModel;
using System.Collections;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Xml;

using BitDiffer.Common.Interfaces;
using BitDiffer.Common.Utility;
using BitDiffer.Common.Misc;
using BitDiffer.Common.Configuration;

namespace BitDiffer.Common.Model
{
	[Serializable]
	public class MemberDetail : RootDetail, IHaveVisibility, IHaveObsoleteAttribute
	{
		protected string _declaration;
		protected string _declarationHtml;
		protected string _declarationMarkdown;
		protected string _category;
		protected Visibility _visibility = Visibility.Invalid;
		protected AttributeDetail _obsoleteAttribute;

		public MemberDetail()
		{
		}

		public MemberDetail(RootDetail parent, MemberInfo mi)
			: base(parent, "")
		{
            // In some cases attributes do not resolve with .winmd files so this logs a warning but continues
            try
            {
				var cads = CustomAttributeData.GetCustomAttributes(mi)
					.OrderBy(x => x.AttributeType.Name)
					.ThenBy(x => x.AttributeType.Namespace)
					.ThenBy(x => x.ConstructorArguments.Count)
					.ThenBy(x => x.ConstructorArguments.FirstOrDefault().Value);

				foreach (CustomAttributeData cad in cads)
                {
	                var attribute = new AttributeDetail(this, cad);
	                switch (attribute.AttributeType)
	                {
						case AttributeType.Obsolete:
							// We found an ObsoleteAttribute; store a reference to it on the member itself.
							// There *should* be only one ObsoleteAttribute on a member, but IL does not prohibit it (C# does)
							// and so code generators like Fody.Obsolete can accidentally add a second one.
							// In this case, take the first as the "winner", as Microsoft's compiler behaves this way.
							if (_obsoleteAttribute == null)
							{
								_obsoleteAttribute = attribute;
							}

							break;
	                }

	                _children.Add(attribute);
                }
            }
            catch(TypeLoadException ex)
            {
                Log.Warn("Failed loading custom attribute data for type '{0}': {1}", parent.FullName, ex.Message);
            }
		}

		public MemberDetail(RootDetail parent, string name)
			: base(parent, name)
		{
		}

		public string Declaration
		{
			get { return _declaration; }
			set { _declaration = value; }
		}

		protected override bool SuppressBreakingChangesInChildren
		{
			get { return (_visibility != Visibility.Public); }
		}

		public Visibility Visibility
		{
			get 
			{
				// TODO ask maintainer why are we checking MemberDetail.Visibility up the tree and never using it
				//Visibility vis = _visibility;
				//MemberDetail check = this.Parent as MemberDetail;

				//while (vis == Visibility.Invalid && check != null)
				//{
				//	vis = check.Visibility;
				//	check = check.Parent as MemberDetail;
				//}

				return _visibility;
			}

			set { _visibility = value; }
		}

		public AttributeDetail ObsoleteAttribute
		{
			get { return _obsoleteAttribute; }
			set { _obsoleteAttribute = value; }
		}

		protected override void SerializeWriteRawContent(XmlWriter writer)
		{
			base.SerializeWriteRawContent(writer);

			if (_declaration != null)
			{
				writer.WriteAttributeString("Declaration", _declaration);
			}

			if (_visibility != Visibility.Invalid)
			{
				writer.WriteAttributeString("Visibility", _visibility.ToString().ToLower());
			}
		}

		public override string ToString()
		{
			if (_visibility != Visibility.Invalid)
			{
				return VisibilityUtil.GetVisibilityString(_visibility) + " " + _declaration;
			}
			else
			{
				return _declaration;
			}
		}

		protected override ChangeType CompareInstance(ICanCompare previous, bool suppressBreakingChanges)
		{
			ChangeType change = base.CompareInstance(previous, suppressBreakingChanges);

			change |= CompareVisibility(previous, suppressBreakingChanges);
			change |= CompareObsoleteStatus(previous, suppressBreakingChanges);
			change |= CompareDeclaration(previous, suppressBreakingChanges);

			return change;
		}

		protected virtual ChangeType CompareDeclaration(ICanCompare previous, bool suppressBreakingChanges)
		{
			MemberDetail other = (MemberDetail)previous;

			if (string.Compare(_declaration, other._declaration) != 0)
			{
				if ((!suppressBreakingChanges) && (_visibility == Visibility.Public && other._visibility == Visibility.Public))
				{
					return ChangeType.DeclarationChangedBreaking;
				}
				else
				{
					return ChangeType.DeclarationChangedNonBreaking;
				}
			}

			return ChangeType.None;
		}

		protected virtual ChangeType CompareVisibility(ICanCompare previous, bool suppressBreakingChanges)
		{
			MemberDetail other = (MemberDetail)previous;

			return VisibilityUtil.GetVisibilityChange(other._visibility, _visibility, suppressBreakingChanges);
		}

		protected virtual ChangeType CompareObsoleteStatus(ICanCompare previous, bool suppressBreakingChanges)
		{
			MemberDetail other = (MemberDetail)previous;

			return ObsoleteUtil.GetObsoleteChange(other._obsoleteAttribute, _obsoleteAttribute, suppressBreakingChanges);
		}

		protected override void ApplyFilterInstance(ComparisonFilter filter)
		{
			base.ApplyFilterInstance(filter);

			if (!filter.IncludePublic || !filter.IncludeProtected || !filter.IncludePrivate || !filter.IncludeInternal)
			{
				if (FilterMatches(filter, _visibility))
				{
					SetFilterStatus(FilterStatus.Include);
				}
				else
				{
					if (this.NavigateBackward == null)
					{
						SetFilterStatus(FilterStatus.ExcludeBlock);
					}
					else
					{
						Visibility priorVisibility = ((MemberDetail)this.NavigateBackward).Visibility;

                        if (FilterMatches(filter, priorVisibility))
                        {
							SetFilterStatus(FilterStatus.Include);
						}
						else
						{
							SetFilterStatus(FilterStatus.ExcludeBlock);
						}
					}
				}
			}
		}

        private bool FilterMatches(ComparisonFilter filter, Visibility visibility)
        {
            switch (visibility)
            {
                case Visibility.Public:                 return filter.IncludePublic;
                case Visibility.Exported:               return filter.IncludePublic;
                case Visibility.Internal:               return filter.IncludeInternal;
                case Visibility.Private:                return filter.IncludePrivate;
                case Visibility.Protected:              return filter.IncludeProtected;
                case Visibility.ProtectedInternal:      return filter.IncludeProtected || filter.IncludeInternal;
                case Visibility.ProtectedPrivate:       return filter.IncludePrivate || filter.IncludeProtected;
                default:                                return false;
            }
        }

		public override string GetTextTitle()
		{
			if ((_visibility != Visibility.Invalid) && (_category != null))
			{
				return VisibilityUtil.GetVisibilityString(_visibility) + " " + _category + " " + this.FullName;
			}
			else
			{
				return this.FullName;
			}
		}

		public override string GetTextDeclaration()
		{
			if (_visibility != Visibility.Invalid)
			{
				return VisibilityUtil.GetVisibilityString(_visibility) + " " + _declaration;
			}
			else
			{
				return _declaration;
			}
		}

		public override string GetHtmlDeclaration()
		{
			return _declarationHtml;
		}

		public override string GetMarkdownDeclaration()
		{
			return _declarationMarkdown;
		}

		protected virtual void AppendAttributesDeclaration(CodeStringBuilder csb)
		{
			AppendMode oldMode = csb.Mode;
			csb.Mode = AppendMode.NonText;

			foreach (AttributeDetail ad in FilterChildren<AttributeDetail>())
			{
				if (ad.AppendInCode)
				{
					csb.AppendText("[");
					csb.AppendRaw(html: ad.GetHtmlDeclaration(), markdown: ad.GetMarkdownDeclaration());
					csb.AppendText("]");
					csb.AppendNewline();
				}
			}

			csb.Mode = oldMode;
		}

		protected override void WriteMarkdownDeclaration(TextWriter tw, bool writeDeclaringAssembly)
		{
			//tw.WriteLine($"<!-- [Member] {GetType().Name} - {ToString()} -->");

			// Write the markdown in multiple lines, with the declaration inside a code block.
			// ASSUMPTION: code is always C#
			if (writeDeclaringAssembly)
			{
				tw.WriteLine(MarkdownUtility.ToInlineCode(DeclaringAssembly.Location));
				tw.WriteLine();
			}

			if (this.Status == Status.Present)
			{
				tw.WriteLine("```csharp");
				tw.WriteLine(GetMarkdownDeclaration());
				tw.WriteLine("```");
			}
			else
			{
				tw.WriteLine("*Not Defined*");
			}

			tw.WriteLine();
		}
	}
}
