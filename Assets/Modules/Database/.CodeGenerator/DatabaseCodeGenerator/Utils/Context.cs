namespace DatabaseCodeGenerator.Utils
{
    public class Context
    {
        public Context(Schema.DatabaseSchema schema)
        {
            Schema = schema;
        }

        public Schema.DatabaseSchema Schema { get; }
    }
}
