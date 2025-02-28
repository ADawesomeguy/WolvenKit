using static WolvenKit.RED4.Types.Enums;

namespace WolvenKit.RED4.Types
{
	[REDMeta]
	public partial class StatPoolPrereqListener : BaseStatPoolPrereqListener
	{
		[Ordinal(0)] 
		[RED("state")] 
		public CWeakHandle<StatPoolPrereqState> State
		{
			get => GetPropertyValue<CWeakHandle<StatPoolPrereqState>>();
			set => SetPropertyValue<CWeakHandle<StatPoolPrereqState>>(value);
		}
	}
}
