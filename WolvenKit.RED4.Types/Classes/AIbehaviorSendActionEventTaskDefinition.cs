using static WolvenKit.RED4.Types.Enums;

namespace WolvenKit.RED4.Types
{
	[REDMeta]
	public partial class AIbehaviorSendActionEventTaskDefinition : AIbehaviorTaskDefinition
	{
		[Ordinal(1)] 
		[RED("event")] 
		public CHandle<gameActionEvent> Event
		{
			get => GetPropertyValue<CHandle<gameActionEvent>>();
			set => SetPropertyValue<CHandle<gameActionEvent>>(value);
		}
	}
}
