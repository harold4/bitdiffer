using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace BitDiffer.Common.Exceptions
{
    public static class ExceptionExtension
    {
	    private static StringBuilder AppendIndent(this StringBuilder sb, int depth)
	    {
		    if (depth == 0)
			    return sb;
		    return sb.Append(' ', 4 * depth);
	    }

		private static void AppendNestedExceptionMessage(StringBuilder sb, Exception ex, int depth = 0)
		{
			while (ex != null)
			{
				if (depth > 0)
				{
					sb.AppendLine();
					sb.AppendIndent(depth);
					sb.Append(" -> ");
				}

				sb.Append(ex.Message);

				var typeLoadException = ex as ReflectionTypeLoadException;
				if (typeLoadException != null)
				{
					var loaderExceptions = typeLoadException.LoaderExceptions;
					sb.AppendLine()
						.AppendIndent(depth)
						.AppendFormat("LoaderExceptions ({0})", loaderExceptions.Length);
					foreach (Exception loaderException in typeLoadException.LoaderExceptions)
					{
						AppendNestedExceptionMessage(sb, loaderException, depth + 1);
					}
				}
				else
				{
					var exFileNotFound = ex as FileNotFoundException;
					if (exFileNotFound != null)
					{
						sb.AppendLine()
							.AppendIndent(depth)
							.AppendFormat("File:{0} FusionLog:{1} Message:{2}", exFileNotFound.FileName, exFileNotFound.FusionLog, exFileNotFound.Message);
					}
				}

				ex = ex.InnerException;
			}
		}

		public static string GetNestedExceptionMessage(this Exception ex)
        {
            var sb = new StringBuilder();
			AppendNestedExceptionMessage(sb, ex);
	        return sb.ToString();
        }
    }
}
