using static WolvenKit.RED4.Types.Enums;

namespace WolvenKit.RED4.Types
{
	[REDMeta]
	public partial class graphGraphResource : CResource
	{
		[Ordinal(1)] 
		[RED("graph")] 
		public CHandle<graphGraphDefinition> Graph
		{
			get => GetPropertyValue<CHandle<graphGraphDefinition>>();
			set => SetPropertyValue<CHandle<graphGraphDefinition>>(value);
		}
	}
}
