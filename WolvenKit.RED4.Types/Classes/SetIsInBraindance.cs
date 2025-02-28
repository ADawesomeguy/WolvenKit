using static WolvenKit.RED4.Types.Enums;

namespace WolvenKit.RED4.Types
{
	[REDMeta]
	public partial class SetIsInBraindance : gameScriptableSystemRequest
	{
		[Ordinal(0)] 
		[RED("newState")] 
		public CBool NewState
		{
			get => GetPropertyValue<CBool>();
			set => SetPropertyValue<CBool>(value);
		}
	}
}
