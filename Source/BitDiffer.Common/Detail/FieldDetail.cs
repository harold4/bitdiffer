using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;

using BitDiffer.Common.Utility;
using BitDiffer.Common.Misc;

namespace BitDiffer.Common.Model
{
    [Serializable]
    public class FieldDetail : MemberDetail
    {
        public FieldDetail()
        {
        }

        public FieldDetail(RootDetail parent, FieldInfo fi)
            : base(parent, fi)
        {
            _name = fi.Name;
            _visibility = VisibilityUtil.GetVisibilityFor(fi);
            _category = "field";

            CodeStringBuilder csb = new CodeStringBuilder();

            AppendAttributesDeclaration(csb);

            csb.Mode = AppendMode.NonText;
            csb.AppendVisibility(_visibility);
            csb.AppendText(" ");
            csb.Mode = AppendMode.All;

            if (fi.IsLiteral)
            {
                csb.AppendKeyword("const ");
            }
            else if (fi.IsStatic)
            {
                csb.AppendKeyword("static ");
            }

            if (fi.IsInitOnly)
            {
                csb.AppendKeyword("readonly ");
            }

            csb.AppendType(fi.FieldType);
            csb.AppendText(" ");
            csb.AppendText(fi.Name);

            if (fi.IsLiteral)
            {
                csb.AppendParameterValue(fi.GetRawConstantValue());
            }

            _declaration = csb.ToString();
            _declarationHtml = csb.ToHtmlString();
            _declarationMarkdown = csb.ToMarkdownString();
        }

        protected override string SerializeGetElementName()
        {
            return "Field";
        }
    }
}
