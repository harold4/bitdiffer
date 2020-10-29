using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using BitDiffer.Common.Model;
using BitDiffer.Common.Misc;
using BitDiffer.Common.Interfaces;

namespace BitDiffer.Common.Utility
{
    public static class ChangeTypeUtil
    {
        private static ChangeType breakingChangesMask = ChangeType.DeclarationChangedBreaking | ChangeType.MembersChangedBreaking | ChangeType.RemovedBreaking | ChangeType.VisibilityChangedBreaking | ChangeType.ValueChangedBreaking;
        private static ChangeType nonBreakingChangesMask = ~breakingChangesMask;
        private static ChangeType childChangesMask = ChangeType.AttributesChanged | ChangeType.MembersChangedBreaking | ChangeType.MembersChangedNonBreaking | ChangeType.ContentChanged;

        public static bool IsAddRemove(ChangeType change)
        {
            if (change == ChangeType.Added || change == ChangeType.RemovedBreaking || change == ChangeType.RemovedNonBreaking)
            {
                return true;
            }

            return false;
        }

        public static bool IsNonBreakingAddRemove(ChangeType change)
        {
            if (change == ChangeType.Added || change == ChangeType.RemovedNonBreaking)
            {
                return true;
            }

            return false;
        }

        public static bool HasBreaking(ChangeType change)
        {
            return (change != ChangeType.None) && ((change & breakingChangesMask) != 0);
        }

        public static bool HasNonBreaking(ChangeType change)
        {
            return (change != ChangeType.None) && ((change & nonBreakingChangesMask) != 0);
        }

        public static bool IsChildChangeOnly(ChangeType change)
        {
            return (change != ChangeType.None) && ((change & childChangesMask) == change);
        }

        private static readonly ChangeType[] _allChangeTypes = Enum.GetValues(typeof(ChangeType)).Cast<ChangeType>().ToArray();

        public static IEnumerable<ChangeType> EnumerateFlags(ChangeType change)
        {
            foreach (var flag in _allChangeTypes)
                if ((flag & change) != 0)
                    yield return flag;
        }

        public static string GetChangeClass(ChangeType change)
        {
            switch (change)
            {
                case ChangeType.None:
                    return null;
                case ChangeType.Added:
                    return "added";
                case ChangeType.RemovedNonBreaking:
                    return "removed";
                case ChangeType.RemovedBreaking:
                    return "removed breaking";
                case ChangeType.ImplementationChanged:
                    return "implementation-changed";
                case ChangeType.ContentChanged:
                    return "implementation-changed";
                default:
                    // Divide the remaining cases into breaking and non-breaking
                    return ChangeTypeUtil.HasBreaking(change) ? "changed breaking" : "changed";
            }
        }

        /// <summary>
        /// Brief text (added/removed/changed)
        /// </summary>
        /// <param name="change"></param>
        /// <returns></returns>
        public static string GetSummaryText(ChangeType change)
        {
            switch (change)
            {
                case ChangeType.None:
                    return null;
                case ChangeType.Added:
                    return "Added";
                case ChangeType.RemovedNonBreaking:
                    return "Removed";
                case ChangeType.RemovedBreaking:
                    return "Removed (Breaking)";
                case ChangeType.ImplementationChanged:
                    return "Implementation changed";
                case ChangeType.ContentChanged:
                    return "Content changed";
                default:
                    // Divide the remaining cases into breaking and non-breaking
                    return ChangeTypeUtil.HasBreaking(change) ? "Changed (Breaking)" : "Changed";
            }
        }
    }
}
