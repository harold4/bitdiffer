using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace BitDiffer.Common.Utility
{
	public class HtmlUtility
	{
		public static void WriteHtmlStart(TextWriter tw)
		{
			tw.Write("<html><head>");
			EmbedSideBySideStylesheet(tw);
			tw.Write("</head><body>");
		}

		public static void EmbedClassicStylesheet(TextWriter tw)
		{
			tw.Write("<style type='text/css'><!-- ");
			tw.Write("h1 { font-size: inherit; font-weight:bold; background-color:#B5D2FF; padding: 5 2 5 2; } ");
			tw.Write("h1::before { content: 'Assembly \\'' } ");
			tw.Write("h1::after { content: '\\'' } ");
			tw.Write("h2 { font-size: inherit; font-weight:bold; background-color:#F0F0FF; padding: 5 2 5 2; } ");
			tw.Write("h3 { font-size: inherit; font-weight:bold; } ");
			tw.Write("h3::before { content: 'In assembly ' } ");
			tw.Write("h3::after { content: ':' } ");

			tw.Write("body { font-family:Tahoma; font-size: 9pt; } ");

			tw.Write(".code { font-family: Consolas, 'Courier New'; color: black; } ");
			tw.Write(".keyword { color:blue; } ");
			tw.Write(".error, .brkchg { color: red; } ");
			tw.Write(".usertype { color:#2B91AF; } ");
			tw.Write(".string { color:#A31515; } ");
			tw.Write(".visibility { color:blue; } ");

			tw.Write(".changes-found { display: none; }");
			tw.Write(".change-summary::before { content: 'Changes Found : '; }");
			tw.Write("--></style>");
		}

		public static void EmbedSideBySideStylesheet(TextWriter tw)
		{
			tw.Write("<style type='text/css'><!-- ");
			tw.Write("h1, h2, h3 { font-size: inherit; font-weight: bold; line-height: 125%; } ");
			tw.Write("h1 { background-color:#B5D2FF; padding: 5 2 5 2; } ");
			tw.Write("h2 { background-color:#F0F0FF; padding: 5 2 5 2; } ");

			tw.Write("body { font-family:Tahoma; font-size: 9pt; } ");

			tw.Write(".item > h2::before { font-weight: normal; font-style: italic; font-size: 85%; margin-right: 1ex; } ");
			tw.Write(".item.added > h2::before { content: 'Added'; color: blue; } ");
			tw.Write(".item.removed > h2::before { content: 'Removed';  color: darkred; } ");
			tw.Write(".item.removed.breaking > h2::before { content: 'Removed (Breaking)';  color: red; } ");
			tw.Write(".item.changed > h2::before { content: 'Changed'; color: green; } ");
			tw.Write(".item.changed.breaking > h2::before { content: 'Changed (Breaking)';  color: red; } ");
			tw.Write(".item.implementation-changed > h2::before { content: 'Implementation changed'; color: #CCC; } ");
			
			//tw.Write(".item-body { display: flex; flex-direction: row; }");
			//tw.Write(".item-body > * { flex: 1; margin-right: 1ex; }");
			tw.Write(".item.changed .item-entry { display: grid; grid-template-columns: 1fr 3fr; }");

			tw.Write(".item.added h3 {  display: none; }");
			tw.Write(".item.removed h3 {  display: none; }");
			tw.Write(".no-code { display: none; }");

			tw.Write(".code { font-family: Consolas, 'Courier New'; color: black; } ");
			tw.Write(".keyword { color:blue; } ");
			tw.Write(".error { color: red; } ");
			tw.Write(".brkchg { color: red; } ");
			tw.Write(".usertype { color:#2B91AF; } ");
			tw.Write(".string { color:#A31515; } ");
			tw.Write(".visibility { color:blue; } ");
			tw.Write(".change-summary { display: none; }");
			tw.Write("--></style>");
		}

		public static void WriteHtmlEnd(TextWriter tw)
		{
			tw.Write("</body></html>");
		}

		public static string HtmlEncode(string text)
		{
			text = text.Replace("<", "&lt;");
			text = text.Replace(">", "&gt;");
			return text;
		}
	}
}
