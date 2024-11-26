namespace Compiler.Nodes
{
    /// <summary>
    /// A node corresponding to a var parameter
    /// </summary>
    public class ConstParameterNode : IParameterNode
    {
        /// <summary>
        /// The identifier associated with the parameter
        /// </summary>
        public IExpressionNode Expression { get; }

        /// <summary>
        /// The type of the parameter
        /// </summary>
        public SimpleTypeDeclarationNode Type { get; set; }

        /// <summary>
        /// The position in the code where the content associated with the node begins
        /// </summary>
        public Position Position { get; }

        /// <summary>
        /// Creates a new var parameter node
        /// </summary>
        /// <param name="expression">The identifier associated with the parameter</param>
        /// <param name="position">The position in the code where the content associated with the node begins</param>
        public ConstParameterNode(IExpressionNode expression, Position position)
        {
            Expression = expression;
            Position = position;
        }

        public ConstParameterNode(IExpressionNode expression)
        {
            Expression = expression;
        }
    }
}