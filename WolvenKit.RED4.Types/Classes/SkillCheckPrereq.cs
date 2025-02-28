using static WolvenKit.RED4.Types.Enums;

namespace WolvenKit.RED4.Types
{
	[REDMeta]
	public partial class SkillCheckPrereq : DevelopmentCheckPrereq
	{
		[Ordinal(1)] 
		[RED("skillToCheck")] 
		public CEnum<gamedataProficiencyType> SkillToCheck
		{
			get => GetPropertyValue<CEnum<gamedataProficiencyType>>();
			set => SetPropertyValue<CEnum<gamedataProficiencyType>>(value);
		}
	}
}
