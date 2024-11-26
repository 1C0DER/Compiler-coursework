using Compiler.CodeGeneration;

namespace Compiler.Nodes
{
    internal class RuntimeKnownConstant : IRuntimeEntity
    {
        private short value;

        public RuntimeKnownConstant(short value)
        {
            this.value = value;
        }
    }
}