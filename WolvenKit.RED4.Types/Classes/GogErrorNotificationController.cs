using static WolvenKit.RED4.Types.Enums;

namespace WolvenKit.RED4.Types
{
	[REDMeta]
	public partial class GogErrorNotificationController : inkWidgetLogicController
	{
		[Ordinal(1)] 
		[RED("errorMessageWidget")] 
		public inkWidgetReference ErrorMessageWidget
		{
			get => GetPropertyValue<inkWidgetReference>();
			set => SetPropertyValue<inkWidgetReference>(value);
		}

		public GogErrorNotificationController()
		{
			ErrorMessageWidget = new();
		}
	}
}
