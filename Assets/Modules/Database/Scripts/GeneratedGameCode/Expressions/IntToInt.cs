//-------------------------------------------------------------------------------
//                                                                               
//    This code was automatically generated.                                     
//    Changes to this file may cause incorrect behavior and will be lost if      
//    the code is regenerated.                                                   
//                                                                               
//-------------------------------------------------------------------------------

using GameDatabase.Enums;
using GameDatabase.Model;
using CodeWriter.ExpressionParser;

namespace GameDatabase.Expressions
{
	public class IntToInt
	{
		public string ParamName1 { get; set; } = "value";

		public IntToInt(string data, int minvalue, int maxvalue, IVariableResolver variableResolver)
		{
			_expressionText = data;
			_minvalue = minvalue;
			_maxvalue = maxvalue;
			_variableResolver = variableResolver;
		}

		public int Evaluate(int value)
		{
			if (_expression == null) Build();
			_value = value;
			return ClampResult(_expression.Invoke().AsInt);
		}

		private void Build()
		{
			var builder = new ExpressionBuilder(_variableResolver);
			builder.AddParameter(ParamName1, GetValue);
			_expression = builder.Build(_expressionText).Invoke;
		}

		private int ClampResult(int value)
        {
			if (value < _minvalue) return _minvalue;
			if (value > _maxvalue) return _maxvalue;
			return value;
        }

		private Variant GetValue() => _value;

		private IVariableResolver _variableResolver;
		private Expression<Variant> _expression;
		private string _expressionText;
		private int _minvalue;
		private int _maxvalue;

		private int _value;
	}
}
