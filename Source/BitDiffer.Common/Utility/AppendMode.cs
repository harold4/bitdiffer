using System;
using System.Collections.Generic;
using System.Text;

namespace BitDiffer.Common.Utility
{
    [Flags]
    public enum AppendMode
    {
        Text = 1,
        Html = 2,
        Markdown = 4,

        NonText = AppendMode.Html | AppendMode.Markdown,
        All = AppendMode.Text | AppendMode.Html | AppendMode.Markdown
    };
}
