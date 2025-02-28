using static WolvenKit.RED4.Types.Enums;

namespace WolvenKit.RED4.Types
{
	[REDMeta]
	public partial class IntercomControllerPS : ScriptableDeviceComponentPS
	{
		[Ordinal(104)] 
		[RED("isCalling")] 
		public CBool IsCalling
		{
			get => GetPropertyValue<CBool>();
			set => SetPropertyValue<CBool>(value);
		}

		[Ordinal(105)] 
		[RED("sceneStarted")] 
		public CBool SceneStarted
		{
			get => GetPropertyValue<CBool>();
			set => SetPropertyValue<CBool>(value);
		}

		[Ordinal(106)] 
		[RED("endingCall")] 
		public CBool EndingCall
		{
			get => GetPropertyValue<CBool>();
			set => SetPropertyValue<CBool>(value);
		}

		[Ordinal(107)] 
		[RED("forceLookAt")] 
		public entEntityID ForceLookAt
		{
			get => GetPropertyValue<entEntityID>();
			set => SetPropertyValue<entEntityID>(value);
		}

		[Ordinal(108)] 
		[RED("forceFollow")] 
		public CBool ForceFollow
		{
			get => GetPropertyValue<CBool>();
			set => SetPropertyValue<CBool>(value);
		}

		public IntercomControllerPS()
		{
			DeviceName = "LocKey#163";
			TweakDBRecord = new() { Value = 71462547290 };
			TweakDBDescriptionRecord = new() { Value = 123510029761 };
			ForceLookAt = new();
		}
	}
}
