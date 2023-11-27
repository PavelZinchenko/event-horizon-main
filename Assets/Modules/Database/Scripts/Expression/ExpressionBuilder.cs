using CodeWriter.ExpressionParser;

namespace GameDatabase.Model
{
    public interface IVariableResolver
    {
        IFunction<Variant> ResolveFunction(string name);
        Expression<Variant> ResolveVariable(string name);
    }

    public interface Expression
    {
        int ArgumentCount { get; }
        Variant Invoke(Variant[] arguments);
    }

    public class ExpressionBuilder
    {
        private ExpressionContext<Variant> _context;

        public ExpressionBuilder(IVariableResolver variableResolver = null)
        {
            _context = new(null, variableResolver.ResolveVariable, variableResolver.ResolveFunction);
        }

        public void AddParameter(string name, Expression<Variant> valueProvider)
        {
            _context.RegisterVariable(name, valueProvider);
        }

        public Expression<Variant> Build(string expression)
        {
            return new VariantExpressionParser().Compile(expression, _context, false);
        }
    }
}
