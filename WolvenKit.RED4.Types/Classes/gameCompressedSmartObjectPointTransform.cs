using static WolvenKit.RED4.Types.Enums;

namespace WolvenKit.RED4.Types
{
	[REDMeta]
	public partial class gameCompressedSmartObjectPointTransform : RedBaseClass
	{
		[Ordinal(0)] 
		[RED("transformId")] 
		public CUInt16 TransformId
		{
			get => GetPropertyValue<CUInt16>();
			set => SetPropertyValue<CUInt16>(value);
		}
	}
}
