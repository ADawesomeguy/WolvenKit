using System.IO;
using System.Runtime.Serialization;
using WolvenKit.RED3.CR2W.Reflection;
using FastMember;
using static WolvenKit.RED3.CR2W.Types.Enums;


namespace WolvenKit.RED3.CR2W.Types
{
	[DataContract(Namespace = "")]
	[REDMeta]
	public class CBehTreeNodeFleeReactionDefinition : CBehTreeNodeAtomicMoveToDefinition
	{
		[Ordinal(1)] [RED("fleeDistance")] 		public CBehTreeValFloat FleeDistance { get; set;}

		[Ordinal(2)] [RED("surrenderDistance")] 		public CBehTreeValFloat SurrenderDistance { get; set;}

		[Ordinal(3)] [RED("queryRadiusRatio")] 		public CFloat QueryRadiusRatio { get; set;}

		[Ordinal(4)] [RED("useCombatTarget")] 		public CBool UseCombatTarget { get; set;}

		public CBehTreeNodeFleeReactionDefinition(IRed3EngineFile cr2w, CVariable parent, string name) : base(cr2w, parent, name){ }

		

		public override void Read(BinaryReader file, uint size) => base.Read(file, size);

		public override void Write(BinaryWriter file) => base.Write(file);

	}
}