using static WolvenKit.RED4.Types.Enums;

namespace WolvenKit.RED4.Types
{
	[REDMeta]
	public partial class SensorJammed : redEvent
	{
		[Ordinal(0)] 
		[RED("sensor")] 
		public CWeakHandle<SensorDevice> Sensor
		{
			get => GetPropertyValue<CWeakHandle<SensorDevice>>();
			set => SetPropertyValue<CWeakHandle<SensorDevice>>(value);
		}
	}
}
