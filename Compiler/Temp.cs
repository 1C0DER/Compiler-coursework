using System.Collections.Immutable;

// What's this file all about?
//
// There are a few references in some of the Nodes to a future oart of the compiler - the code generator.
// If I deleted these, I would have to give you lots of replacement Node classes in a few weeks time.
// Instead, I have created this dummy version of the missing bits so that everything will compile and run.
// I'll give you the real code generator to replace this file in a couple of weeks time.

namespace Compiler.CodeGeneration
{
    public interface IRuntimeEntity { }
    public class TriangleAbstractMachine
    {
        public enum Primitive { }
        public enum Type { }
        public static ImmutableDictionary<Type, byte> TypeSize { get; }
    }
}