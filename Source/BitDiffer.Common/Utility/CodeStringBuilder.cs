using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Reflection;

using BitDiffer.Common.Model;
using BitDiffer.Common.Misc;

namespace BitDiffer.Common.Utility
{
    public class CodeStringBuilder
    {
        private AppendMode _mode = AppendMode.All;
        private readonly StringBuilder _sbText = new StringBuilder(25);
        private readonly StringBuilder _sbHtml = new StringBuilder(75);
        private readonly StringBuilder _sbMarkdown = new StringBuilder(50); // TODO choose initial size more rigorously -- 50 is a guess.

        public CodeStringBuilder()
        {
        }

        public CodeStringBuilder(AppendMode mode)
        {
            _mode = mode;
        }

        public AppendMode Mode
        {
            get { return _mode; }
            set { _mode = value; }
        }

        public void AppendText(string text)
        {
            AppendText(text, null);
        }

        public void AppendKeyword(string keyword)
        {
            AppendText(keyword, "keyword");
        }

        public void AppendVisibility(Visibility visibility)
        {
            AppendText(VisibilityUtil.GetVisibilityString(visibility), "visibility");
        }

        public void AppendType(Type type)
        {
            AppendType(type, true);
        }

        public void AppendType(Type type, bool includeNamespace)
        {
            if (type.IsArray)
            {
                // simplify arrays
                Type elementType = type.GetElementType();
                if (elementType != null)
                {
                    OpenText("usertype");
                    AppendType(elementType, includeNamespace);
                    AppendText("[]");
                    CloseText("usertype");
                    return;
                }
            }

            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
            {
                OpenText("usertype");
                AppendType(Nullable.GetUnderlyingType(type), includeNamespace);
                AppendText("?");
                CloseText("usertype");
                return;
            }

            string typeAsKeyword = GetTypeNameAsKeyword(type);
            if (typeAsKeyword != null)
            {
                AppendKeyword(typeAsKeyword);
                return;
            }

            if (type.IsGenericParameter && !type.IsGenericType)
            {
                AppendText(type.Name);
                return;
            }

            string typeName = type.Name;
            // UI only: cut out "Attribute" part
            if (typeof(Attribute).IsAssignableFrom(type))
            {
                typeName = typeName.Substring(0, typeName.Length - 9); // 9 == length of "Attribute"
            }

            // Dont show the namespaces on user types in the UI - but keep them in text, for comparison and reports
            if (includeNamespace)
            {
                AppendMode restore = _mode;

                _mode &= ~AppendMode.NonText;
                AppendText(type.Namespace);
                AppendText(".");
                AppendTypeName(type, type.Name);

                // Now non-text
                _mode |= AppendMode.NonText;
                _mode &= ~AppendMode.Text;
                AppendTypeName(type, typeName);

                _mode = restore;
            }
            else
            {
                AppendTypeName(type, typeName);
            }
        }

        private void AppendTypeName(Type type, string typeName)
        {
            if (type.IsGenericType)
            {
                AppendGeneric(typeName, type.GetGenericArguments(), "usertype");
            }
            else
            {
                AppendText(typeName, "usertype");
            }
        }

        public void OpenText(string css)
        {
            if ((_mode & AppendMode.Html) != 0)
            {
                if (css != null)
                {
                    _sbHtml.Append("<span class='");
                    _sbHtml.Append(css);
                    _sbHtml.Append("'>");
                }
            }
        }

        public void CloseText(string css)
        {
            if ((_mode & AppendMode.Html) != 0)
            {
                if (css != null)
                {
                    _sbHtml.Append("</span>");
                }
            }
        }

        public void AppendText(string word, string css)
        {
            OpenText(css);

            if ((_mode & AppendMode.Text) != 0)
            {
                _sbText.Append(word);
            }

            if ((_mode & AppendMode.Html) != 0)
            {
                _sbHtml.Append(HtmlEncode(word));
            }

            if ((_mode & AppendMode.Markdown) != 0)
            {
                _sbMarkdown.Append(word);
            }

            CloseText(css);

        }

        public void AppendNewline()
        {
            if ((_mode & AppendMode.Text) != 0)
            {
                _sbText.Append(Environment.NewLine);
            }

            if ((_mode & AppendMode.Html) != 0)
            {
                _sbHtml.Append("<br>");
            }

            if ((_mode & AppendMode.Markdown) != 0)
            {
                _sbMarkdown.Append(Environment.NewLine);
            }
        }

        public void AppendIndent()
        {
            if ((_mode & AppendMode.Text) != 0)
            {
                _sbText.Append("     ");
            }

            if ((_mode & AppendMode.Html) != 0)
            {
                _sbHtml.Append("&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;");
            }

            if ((_mode & AppendMode.Markdown) != 0)
            {
                _sbMarkdown.Append("     ");
            }
        }

        public void AppendParameter(ParameterInfo pi)
        {
            AppendParameterType(pi);

            // Dont use parameter names in comparing declarations
            AppendMode restore = _mode;
            _mode &= ~AppendMode.Text;

            if (pi.Name != null)
            {
                AppendText(" ");
                AppendText(pi.Name);
            }

            AppendParameterValue(pi.RawDefaultValue);

            _mode = restore;
        }

        public void AppendParameterType(ParameterInfo pi)
        {
            if (pi.IsIn && pi.IsOut)
            {
                AppendKeyword("ref ");
            }
            else if (pi.IsOut)
            {
                AppendKeyword("out ");
            }

            AppendType(pi.ParameterType);
        }

        public void AppendRaw(string text = null, string html = null, string markdown = null)
        {
            if (text != null)
            {
                _sbText.Append(text);
            }

            if (html != null)
            {
                _sbHtml.Append(html);
            }

            if (markdown != null)
            {
                _sbMarkdown.Append(markdown);
            }
        }

        public void AppendGenericRestrictions(Type type)
        {
            if (!type.IsGenericTypeDefinition)
            {
                return;
            }

            AppendGenericRestrictions(type.GetGenericArguments());
        }

        public void AppendGenericRestrictions(MethodBase mi)
        {
            AppendGenericRestrictions(mi.GetGenericArguments());
        }

        internal void AppendGenericRestrictions(Type[] arguments)
        {
            if (arguments == null || arguments.Length == 0)
            {
                return;
            }

            foreach (Type arg in arguments)
            {
                Type[] constraints = arg.GetGenericParameterConstraints();

                if (constraints == null || constraints.Length == 0)
                {
                    return;
                }

                AppendKeyword(" where ");
                AppendText(arg.Name);
                AppendText(" : ");

                if ((arg.GenericParameterAttributes & GenericParameterAttributes.ReferenceTypeConstraint) != 0)
                {
                    AppendKeyword("class");
                    AppendText(", ");
                }

                if ((arg.GenericParameterAttributes & GenericParameterAttributes.NotNullableValueTypeConstraint) != 0)
                {
                    AppendKeyword("struct");
                    AppendText(", ");
                }

                if ((arg.GenericParameterAttributes & GenericParameterAttributes.DefaultConstructorConstraint) != 0)
                {
                    AppendKeyword("new");
                    AppendText("(), ");
                }

                foreach (Type constraint in constraints)
                {
                    AppendType(constraint);
                    AppendText(", ");
                }

                RemoveCharsFromEnd(2);
            }
        }

        public void AppendParameterValue(object value)
        {
            if ((value == null) || (value.GetType() == typeof(DBNull)))
            {
                return;
            }

            AppendText(" = ");

            AppendQuotedValue(value);
        }

        public void AppendQuotedValue(object value)
        {
            switch (value)
            {
                case null:
                    AppendText("null");
                    break;
                case string s:
                    AppendText("\"" + s + "\"", "string");
                    break;
                case char c:
                    AppendText("'" + c + "'", "string");
                    break;
                case bool b:
                    AppendText(b.ToString().ToLower(), "keyword");
                    break;
                default:
                    AppendText(value.ToString());
                    break;
            }
        }

        public void AppendBaseClasses(Type type)
        {
            if (((type.BaseType == null) || (type.BaseType == typeof(object))) && (type.GetInterfaces().Length == 0))
            {
                return;
            }

            // Dont use base types in comparing declarations.. someday, would be good to do a more intelligent compare (implemented interfaces removed is possibly a breaking change?)
            AppendMode restore = _mode;
            _mode &= ~AppendMode.Text;

            AppendText(" : ");

            if ((type.BaseType != null) && (type.BaseType != typeof(object)))
            {
                AppendType(type.BaseType);
                AppendText(", ");
            }

            foreach (Type intf in type.GetInterfaces())
            {
                AppendType(intf);
                AppendText(", ");
            }

            RemoveCharsFromEnd(2);

            _mode = restore;
        }

        public void RemoveCharsFromEnd(int count)
        {
            if ((_mode & AppendMode.Text) != 0)
            {
                _sbText.Remove(_sbText.Length - count, count);
            }

            if ((_mode & AppendMode.Html) != 0)
            {
                _sbHtml.Remove(_sbHtml.Length - count, count);
            }

            if ((_mode & AppendMode.Markdown) != 0)
            {
                _sbMarkdown.Remove(_sbMarkdown.Length - count, count);
            }
        }

        private string HtmlEncode(string text)
        {
            text = text.Replace("<", "&lt;");
            text = text.Replace(">", "&gt;");
            return text;
        }

        private string GetTypeNameAsKeyword(Type type)
        {
            if (type == typeof(void))
            {
                return "void";
            }

            if (type == typeof(string))
            {
                return "string";
            }
            else if (type == typeof(byte))
            {
                return "byte";
            }
            else if (type == typeof(short))
            {
                return "short";
            }
            else if (type == typeof(int))
            {
                return "int";
            }
            else if (type == typeof(long))
            {
                return "long";
            }
            else if (type == typeof(char))
            {
                return "char";
            }
            else if (type == typeof(bool))
            {
                return "bool";
            }
            else if (type == typeof(DateTime))
            {
                return "DateTime";
            }
            else if (type == typeof(decimal))
            {
                return "decimal";
            }
            else if (type == typeof(double))
            {
                return "double";
            }
            else if (type == typeof(float))
            {
                return "float";
            }
            else if (type == typeof(sbyte))
            {
                return "sbyte";
            }
            else if (type == typeof(ushort))
            {
                return "ushort";
            }
            else if (type == typeof(uint))
            {
                return "uint";
            }
            else if (type == typeof(ulong))
            {
                return "ulong";
            }
            else if (type == typeof(object))
            {
                return "object";
            }

            return null;
        }

        private void AppendGeneric(string name, Type[] genericArguments, string css)
        {
            if ((genericArguments == null) || (genericArguments.Length == 0))
            {
                System.Diagnostics.Debug.Assert(false);
                return;
            }

            int apos = name.IndexOf('`');
            if (apos > 0)
            {
                AppendText(name.Substring(0, apos), css);
            }
            else
            {
                AppendText(name, css);
            }

            AppendText("<");

            foreach (Type gentype in genericArguments)
            {
                AppendType(gentype);
                AppendText(", ");
            }

            RemoveCharsFromEnd(2);
            AppendText(">");
        }

        public void AppendMethodName(MethodBase mi)
        {
            if (!mi.IsGenericMethod)
            {
                AppendText(mi.Name);
                return;
            }

            AppendGeneric(mi.Name, mi.GetGenericArguments(), null);
        }

        public override string ToString()
        {
            return _sbText.ToString();
        }

        public virtual string ToHtmlString()
        {
            return _sbHtml.ToString();
        }

        public virtual string ToMarkdownString()
        {
            return _sbMarkdown.ToString();
        }
    }
}
