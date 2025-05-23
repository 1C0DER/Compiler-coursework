﻿using Compiler.IO;
using Compiler.Nodes;
using Compiler.Tokenization;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using static Compiler.Tokenization.TokenType;

namespace Compiler.SyntacticAnalysis
{
    /// <summary>
    /// A recursive descent parser
    /// </summary>
    public class Parser
    {
        /// <summary>
        /// The error reporter
        /// </summary>
        public ErrorReporter Reporter { get; }

        /// <summary>
        /// The tokens to be parsed
        /// </summary>
        private List<Token> tokens;

        /// <summary>
        /// The index of the current token in tokens
        /// </summary>
        private int currentIndex;

        /// <summary>
        /// The current token
        /// </summary>
        private Token CurrentToken { get { return tokens[currentIndex]; } }

        /// <summary>
        /// Advances the current token to the next one to be parsed
        /// </summary>
        private void MoveNext()
        {
            if (currentIndex < tokens.Count - 1)
                currentIndex += 1;
        }

        /// <summary>
        /// Creates a new parser
        /// </summary>
        /// <param name="reporter">The error reporter to use</param>
        public Parser(ErrorReporter reporter)
        {
            Reporter = reporter;
        }

        /// <summary>
        /// Checks the current token is the expected kind and moves to the next token
        /// </summary>
        /// <param name="expectedType">The expected token type</param>
        private void Accept(TokenType expectedType)
        {
            if (CurrentToken.Type == expectedType)
            {
                Debugger.Write($"Accepted {CurrentToken}");
                MoveNext();
            }
        }

        /// <summary>
        /// Parses a program
        /// </summary>
        /// <param name="tokens">The tokens to parse</param>
        public ProgramNode Parse(List<Token> tokens)
        {
            this.tokens = tokens;
            ProgramNode program = ParseProgram();
            return program;
        }



        /// <summary>
        /// Parses a program
        /// </summary>
        /// <summary>
        /// Parses a program
        /// </summary>
        private ProgramNode ParseProgram()
        {
            Debugger.Write("Parsing program");
            Accept(Declare);
            Accept(Global);
            IDeclarationNode declaration = ParseDeclaration();
            Accept(Do);
            ICommandNode command = ParseCommand();
            ProgramNode program = new ProgramNode(declaration, command);
            return program;
        }


        /// <summary>
        /// Parses a command
        /// </summary>
        private ICommandNode ParseCommand()
        {
            Debugger.Write("Parsing command");
            List<ICommandNode> commands = new List<ICommandNode>();
            commands.Add(ParseSingleCommand());
            Accept(Semicolon);

            while (CurrentToken.Type == Semicolon)
            {
                commands.Add(ParseSingleCommand());
                Accept(Semicolon);
            }
            if (commands.Count == 1)
                return commands[0];
            else
                return new SequentialCommandNode(commands);
        }

        /// <summary>
        /// Parses a single command
        /// </summary>
        private ICommandNode ParseSingleCommand()
        {
            Debugger.Write("Parsing Single Command");
            switch (CurrentToken.Type)
            {
                case Identifier:
                    return ParseAssignmentOrCallCommand();
                case Begin:
                    return ParseBeginCommand();
                case Let:
                    return ParseLetCommand();
                case If:
                    return ParseIfCommand();
                case While:
                    return ParseWhileCommand();
                default:
                    // Handling unexpected token types that do not start a command
                    Reporter.ReporterError($"Unexpected token '{CurrentToken.Type}' at position {CurrentToken.Position}. Expected a command starter.");
                    return new ErrorNode(CurrentToken.Position); 
            }
        }

        /// <summary>
        /// Parses an assignment or call command
        /// </summary>
        private ICommandNode ParseAssignmentOrCallCommand()
        {
            Debugger.Write("Parsing Assignment Command or Call Command");
            Position startPosition = CurrentToken.Position;
            IdentifierNode identifier = ParseIdentifier();

            if (CurrentToken.Type == Becomes)
            {
                Debugger.Write("Parsing Assignment Command");
                Accept(Becomes);
                IExpressionNode expression = ParseExpression();
                return new AssignCommandNode(identifier, expression);
            }
            else if (CurrentToken.Type == LeftSquareBracket)
            {
                Debugger.Write("Parsing Call Command");
                Accept(LeftSquareBracket);
                IParameterNode parameter = ParseParameter();
                Accept(RightSquareBracket);
                return new CallCommandNode(identifier, parameter);
            }
            else
            {
                return new ErrorNode(startPosition);
            }
        }


        /// <summary>
        /// Parses a begin command
        /// </summary>
        private ICommandNode ParseBeginCommand()
        {
            Debugger.Write("Parsing Begin Command");
            Accept(Begin);
            ICommandNode command = ParseCommand();
            Accept(End);
            return command;
        }

        /// <summary>
        /// Parses a skip command
        /// </summary>
        /// <returns>An abstract syntax tree representing the skip command</returns>
        private ICommandNode ParseSkipCommand()
        {
            Debugger.Write("Parsing Skip Command");
            Position startPosition = CurrentToken.Position;
            return new BlankCommandNode(startPosition);
        }

        /// <summary>
        /// Parses a while command
        /// </summary>
        private ICommandNode ParseWhileCommand()
        {
            Debugger.Write("Parsing While Command");
            Position startPosition = CurrentToken.Position;

            if (CurrentToken.Type != While)
            {
                Reporter.ReporterError($"Expected 'while' at position {startPosition}, but found '{CurrentToken.Type}'.");
                return new ErrorNode(startPosition);  
            }

            Accept(While);
            IExpressionNode expression = ParseExpression();

            if (expression == null)
            {
                return new ErrorNode(CurrentToken.Position); 
            }

            if (CurrentToken.Type != Do)
            {
                Reporter.ReporterError($"Expected 'do' after 'while' condition at position {CurrentToken.Position}, but found '{CurrentToken.Type}'.");
                return new ErrorNode(CurrentToken.Position);
            }
            Accept(Do);

            ICommandNode loopBody = ParseSingleCommand();
            if (loopBody is ErrorNode)
            {
                return loopBody; 
            }

            if (CurrentToken.Type != After)
            {
                Reporter.ReporterError($"Expected 'after' at position {CurrentToken.Position}, but found '{CurrentToken.Type}'.");
                return new ErrorNode(CurrentToken.Position);  
            }
            Accept(After);

            ICommandNode afterCommand = ParseSingleCommand();
            if (afterCommand is ErrorNode)
            {
                return afterCommand;
            }

            if (CurrentToken.Type != Wend)
            {
                Reporter.ReporterError($"Expected 'wend' to close 'while' loop at position {CurrentToken.Position}, but found '{CurrentToken.Type}'.");
                return new ErrorNode(CurrentToken.Position);  
            }
            Accept(Wend);
            return new WhileCommandNode(expression, afterCommand, loopBody, startPosition);
        }

        /// <summary>
        /// Parses an if command
        /// </summary>
        private ICommandNode ParseIfCommand()
        {
            Debugger.Write("Parsing If Command");
            Position startPosition = CurrentToken.Position;

            if (CurrentToken.Type != If)
            {
                Reporter.ReporterError($"Expected 'if' at position {startPosition}, but found '{CurrentToken.Type}'.");
                return new ErrorNode(startPosition);  
            }
            Accept(If);
            IExpressionNode expression = ParseExpression();
            if (expression == null)
            {
                return new ErrorNode(CurrentToken.Position);
            }

            if (CurrentToken.Type != Colon)
            {
                Reporter.ReporterError($"Expected ':' after 'if' condition at position {CurrentToken.Position}, but found '{CurrentToken.Type}'.");
                return new ErrorNode(CurrentToken.Position);  
            }
            Accept(Colon);

            ICommandNode colonCommand1 = ParseSingleCommand();
            if (colonCommand1 is ErrorNode)
            {
                return colonCommand1; 
            }

            if (CurrentToken.Type != Colon)
            {
                Reporter.ReporterError($"Expected second ':' in 'if' command at position {CurrentToken.Position}, but found '{CurrentToken.Type}'.");
                return new ErrorNode(CurrentToken.Position);  // Error handling for missing second ':'
            }
            Accept(Colon);

            ICommandNode colonCommand2 = ParseSingleCommand();
            if (colonCommand2 is ErrorNode)
            {
                return colonCommand2; 
            }

            if (CurrentToken.Type != EndIf)
            {
                Reporter.ReporterError($"Expected 'endif' to close 'if' command at position {CurrentToken.Position}, but found '{CurrentToken.Type}'.");
                return new ErrorNode(CurrentToken.Position);  
            }
            Accept(EndIf);

            return new IfCommandNode(expression, colonCommand1, colonCommand1, startPosition);
        }

        /// <summary>
        /// Parses a let command
        /// </summary>
        private ICommandNode ParseLetCommand()
        {
            Debugger.Write("Parsing Let Command");
            Position startPosition = CurrentToken.Position;

            if (CurrentToken.Type != Let)
            {
                Reporter.ReporterError($"Expected 'let' at position {startPosition}, but found '{CurrentToken.Type}'.");
                return new ErrorNode(startPosition); 
            }
            Accept(Let);
            IDeclarationNode declaration = ParseDeclaration();

            if (CurrentToken.Type != In)
            {
                Reporter.ReporterError($"Expected 'in' after 'let' declaration at position {CurrentToken.Position}, but found '{CurrentToken.Type}'.");
                return new ErrorNode(CurrentToken.Position);  
            }
            Accept(In);

            ICommandNode command = ParseSingleCommand();
            if (command is ErrorNode)
            {
                return command; 
            }

            if (CurrentToken.Type != EndLet)
            {
                Reporter.ReporterError($"Expected 'endlet' to close 'let' command at position {CurrentToken.Position}, but found '{CurrentToken.Type}'.");
                return new ErrorNode(CurrentToken.Position);  
            }
            Accept(EndLet);
            return new LetCommandNode(declaration, command, startPosition);
        }

        /// <summary>
        /// Parses a declaration
        /// </summary>
        private IDeclarationNode ParseDeclaration()
        {
            Debugger.Write("Parsing Declaration");
            List<IDeclarationNode> declarations = new List<IDeclarationNode>();
            declarations.Add(ParseSingleDeclaration());
            Accept(Semicolon);

            while (CurrentToken.Type == Dec)
            {
                declarations.Add(ParseSingleDeclaration());
                Accept(Semicolon);
            }
            if (declarations.Count == 1)
                return declarations[0];
            else
                return new SequentialDeclarationNode(declarations);
        }

        /// <summary>
        /// Parses a single declaration
        /// </summary>
        private IDeclarationNode ParseSingleDeclaration()
        {
            Debugger.Write("Parsing Single Declaration");
            Accept(Dec);
            IdentifierNode identifier = ParseIdentifier();
            Accept(As);
            switch (CurrentToken.Type)
            {
                case Const:
                    Accept(Const);
                    Accept(Becomes);
                    IExpressionNode expression = ParseExpression();
                    return new ConstDeclarationNode(identifier, expression);

                case Var:
                    Accept(Var);
                    Accept(Colon);
                    TypeDenoterNode typeDenoter = ParseTypeDenoter();
                    Accept(Becomes);
                    IExpressionNode varExpression = ParseExpression();
                    return new VarDeclarationNode(identifier, typeDenoter, varExpression);

                default:
                    return new ErrorNode(CurrentToken.Position);
            }
        }

        /// <summary>
        /// Parses a parameter
        /// </summary>
        private IParameterNode ParseParameter()
        {
            Debugger.Write("Parsing Parameter");
            switch (CurrentToken.Type)
            {
                case Void:
                    return ParseVoidParameter(CurrentToken.Position);
                case Const:
                    return ParseConstParameter();
                case Var:
                    return ParseVarParameter();
                default:
                    return new ErrorNode(CurrentToken.Position);
            }
        }

        private IParameterNode ParseVoidParameter(Position position)
        {
            Debugger.Write("Parsing Void Parameter");
            if (CurrentToken.Type != Void)
            {
                Reporter.ReporterError($"Expected 'void' at position {position}, but found '{CurrentToken.Type}'.");
                return new ErrorNode(CurrentToken.Position);  // Error handling for missing 'void'
            }

            Accept(Void); // Accept the 'void' token
            return new VoidParameterNode(CurrentToken.Position); // Return a VoidParameterNode
        }

        private IParameterNode ParseConstParameter()
        {
            Debugger.Write("Parsing Const Parameter");
            if (CurrentToken.Type != Const)
            {
                Reporter.ReporterError($"Expected 'const' at position {CurrentToken.Position}, but found '{CurrentToken.Type}'.");
                return new ErrorNode(CurrentToken.Position);  // Error handling for missing 'const'
            }

            Accept(Const); // Accept the 'const' keyword
            IExpressionNode expression = ParseExpression();  // Parse the expression
            return new ConstParameterNode(expression);  // Return the expression as a ConstParameterNode
        }

        private IParameterNode ParseVarParameter()
        {
            Debugger.Write("Parsing Variable Parameter");
            if (CurrentToken.Type != Var)
            {
                Reporter.ReporterError($"Expected 'var' at position {CurrentToken.Position}, but found '{CurrentToken.Type}'.");
                return new ErrorNode(CurrentToken.Position);  // Error handling for missing 'var'
            }

            Accept(Var);  // Accept the 'var' keyword
            IdentifierNode identifier = ParseIdentifier();  // Parse the identifier
            return new VarParameterNode(identifier);  // Return the identifier as a VarParameterNode
        }

        /// <summary>
        /// Parses a type denoter
        /// </summary>
        /// <returns>An abstract syntax tree representing the type denoter</returns>
        private TypeDenoterNode ParseTypeDenoter()
        {
            Debugger.Write("Parsing Type Denoter");
            IdentifierNode identifier = ParseIdentifier();
            return new TypeDenoterNode(identifier);
        }


        /// <summary>
        /// Parses an expression
        /// </summary>
        private IExpressionNode ParseExpression()
        {
            Debugger.Write("Parsing Expression");
            IExpressionNode leftExpression = ParsePrimaryExpression();
            if (leftExpression == null || leftExpression is ErrorNode)
            {
                Reporter.ReporterError("Invalid expression at the start of an expression parsing.");
                return new ErrorNode(CurrentToken.Position);
            }

            while (CurrentToken.Type == Operator)
            {
                OperatorNode operation = ParseOperator();
                IExpressionNode rightExpression = ParsePrimaryExpression();
                leftExpression = new BinaryExpressionNode(leftExpression, operation, rightExpression);
            }
            return leftExpression;
        }

        /// <summary>
        /// Parses a primary expression
        /// </summary>
        private IExpressionNode ParsePrimaryExpression()
        {
            Debugger.Write("Parsing Primary Expression");
            switch (CurrentToken.Type)
            {
                case IntLiteral:
                    return ParseIntExpression();
                case CharLiteral:
                    return ParseCharExpression();
                case Identifier:
                    return ParseIdExpression();
                case Operator:
                    return ParseUnaryExpression();
                case LeftBracket:
                    return ParseBracketExpression();
                default:
                    return new ErrorNode(CurrentToken.Position);
            }
        }

        /// <summary>
        /// Parses an int expression
        /// </summary>
        /// <returns>An abstract syntax tree representing the int expression</returns>
        private IExpressionNode ParseIntExpression()
        {
            Debugger.Write("Parsing Int Expression");
            IntegerLiteralNode intLit = ParseIntegerLiteral();
            return new IntegerExpressionNode(intLit);
        }

        /// <summary>
        /// Parses a char expression
        /// </summary>
        /// <returns>An abstract syntax tree representing the char expression</returns>
        private IExpressionNode ParseCharExpression()
        {
            Debugger.Write("Parsing Char Expression");
            CharacterLiteralNode charLit = ParseCharacterLiteral();
            return new CharacterExpressionNode(charLit);
        }

        /// <summary>
        /// Parses an ID expression
        /// </summary>
        private IExpressionNode ParseIdExpression()
        {
            Debugger.Write("Parsing Call Expression or Identifier Expression");
            IdentifierNode identifier = ParseIdentifier();
            Accept(LeftSquareBracket);
            IParameterNode parameter = ParseParameter();
            Accept(RightSquareBracket);
            return new IdExpressionNode(identifier, parameter);
        }

        /// <summary>
        /// Parses a unary expresion
        /// </summary>
        /// <returns>An abstract syntax tree representing the unary expression</returns>
        private IExpressionNode ParseUnaryExpression()
        {
            Debugger.Write("Parsing Unary Expression");
            OperatorNode operation = ParseOperator();
            IExpressionNode expression = ParsePrimaryExpression();
            return new UnaryExpressionNode(operation, expression);
        }

        /// <summary>
        /// Parses a bracket expression
        /// </summary>
        /// <returns>An abstract syntax tree representing the bracket expression</returns>
        private IExpressionNode ParseBracketExpression()
        {
            Debugger.Write("Parsing Bracket Expression");
            Accept(LeftBracket);
            IExpressionNode expression = ParseExpression();
            Accept(RightBracket);
            return expression;
        }

        /// <summary>
        /// Parses an integer literal
        /// </summary>
        /// <returns>An abstract syntax tree representing the integer literal</returns>
        private IntegerLiteralNode ParseIntegerLiteral()
        {
            Debugger.Write("Parsing integer literal");
            if (CurrentToken.Type != IntLiteral)
            {
                Reporter.ReporterError($"Expected an integer literal, but found '{CurrentToken.Type}' at position {CurrentToken.Position}.");
                return null;
            }
            Token integerLiteralToken = CurrentToken;
            MoveNext();
            Accept(IntLiteral);
            return new IntegerLiteralNode(integerLiteralToken);
        }

        /// <summary>
        /// Parses a character literal
        /// </summary>
        /// <returns>An abstract syntax tree representing the character literal</returns>
        private CharacterLiteralNode ParseCharacterLiteral()
        {
            Debugger.Write("Parsing character literal");
            if (CurrentToken.Type != CharLiteral)
            {
                Reporter.ReporterError($"Expected an chracter literal, but found '{CurrentToken.Type}' at position {CurrentToken.Position}.");
                return null;
            }
            Token CharacterLiteralToken = CurrentToken;
            MoveNext();
            Accept(CharLiteral);
            return new CharacterLiteralNode(CharacterLiteralToken);
        }

        /// <summary>
        /// Parses an identifier
        /// </summary>
        /// <returns>An abstract syntax tree representing the identifier</returns>
        private IdentifierNode ParseIdentifier()
        {
            Debugger.Write("Parsing identifier");
            if (CurrentToken.Type != Identifier)
            {
                Reporter.ReporterError($"Expected identifier, but found '{CurrentToken.Type}' at position {CurrentToken.Position}.");
                return null;
            }
            Token IdentifierToken = CurrentToken;
            Accept(Identifier);
            return new IdentifierNode(IdentifierToken);
        }

        /// <summary>
        /// Parses an operator
        /// </summary>
        /// <returns>An abstract syntax tree representing the operator</returns>
        private OperatorNode ParseOperator()
        {
            Debugger.Write("Parsing operator");
            if(CurrentToken.Type != Operator)
            {
                Reporter.ReporterError($"Expected operator, but found '{CurrentToken.Type}' at position {CurrentToken.Position}.");
                return null;
            }
            Token OperatorToken = CurrentToken;
            MoveNext();
            Accept(Operator);
            return new OperatorNode(OperatorToken);
        }
    }
}