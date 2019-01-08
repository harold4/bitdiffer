using System;
using System.Collections.Generic;
using System.Text;

using BitDiffer.Common.Model;
using BitDiffer.Common.Misc;

namespace BitDiffer.Common.Interfaces
{
	public interface IHaveObsoleteAttribute
	{
		AttributeDetail ObsoleteAttribute
		{
			get;
			set;
		}
	}
}
