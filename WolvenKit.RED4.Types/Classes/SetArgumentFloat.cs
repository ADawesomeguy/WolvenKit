using static WolvenKit.RED4.Types.Enums;

namespace WolvenKit.RED4.Types
{
	[REDMeta]
	public partial class SetArgumentFloat : SetArguments
	{
		[Ordinal(1)] 
		[RED("customVar")] 
		public CFloat CustomVar
		{
			get => GetPropertyValue<CFloat>();
			set => SetPropertyValue<CFloat>(value);
		}
	}
}
