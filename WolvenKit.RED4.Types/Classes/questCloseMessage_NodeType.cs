using static WolvenKit.RED4.Types.Enums;

namespace WolvenKit.RED4.Types
{
	[REDMeta]
	public partial class questCloseMessage_NodeType : questIPhoneManagerNodeType
	{
		[Ordinal(0)] 
		[RED("msg")] 
		public CHandle<gameJournalPath> Msg
		{
			get => GetPropertyValue<CHandle<gameJournalPath>>();
			set => SetPropertyValue<CHandle<gameJournalPath>>(value);
		}
	}
}
