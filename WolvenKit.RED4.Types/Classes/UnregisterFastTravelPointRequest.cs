using static WolvenKit.RED4.Types.Enums;

namespace WolvenKit.RED4.Types
{
	[REDMeta]
	public partial class UnregisterFastTravelPointRequest : gameScriptableSystemRequest
	{
		[Ordinal(0)] 
		[RED("pointData")] 
		public CHandle<gameFastTravelPointData> PointData
		{
			get => GetPropertyValue<CHandle<gameFastTravelPointData>>();
			set => SetPropertyValue<CHandle<gameFastTravelPointData>>(value);
		}

		[Ordinal(1)] 
		[RED("requesterID")] 
		public entEntityID RequesterID
		{
			get => GetPropertyValue<entEntityID>();
			set => SetPropertyValue<entEntityID>(value);
		}

		public UnregisterFastTravelPointRequest()
		{
			RequesterID = new();
		}
	}
}
