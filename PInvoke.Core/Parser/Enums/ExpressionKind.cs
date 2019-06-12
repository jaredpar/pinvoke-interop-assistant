// Copyright (c) Microsoft Corporation.  All rights reserved.


namespace PInvoke.Parser.Enums
{
    /// <summary>
    /// Kind of expression
    /// </summary>
    /// <remarks></remarks>
    public enum ExpressionKind
    {

        // Binary operation such as +,-,/ 
        // Token: Operation
        BinaryOperation,

        // '-' operation.  Left is the value
        NegativeOperation,

        // ! operation, Left is the value
        NegationOperation,

        // Token is the name of the function.  
        // Left: Value
        // Right: , if there are more arguments
        FunctionCall,

        List,

        // Token: Target Type
        // Left: Source that is being cast
        Cast,

        // Token: Value of the expression
        Leaf
    }

}
