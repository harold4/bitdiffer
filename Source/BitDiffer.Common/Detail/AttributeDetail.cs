using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using BitDiffer.Common.Interfaces;
using BitDiffer.Common.Utility;
using BitDiffer.Common.Misc;
using BitDiffer.Common.Configuration;

namespace BitDiffer.Common.Model
{
    [Serializable]
    public class AttributeDetail : MemberDetail
    {
        public AttributeDetail()
        {
        }

        public AttributeDetail(RootDetail parent, CustomAttributeData cad)
            : base(parent, cad.Constructor.DeclaringType.FullName)
        {
            _declaration = cad.ToString();

            CodeStringBuilder csb = new CodeStringBuilder();

            AppendAttributesDeclaration(csb);

            csb.AppendType(cad.Constructor.DeclaringType);

            using (var e = cad.ConstructorArguments.GetEnumerator())
            {
                if (e.MoveNext())
                {
                    csb.AppendText("(");
                    csb.AppendQuotedValue(e.Current.Value);
                    while (e.MoveNext())
                    {
                        csb.AppendText(", ");
                        csb.AppendQuotedValue(e.Current.Value);
                    }
                    csb.AppendText(")");
                }
            }

            _declaration = csb.ToString();
            _declarationHtml = csb.ToHtmlString();
            _declarationMarkdown = csb.ToMarkdownString();

            if (cad.AttributeType.IsAssignableFrom(typeof(ExtensionAttribute)))
            {
                AttributeType = AttributeType.Extension;
                AppendInCode = false;
            }
            else if (cad.AttributeType.IsAssignableFrom(typeof(ObsoleteAttribute)))
            {
                AttributeType = AttributeType.Obsolete;
            }
            else if (cad.AttributeType.IsAssignableFrom(typeof(CompilerGeneratedAttribute)))
            {
                AttributeType = AttributeType.CompilerGenerated;
                AppendInCode = false;
            }
        }

        protected override bool FullNameRoot
        {
            get { return true; }
        }

        public AttributeType AttributeType { get; }

        public override string GetTextTitle()
        {
            return "Attribute " + _name;
        }

        public bool AppendInCode { get; } = true;


        protected override void ApplyFilterInstance(ComparisonFilter filter)
        {
            base.ApplyFilterInstance(filter);

            if ((filter.IgnoreAssemblyAttributeChanges) && (this.Parent.GetType() == typeof(AttributesDetail)) && (this.Parent.Parent.GetType() == typeof(AssemblyDetail)))
            {
                _changeThisInstance = ChangeType.None;
            }
        }

        protected override string SerializeGetElementName()
        {
            return "Attribute";
        }
    }
}
