using CodeWriter.ExpressionParser;

namespace GameDatabase.Model
{
    /// <summary>
    /// Interface providing default no-op implementations for IVariableResolver, for use in partial classes and codegen.
    /// </summary>
    public interface IDefaultVariablesResolver: IVariableResolver
    {
        IFunction<Variant> IVariableResolver.ResolveFunction(string name)
        {
            return null;
        }

        Expression<Variant> IVariableResolver.ResolveVariable(string name)
        {
            return null;
        }
    }
}