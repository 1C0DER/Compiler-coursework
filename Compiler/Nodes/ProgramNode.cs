﻿namespace Compiler.Nodes
{
    /// <summary>
    /// A node corresponding to a program
    /// </summary>
    public class ProgramNode : IAbstractSyntaxTreeNode
    {
        /// <summary>
        /// The command that the program contains
        /// </summary>
        public ICommandNode Command { get; }

        public IDeclarationNode Declaration { get; }

        /// <summary>
        /// The position in the code where the content associated with the node begins
        /// </summary>
        public Position Position { get { return Command.Position; } }

        /// <summary>
        /// Creates a new program node
        /// </summary>
        /// <param name="command">The command that the program contains</param>
        /// <param name="declaration">The command that the program contains</param>
        public ProgramNode(IDeclarationNode declaration, ICommandNode command)
        {
            Command = command;
            Declaration = declaration;
        }
    }
}