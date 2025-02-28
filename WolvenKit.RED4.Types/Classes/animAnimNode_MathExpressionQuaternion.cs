using static WolvenKit.RED4.Types.Enums;

namespace WolvenKit.RED4.Types
{
	[REDMeta]
	public partial class animAnimNode_MathExpressionQuaternion : animAnimNode_QuaternionValue
	{
		[Ordinal(11)] 
		[RED("expressionData")] 
		public animMathExpressionNodeData ExpressionData
		{
			get => GetPropertyValue<animMathExpressionNodeData>();
			set => SetPropertyValue<animMathExpressionNodeData>(value);
		}

		public animAnimNode_MathExpressionQuaternion()
		{
			Id = 4294967295;
			ExpressionData = new() { FloatSockets = new(), VectorSockets = new(), QuaternionSockets = new() };
		}
	}
}
