using static WolvenKit.RED4.Types.Enums;

namespace WolvenKit.RED4.Types
{
	[REDMeta]
	public partial class DisableBraindanceActions : redEvent
	{
		[Ordinal(0)] 
		[RED("actionMask")] 
		public SBraindanceInputMask ActionMask
		{
			get => GetPropertyValue<SBraindanceInputMask>();
			set => SetPropertyValue<SBraindanceInputMask>(value);
		}

		public DisableBraindanceActions()
		{
			ActionMask = new();
		}
	}
}
