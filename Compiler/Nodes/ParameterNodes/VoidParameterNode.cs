namespace Compiler.Nodes
{
    /// <summary>
    /// A node corresponding to a void parameter
    /// </summary>
    public class VoidParameterNode : IParameterNode
    {
        /// <summary>
        /// The position in the code where the content associated with the node begins
        /// </summary>
        public Position Position { get; }

        public SimpleTypeDeclarationNode Type { get; set; }

        /// <summary>
        /// Creates a new void parameter node
        /// </summary>
        /// <param name="position">The position in the code where the content associated with the node begins</param>
        public VoidParameterNode(Position position)
        {
            Position = position;
        }
    }
}
