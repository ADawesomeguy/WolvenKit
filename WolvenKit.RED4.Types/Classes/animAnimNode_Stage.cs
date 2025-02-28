using static WolvenKit.RED4.Types.Enums;

namespace WolvenKit.RED4.Types
{
	[REDMeta]
	public partial class animAnimNode_Stage : animAnimNode_Container
	{
		[Ordinal(12)] 
		[RED("inputPoses")] 
		public CArray<animPoseLink> InputPoses
		{
			get => GetPropertyValue<CArray<animPoseLink>>();
			set => SetPropertyValue<CArray<animPoseLink>>(value);
		}

		public animAnimNode_Stage()
		{
			Id = 4294967295;
			Nodes = new();
			InputPoses = new();
		}
	}
}
