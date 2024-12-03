namespace Compiler.Nodes
{
    /// <summary>
    /// A node corresponding to an if command
    /// </summary>
    public class DoCommandNode : ICommandNode
    {

        /// <summary>
        /// The then branch command
        /// </summary>
        public ICommandNode Command { get; }


        /// <summary>
        /// The position in the code where the content associated with the node begins
        /// </summary>
        public Position Position { get; }

        /// <summary>
        /// Creates a new if command node
        /// </summary>
        /// <param name="command">The else branch command</param>
        /// <param name="position">The position in the code where the content associated with the node begins</param>
        public DoCommandNode(ICommandNode command, Position position)
        {
            Command = command;
            Position = position;
        }
    }
}