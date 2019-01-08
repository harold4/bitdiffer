using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using BitDiffer.Common.Interfaces;
using BitDiffer.Common.Misc;
using BitDiffer.Common.Model;

namespace BitDiffer.Common.Utility
{
	public static class ObsoleteUtil
	{
		public static string GetObsoleteString(AttributeDetail obsoleteAttribute)
		{
			if (obsoleteAttribute == null)
			{
				return "not obsolete";
			}

			return obsoleteAttribute.GetTextDeclaration();
		}

		public static ChangeType GetObsoleteChange(AttributeDetail from, AttributeDetail to, bool suppressBreakingChanges)
		{
			if (Equals(from, to))
			{
				return ChangeType.None;
			}

			if (to == null)
			{
				// Removing Obsolete will not break code
				return ChangeType.ObsoleteChangedNonBreaking;
			}

			// ObsoleteWithError is a breaking change in the sense that source cannot compile.
			// However, Obsolete(false) MAY also be a breaking change if the caller uses warn-as-error.
			// TODO Consider adding an option to state what obsolete levels are considered breaking.

			// FIXME
			//if ((!suppressBreakingChanges) && to.C)
			//{
			//	return ChangeType.ObsoleteChangedBreaking;
			//}
			//else
			//{
				return ChangeType.ObsoleteChangedNonBreaking;
			//}
		}

		public static string GetObsoleteChangeText(RootDetail from, RootDetail to)
		{
			return GetObsoleteChangeText(((IHaveObsoleteAttribute)from).ObsoleteAttribute, ((IHaveObsoleteAttribute)to).ObsoleteAttribute);
		}

		public static string GetObsoleteChangeText(IHaveObsoleteAttribute from, IHaveObsoleteAttribute to)
		{
			return GetObsoleteChangeText(from.ObsoleteAttribute, to.ObsoleteAttribute);
		}

		public static string GetObsoleteChangeText(AttributeDetail from, AttributeDetail to)
		{
			// When collapsing properties the ObsoleteStatus of the property itself (as opposed to the child accessors) may actually
			// be the same....
			if (Equals(from, to))
			{
				return "Obsolete status not changed";
			}

			return $"Obsolete status was changed from '{GetObsoleteString(from)}' to '{GetObsoleteString(to)}'";
		}
	}
}
