using CodeWriter.ExpressionParser;

namespace GameDatabase.Model
{
    public interface IVariableResolver
    {
        Expression<Variant> Resolve(string name);
    }

    public class ExpressionBuilder
    {
        private ExpressionContext<Variant> _context;

        public ExpressionBuilder(IVariableResolver variableResolver = null)
        {
            _context = new(null, variableResolver.Resolve);
        }

        public void AddParameter(string name, Expression<Variant> valueProvider)
        {
            _context.RegisterVariable(name, valueProvider);
        }

        public void AddParameter(string name, Expression<float> valueProvider)
        {
            throw new System.NotImplementedException();
        }

        public Expression<Variant> Build(string expression)
        {
            return new VariantExpressionParser().Compile(expression, _context, false);
        }
    }
}
