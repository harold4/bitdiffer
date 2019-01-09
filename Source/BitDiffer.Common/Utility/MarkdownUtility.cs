using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BitDiffer.Common.Utility
{
	public static class MarkdownUtility
	{

		/// <summary>
		/// Escape sequence for markdown inline code.
		/// </summary>
		/// <remarks>
		/// Use double backticks in case the text has a backtick (likelihood of one is low, two or more in sequence is vanishingly small).
		/// </remarks>
		private const string MarkdownInlineCode = "``";

		/// <summary>
		/// Encodes the specified value as inline code in Markdown (e.g. <c>``foo``</c>).
		/// </summary>
		/// <param name="value"></param>
		/// <returns></returns>
		public static string ToInlineCode(string value)
		{
			return MarkdownInlineCode + value + MarkdownInlineCode;
		}
	}
}
