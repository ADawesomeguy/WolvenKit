using static WolvenKit.RED4.Types.Enums;

namespace WolvenKit.RED4.Types
{
	[REDMeta]
	public partial class AnimationsLoadedCondition : AIbehaviorconditionScript
	{
		[Ordinal(0)] 
		[RED("coreAnims")] 
		public CBool CoreAnims
		{
			get => GetPropertyValue<CBool>();
			set => SetPropertyValue<CBool>(value);
		}

		[Ordinal(1)] 
		[RED("melee")] 
		public CBool Melee
		{
			get => GetPropertyValue<CBool>();
			set => SetPropertyValue<CBool>(value);
		}
	}
}
