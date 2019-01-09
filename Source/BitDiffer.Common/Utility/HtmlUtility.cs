using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using BitDiffer.Common.Misc;

namespace BitDiffer.Common.Utility
{
	public class HtmlUtility
	{
		public static void WriteHtmlStart(TextWriter tw)
		{
			tw.Write("<html><head>");
			tw.Write("<meta http-equiv='X-UA-Compatible' content='IE=edge' />"); // use as modern an HTML engine as possible
			EmbedSideBySideStylesheet(tw);
			tw.Write("</head><body>");
		}

		public static void EmbedClassicStylesheet(TextWriter tw)
		{
			// TODO make this an embedded resource .css file so it's easy to edit.
			tw.WriteLine("<style type='text/css'>");
			tw.WriteLine("h1 { font-size: inherit; font-weight:bold; background-color:#B5D2FF; padding: 5px 2px; }");
			tw.WriteLine("h1::before { content: 'Assembly \\'' }");
			tw.WriteLine("h1::after { content: '\\'' }");
			tw.WriteLine("h2 { font-size: inherit; font-weight:bold; background-color:#F0F0FF; padding: 5px 2px; }");
			tw.WriteLine("h3 { font-size: inherit; font-weight:bold; }");
			tw.WriteLine("h3::before { content: 'In assembly ' }");
			tw.WriteLine("h3::after { content: ':' }");

			tw.WriteLine("body { font-family:Tahoma; font-size: 9pt; }");

			tw.WriteLine(".code { font-family: Consolas, 'Courier New'; }");
			tw.WriteLine(".keyword { color:blue; }");
			tw.WriteLine(".error, .brkchg { color: red; }");
			tw.WriteLine(".usertype { color:#2B91AF; }");
			tw.WriteLine(".string { color:#A31515; }");
			tw.WriteLine(".visibility { color:blue; }");

			tw.WriteLine(".changes-found { display: none; }");
			tw.WriteLine(".change-summary::before { content: 'Changes Found : '; }");
			tw.WriteLine(".item-change { display: none; }");
			tw.WriteLine("</style>");
		}

		public static void EmbedSideBySideStylesheet(TextWriter tw)
		{
			// TODO make this an embedded resource .css file so it's easy to edit.
			tw.WriteLine("<style type='text/css'>");

			tw.WriteLine("body { font-family:Tahoma; font-size: 9pt; }");
			tw.WriteLine(".report:not(:first-child) { margin-top: 5em; }"); // in case you generate more than 1 per file

			tw.WriteLine("h1, h2 { padding: 0.5ex; }");

			tw.WriteLine("h1 { background-color:#B5D2FF; }");

			tw.WriteLine("h2 { font-size: inherit; font-weight: bold; background-color:#F0F0FF; line-height: 125%; }");
			tw.WriteLine(".item > h2 > .item-change { font-weight: normal; font-style: italic; font-size: 85%; margin-right: 1ex; }");
			tw.WriteLine(".item.added > h2 > .item-change { color: blue; }");
			tw.WriteLine(".item.removed > h2 > .item-change { color: darkred; }");
			tw.WriteLine(".item.removed.breaking > h2 > .item-change { color: red; }");
			tw.WriteLine(".item.changed > h2 > .item-change { color: green; }");
			tw.WriteLine(".item.changed.breaking > h2 > .item-change { color: red; }");
			tw.WriteLine(".item.implementation-changed > h2 > .item-change { color: #CCC; }");

			tw.WriteLine(".item.changed > .item-body > .item-entry { display: grid; grid-template-columns: 1fr 3fr; }");

			tw.WriteLine("h3 { font-size: inherit; font-weight: bold; }");
			tw.WriteLine(".item.added h3 {  display: none; }");
			tw.WriteLine(".item.removed h3 {  display: none; }");
			tw.WriteLine(".no-code { display: none; }");

			tw.WriteLine(".code { font-family: Consolas, 'Courier New'; }");
			tw.WriteLine(".keyword { color:blue; }");
			tw.WriteLine(".error { font-family: Consolas, 'Courier New'; color: red; white-space: pre; }");
			tw.WriteLine(".brkchg { color: red; }");
			tw.WriteLine(".usertype { color:#2B91AF; }");
			tw.WriteLine(".string { color:#A31515; }");
			tw.WriteLine(".visibility { color:blue; }");
			tw.WriteLine(".change-summary::before { content: 'Changes Found : '; }");
			//tw.WriteLine(".change-summary { display: none; }");
			tw.WriteLine("</style>");
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
