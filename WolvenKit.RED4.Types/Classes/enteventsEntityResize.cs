using static WolvenKit.RED4.Types.Enums;

namespace WolvenKit.RED4.Types
{
	[REDMeta]
	public partial class enteventsEntityResize : redEvent
	{
		[Ordinal(0)] 
		[RED("extents")] 
		public Vector3 Extents
		{
			get => GetPropertyValue<Vector3>();
			set => SetPropertyValue<Vector3>(value);
		}

		public enteventsEntityResize()
		{
			Extents = new();
		}
	}
}
