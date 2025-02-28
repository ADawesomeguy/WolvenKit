using static WolvenKit.RED4.Types.Enums;

namespace WolvenKit.RED4.Types
{
	[REDMeta]
	public partial class worldNavigationTileResource : resStreamedResource
	{
		[Ordinal(1)] 
		[RED("localBoundingBox")] 
		public Box LocalBoundingBox
		{
			get => GetPropertyValue<Box>();
			set => SetPropertyValue<Box>(value);
		}

		[Ordinal(2)] 
		[RED("tilesData")] 
		public CArray<worldNavigationTileData> TilesData
		{
			get => GetPropertyValue<CArray<worldNavigationTileData>>();
			set => SetPropertyValue<CArray<worldNavigationTileData>>(value);
		}

		public worldNavigationTileResource()
		{
			LocalBoundingBox = new() { Min = new(), Max = new() { X = 1.000000F, Y = 1.000000F, Z = 1.000000F, W = 1.000000F } };
			TilesData = new();
		}
	}
}
