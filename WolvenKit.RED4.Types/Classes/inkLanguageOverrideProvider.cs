using static WolvenKit.RED4.Types.Enums;

namespace WolvenKit.RED4.Types
{
	[REDMeta]
	public partial class inkLanguageOverrideProvider : inkUserData
	{
		[Ordinal(0)] 
		[RED("languageId")] 
		public CEnum<inkLanguageId> LanguageId
		{
			get => GetPropertyValue<CEnum<inkLanguageId>>();
			set => SetPropertyValue<CEnum<inkLanguageId>>(value);
		}
	}
}
