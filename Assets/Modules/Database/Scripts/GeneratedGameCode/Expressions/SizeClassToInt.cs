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
	public class SizeClassToInt : IFunction<Variant>
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
					builder.AddVariableResolver(new EnumResolver());
					builder.AddParameter(ParamName1, GetValue);
					_expression = builder.Build(_expressionText).Invoke;
				}

				return _expression;
			}
		}


		public SizeClassToInt(string data, int minvalue, int maxvalue, IVariableResolver variableResolver)
		{
			_expressionText = data;
			_minvalue = minvalue;
			_maxvalue = maxvalue;
			_variableResolver = variableResolver;
		}

		public int Evaluate(SizeClass value)
		{
			_value = (int)value;
			return ClampResult(Expression.Invoke().AsInt);
		}

		private int ClampResult(int value)
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
				_value = arguments[0].Invoke().AsInt;
				return ClampResult(Expression.Invoke().AsInt);
			};
		}

		private IVariableResolver _variableResolver;
		private Expression<Variant> _expression;
		private string _expressionText;
		private int _minvalue;
		private int _maxvalue;

		private int _value;

		private class EnumResolver : IVariableResolver
		{
			public IFunction<Variant> ResolveFunction(string name)
			{
				return null;
			}

			public Expression<Variant> ResolveVariable(string name)
			{
				switch (name)
				{
					case "Undefined": return () => (int)SizeClass.Undefined;
					case "Frigate": return () => (int)SizeClass.Frigate;
					case "Destroyer": return () => (int)SizeClass.Destroyer;
					case "Cruiser": return () => (int)SizeClass.Cruiser;
					case "Battleship": return () => (int)SizeClass.Battleship;
					case "Titan": return () => (int)SizeClass.Titan;
					case "Starbase": return () => (int)SizeClass.Starbase;
				}

				return null;
			}
		}
	}
}
