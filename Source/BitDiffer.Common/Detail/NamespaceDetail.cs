using System;
using System.Collections.Generic;
using System.Text;
using BitDiffer.Common.Utility;

namespace BitDiffer.Common.Model
{
	[Serializable]
	public class NamespaceDetail : ParentDetail
	{
		public NamespaceDetail()
		{
		}

		public NamespaceDetail(RootDetail parent, string name)
			: base(parent, name)
		{
		}

		protected override bool FullNameRoot
		{
			get { return true; }
		}

		protected override bool SerializeShouldWriteName()
		{
			return true;
		}

		public override bool ExcludeFromReport
		{
			get { return true; }
		}

		protected override bool SuppressBreakingChangesInChildren
		{
			get { return false; }
		}

		public virtual bool ExcludeChildrenFromReport
		{
			get { return this.CollapseChildren; }// || ChangeTypeUtil.IsNonBreakingAddRemove(CombineAllChangesThisInstanceGoingForward()); }
		}


		public override string GetTextTitle()
		{
			return "namespace " + _name;
		}

		protected override string SerializeGetElementName()
		{
			return "Namespace";
		}
	}
}
