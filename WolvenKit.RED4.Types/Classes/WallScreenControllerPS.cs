using static WolvenKit.RED4.Types.Enums;

namespace WolvenKit.RED4.Types
{
	[REDMeta]
	public partial class WallScreenControllerPS : TVControllerPS
	{
		[Ordinal(114)] 
		[RED("isShown")] 
		public CBool IsShown
		{
			get => GetPropertyValue<CBool>();
			set => SetPropertyValue<CBool>(value);
		}

		public WallScreenControllerPS()
		{
			TweakDBRecord = new() { Value = 78369534372 };
			TweakDBDescriptionRecord = new() { Value = 131860853415 };
		}
	}
}
