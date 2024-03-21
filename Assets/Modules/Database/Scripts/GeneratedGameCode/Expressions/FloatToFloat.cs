//-------------------------------------------------------------------------------
//                                                                               
//    This code was automatically generated.                                     
//    Changes to this file may cause incorrect behavior and will be lost if      
//    the code is regenerated.                                                   
//                                                                               
//-------------------------------------------------------------------------------

using System.Collections.Generic;
using GameDatabase.Enums;
using GameDatabase.Model;
using CodeWriter.ExpressionParser;

namespace GameDatabase.Expressions
{
	public class FloatToFloat : IFunction<Variant>
	{
		public string ParamName1 { get; set; } = "value";

		public int ArgumentCount => 1;

		private Expression<Variant> Expression
		{
			get
			{
				if (_expression == null)
				{
					var builder = new ExpressionBuilder();
					builder.AddVariableResolver(_variableResolver);
					builder.AddParameter(ParamName1, GetValue);
					_expression = builder.Build(_expressionText).Invoke;
				}

				return _expression;
			}
		}


		public FloatToFloat(string data, float minvalue, float maxvalue, IVariableResolver variableResolver)
		{
			_expressionText = data;
			_minvalue = minvalue;
			_maxvalue = maxvalue;
			_variableResolver = variableResolver;
		}

		public float Evaluate(float value)
		{
			_value = (float)value;
			return ClampResult(Expression.Invoke().AsSingle);
		}

		private float ClampResult(float value)
		{
			if (value < _minvalue) return _minvalue;
			if (value > _maxvalue) return _maxvalue;
			return value;
		}

		private Variant GetValue() => _value;

		public Expression<Variant> Invoke(List<Expression<Variant>> arguments)
		{
			if (arguments.Count != ArgumentCount) 
				throw new System.ArgumentException();

			return () =>
			{
				_value = arguments[0].Invoke().AsSingle;
				return ClampResult(Expression.Invoke().AsSingle);
			};
		}

		private IVariableResolver _variableResolver;
		private Expression<Variant> _expression;
		private string _expressionText;
		private float _minvalue;
		private float _maxvalue;

		private float _value;
	}
}
