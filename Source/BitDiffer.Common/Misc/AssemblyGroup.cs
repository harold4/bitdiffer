using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;
using System.Xml;
using System.IO;

using BitDiffer.Common.Interfaces;
using BitDiffer.Common.Model;
using BitDiffer.Common.Utility;
using BitDiffer.Common.Configuration;

namespace BitDiffer.Common.Misc
{
    public class AssemblyGroup
    {
        private string _name;
        private ChangeType _change;
        private bool _hasErrors;
        private string _errorDetail;
        private List<AssemblyDetail> _assemblies = new List<AssemblyDetail>();

        public string Name
        {
            get { return _name; }
            set { _name = value; }
        }

        public bool HasErrors
        {
            get { return _hasErrors; }
            set { _hasErrors = value; }
        }

        public string ErrorDetail
        {
            get { return _errorDetail; }
            set { _errorDetail = value; }
        }

        public ChangeType Change
        {
            get { return _change; }
            set { _change = value; }
        }

        public List<AssemblyDetail> Assemblies
        {
            get { return _assemblies; }
            set { _assemblies = value; }
        }

        public void PerformCompare(ComparisonFilter filter)
        {
            for (int i = 1; i < _assemblies.Count; i++)
            {
                _assemblies[i].PerformCompare(_assemblies[i - 1]);
                _assemblies[i].ApplyFilter(filter);
            }

            _change = ChangeType.None;

            foreach (AssemblyDetail ad in _assemblies)
            {
                if (ChangeTypeUtil.HasBreaking(ad.Change))
                {
                    _change = ChangeType.MembersChangedBreaking;
                    break;
                }
                else if (ChangeTypeUtil.HasNonBreaking(ad.Change))
                {
                    _change = ChangeType.MembersChangedNonBreaking;
                }
            }
        }

        internal void SerializeWriteRawXml(XmlWriter writer)
        {
            writer.WriteStartElement("Group");
            writer.WriteAttributeString("Name", _name);
            writer.WriteAttributeString("Change", _change.ToString());

            writer.WriteStartElement("Assemblies");
            foreach (AssemblyDetail ad in _assemblies)
            {
                ad.SerializeWriteRawXml(writer);
            }
            writer.WriteEndElement();
            writer.WriteEndElement();
        }

        internal void SerializeWriteXml(XmlWriter writer)
        {
            writer.WriteStartElement("Group");
            writer.WriteAttributeString("Name", _name);

            writer.WriteStartElement("Assemblies");
            foreach (AssemblyDetail ad in _assemblies)
            {
                ad.SerializeWriteXml(writer);
            }
            writer.WriteEndElement();
            writer.WriteEndElement();
        }

        public void WriteHtmlDescription(TextWriter tw)
        {
            tw.Write("<h1>");
            tw.Write(_name);
            tw.Write("</h1>");

            if (_hasErrors)
            {
                if (string.IsNullOrEmpty(_errorDetail))
                {
                    tw.Write("<p class='error'>Failed to load one or more versions of this assembly. Examine the log messages pane for detailed error information.</p>");
                }
                else
                {
                    tw.Write("<p class='error'>Failed to load one or more versions of this assembly: " + _errorDetail + "</p>");
                }
            }
            else
            {
                if (_change == ChangeType.None)
                {
                    if (_assemblies.Count == 1)
                    {
                        tw.Write("<p>Only one version of this assembly was found (nothing to compare to!)</p>");
                    }
                    else
                    {
                        tw.Write("<p>No changes were found across all versions of this assembly.</p>");
                    }
                }
                else if (ChangeTypeUtil.HasBreaking(_change))
                {
                    tw.Write("<p class='brkchg'>Breaking changes were found between versions of this assembly.</p>");
                }
                else if (ChangeTypeUtil.HasNonBreaking(_change))
                {
                    tw.Write("<p>Non-breaking changes were found between versions of this assembly.</p>");
                }

                tw.Write("<p>The following files have been compared in this set:</p>");

                tw.Write("<ul>");

                foreach (AssemblyDetail detail in _assemblies)
                {
                    tw.Write("<li>");
                    tw.Write(detail.Location);
                    tw.Write("</li>");
                }

                tw.Write("</ul>");
            }
        }

        public void WriteMarkdownDescription(TextWriter tw)
        {
            tw.WriteLine($"# {_name}");
            tw.WriteLine();

            if (_hasErrors)
            {
                tw.WriteLine("```");
                if (string.IsNullOrEmpty(_errorDetail))
                {
                    tw.WriteLine("Failed to load one or more versions of this assembly. Examine the log messages pane for detailed error information.");
                }
                else
                {
                    tw.WriteLine("Failed to load one or more versions of this assembly:");
                    tw.WriteLine(_errorDetail);
                }
                tw.WriteLine("```");
            }
            else
            {
                if (_change == ChangeType.None)
                {
                    if (_assemblies.Count == 1)
                    {
                        tw.WriteLine("Only one version of this assembly was found (nothing to compare to!)");
                        tw.WriteLine();
                    }
                    else
                    {
                        tw.WriteLine("No changes were found across all versions of this assembly.");
                        tw.WriteLine();
                    }
                }
                else if (ChangeTypeUtil.HasBreaking(_change))
                {
                    tw.WriteLine("**Breaking changes were found between versions of this assembly.**");
                    tw.WriteLine();
                }
                else if (ChangeTypeUtil.HasNonBreaking(_change))
                {
                    tw.WriteLine("Non-breaking changes were found between versions of this assembly.");
                    tw.WriteLine();
                }

                tw.WriteLine("The following files have been compared in this set:");
                tw.WriteLine();

                foreach (AssemblyDetail detail in _assemblies)
                {
                    tw.WriteLine($"* {MarkdownUtility.ToInlineCode(detail.Location)}");
                }

                tw.WriteLine();
            }
        }

        public void WriteHtmlReport(TextWriter tw)
        {
            tw.Write("<div class='report'>");
            WriteHtmlDescription(tw);

            if (_assemblies.Count > 0)
            {
                _assemblies[0].WriteHtmlDescription(tw, true, true);
            }
            tw.Write("</div>");
        }

        public void WriteMarkdownReport(TextWriter tw)
        {
            WriteMarkdownDescription(tw);

            if (_assemblies.Count > 0)
            {
                _assemblies[0].WriteMarkdownDescription(tw, true, true);
            }
        }

    }
}
