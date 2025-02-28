using static WolvenKit.RED4.Types.Enums;

namespace WolvenKit.RED4.Types
{
	[REDMeta]
	public partial class VehicleEventsTransition : VehicleTransition
	{
		[Ordinal(1)] 
		[RED("isCameraTogglePressed")] 
		public CBool IsCameraTogglePressed
		{
			get => GetPropertyValue<CBool>();
			set => SetPropertyValue<CBool>(value);
		}

		[Ordinal(2)] 
		[RED("cameraToggleHoldToResetTimeSeconds")] 
		public CFloat CameraToggleHoldToResetTimeSeconds
		{
			get => GetPropertyValue<CFloat>();
			set => SetPropertyValue<CFloat>(value);
		}
	}
}
