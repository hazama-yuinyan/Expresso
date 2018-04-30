using System;
using Expresso.Formatting;


namespace Expresso.Ast
{
    /// <summary>
    /// The formatting options factory creates pre-defined formatting option styles.
    /// </summary>
    public static class FormattingOptionsFactory
    {
        public static ExpressoFormattingOptions CreateEmpty()
        {
            return new ExpressoFormattingOptions();
        }

        /// <summary>
        /// Creates mono indent style ExpressoFormatting options.
        /// </summary>
        public static ExpressoFormattingOptions CreateMono()
        {
            return new ExpressoFormattingOptions{
                IndentClassBody = true,
                IndentInterfaceBody = true,
                IndentEnumBody = true,
                IndentMethodBody = true,
                IndentBlocks = true,
                IndentMatchBody = false,
                IndentMatchPatternBody = true,
                IndentBreakStatements = true,
                IndentBlocksInsideExpressions = false,
                ClassBraceStyle = BraceStyle.NextLine,
                InterfaceBraceStyle = BraceStyle.NextLine,
                EnumBraceStyle = BraceStyle.NextLine,
                MethodBraceStyle = BraceStyle.NextLine,
                AnonymousMethodBraceStyle = BraceStyle.EndOfLine,

                StatementBraceStyle = BraceStyle.EndOfLine,

                ElseNewLinePlacement = NewLinePlacement.SameLine,
                ElseIfNewLinePlacement = NewLinePlacement.SameLine,
                WhileNewLinePlacement = NewLinePlacement.SameLine,
                ArrayInitializerWrapping = Wrapping.WrapIfTooLong,
                ArrayInitializerBraceStyle = BraceStyle.EndOfLine,
                AllowOneLinedArrayInitialziers = true,

                SpaceBeforeMethodCallParentheses = true,
                SpaceBeforeMethodDeclarationParentheses = true,
                SpaceBeforeConstructorDeclarationParentheses = true,
                SpaceBeforeDelegateDeclarationParentheses = true,
                SpaceAfterMethodCallParameterComma = true,
                SpaceAfterConstructorDeclarationParameterComma = true,

                SpaceBeforeNewParentheses = true,
                SpacesWithinNewParentheses = false,
                SpacesBetweenEmptyNewParentheses = false,
                SpaceBeforeNewParameterComma = false,
                SpaceAfterNewParameterComma = true,

                SpaceBeforeIfParentheses = true,
                SpaceBeforeWhileParentheses = true,
                SpaceBeforeForParentheses = true,
                SpaceBeforeForeachParentheses = true,
                SpaceBeforeCatchParentheses = true,
                SpaceBeforeSwitchParentheses = true,
                SpaceBeforeLockParentheses = true,
                SpaceBeforeUsingParentheses = true,
                SpaceAroundAssignment = true,
                SpaceAroundLogicalOperator = true,
                SpaceAroundEqualityOperator = true,
                SpaceAroundRelationalOperator = true,
                SpaceAroundBitwiseOperator = true,
                SpaceAroundAdditiveOperator = true,
                SpaceAroundMultiplicativeOperator = true,
                SpaceAroundShiftOperator = true,
                SpaceAroundNullCoalescingOperator = true,
                SpacesWithinParentheses = false,
                SpaceWithinMethodCallParentheses = false,
                SpaceWithinMethodDeclarationParentheses = false,
                SpacesWithinIfParentheses = false,
                SpacesWithinWhileParentheses = false,
                SpacesWithinForParentheses = false,
                SpacesWithinForeachParentheses = false,
                SpacesWithinCatchParentheses = false,
                SpacesWithinSwitchParentheses = false,
                SpacesWithinLockParentheses = false,
                SpacesWithinUsingParentheses = false,
                SpacesWithinCastParentheses = false,
                SpacesWithinSizeOfParentheses = false,
                SpacesWithinTypeOfParentheses = false,
                SpacesWithinCheckedExpressionParantheses = false,
                SpaceBeforeConditionalOperatorCondition = true,
                SpaceAfterConditionalOperatorCondition = true,
                SpaceBeforeConditionalOperatorSeparator = true,
                SpaceAfterConditionalOperatorSeparator = true,

                SpacesWithinBrackets = false,
                SpacesBeforeBrackets = true,
                SpaceBeforeBracketComma = false,
                SpaceAfterBracketComma = true,

                SpaceBeforeForSemicolon = false,
                SpaceAfterForSemicolon = true,
                SpaceAfterTypecast = false,

                AlignEmbeddedStatements = true,
                SpaceBeforeMethodDeclarationParameterComma = false,
                SpaceAfterMethodDeclarationParameterComma = true,
                SpaceAfterDelegateDeclarationParameterComma = true,
                SpaceBeforeFieldDeclarationComma = false,
                SpaceAfterFieldDeclarationComma = true,
                SpaceBeforeLocalVariableDeclarationComma = false,
                SpaceAfterLocalVariableDeclarationComma = true,

                SpaceBeforeIndexerDeclarationBracket = true,
                SpaceWithinIndexerDeclarationBracket = false,
                SpaceBeforeIndexerDeclarationParameterComma = false,
                SpaceInNamedArgumentAfterDoubleColon = true,
                RemoveEndOfLineWhiteSpace = true,

                SpaceAfterIndexerDeclarationParameterComma = true,

                MinimumBlankLinesBeforeUsings = 0,
                MinimumBlankLinesAfterUsings = 1,

                MinimumBlankLinesBeforeFirstDeclaration = 0,
                MinimumBlankLinesBetweenTypes = 1,
                MinimumBlankLinesBetweenFields = 0,
                MinimumBlankLinesBetweenEventFields = 0,
                MinimumBlankLinesBetweenMembers = 1,
                MinimumBlankLinesAroundRegion = 1,
                MinimumBlankLinesInsideRegion = 1,
                AlignToFirstIndexerArgument = false,
                AlignToFirstIndexerDeclarationParameter = true,
                AlignToFirstMethodCallArgument = false,
                AlignToFirstMethodDeclarationParameter = true,
                KeepCommentsAtFirstColumn = true,
                ChainedMethodCallWrapping = Wrapping.DoNotModify,
                MethodCallArgumentWrapping = Wrapping.DoNotModify,
                NewLineAferMethodCallOpenParentheses = NewLinePlacement.DoNotCare,
                MethodCallClosingParenthesesOnNewLine = NewLinePlacement.DoNotCare,

                IndexerArgumentWrapping = Wrapping.DoNotModify,
                NewLineAferIndexerOpenBracket = NewLinePlacement.DoNotCare,
                IndexerClosingBracketOnNewLine = NewLinePlacement.DoNotCare,

                NewLineBeforeNewQueryClause = NewLinePlacement.NewLine
            };
        }

        /// <summary>
        /// The K&amp;R style, so named because it was used in Kernighan and Ritchie's book The C Programming Language,
        /// is commonly used in C. It is less common for C++, C#, and others.
        /// </summary>
        public static ExpressoFormattingOptions CreateKRStyle()
        {
            return new ExpressoFormattingOptions{
                IndentClassBody = true,
                IndentInterfaceBody = true,
                IndentEnumBody = true,
                IndentMethodBody = true,
                IndentBlocks = true,
                IndentMatchBody = true,
                IndentMatchPatternBody = true,
                IndentBreakStatements = true,
                ClassBraceStyle = BraceStyle.NextLine,
                InterfaceBraceStyle = BraceStyle.NextLine,
                EnumBraceStyle = BraceStyle.NextLine,
                MethodBraceStyle = BraceStyle.NextLine,
                AnonymousMethodBraceStyle = BraceStyle.EndOfLine,

                StatementBraceStyle = BraceStyle.EndOfLine,

                ElseNewLinePlacement = NewLinePlacement.SameLine,
                ElseIfNewLinePlacement = NewLinePlacement.SameLine,
                WhileNewLinePlacement = NewLinePlacement.SameLine,
                ArrayInitializerWrapping = Wrapping.WrapIfTooLong,
                ArrayInitializerBraceStyle = BraceStyle.EndOfLine,

                SpaceBeforeMethodCallParentheses = false,
                SpaceBeforeMethodDeclarationParentheses = false,
                SpaceBeforeConstructorDeclarationParentheses = false,
                SpaceBeforeDelegateDeclarationParentheses = false,
                SpaceBeforeIndexerDeclarationBracket = false,
                SpaceAfterMethodCallParameterComma = true,
                SpaceAfterConstructorDeclarationParameterComma = true,
                NewLineBeforeConstructorInitializerColon = NewLinePlacement.NewLine,
                NewLineAfterConstructorInitializerColon = NewLinePlacement.SameLine,

                SpaceBeforeNewParentheses = false,
                SpacesWithinNewParentheses = false,
                SpacesBetweenEmptyNewParentheses = false,
                SpaceBeforeNewParameterComma = false,
                SpaceAfterNewParameterComma = true,

                SpaceBeforeIfParentheses = true,
                SpaceBeforeWhileParentheses = true,
                SpaceBeforeForParentheses = true,
                SpaceBeforeForeachParentheses = true,
                SpaceBeforeCatchParentheses = true,
                SpaceBeforeSwitchParentheses = true,
                SpaceBeforeLockParentheses = true,
                SpaceBeforeUsingParentheses = true,

                SpaceAroundAssignment = true,
                SpaceAroundLogicalOperator = true,
                SpaceAroundEqualityOperator = true,
                SpaceAroundRelationalOperator = true,
                SpaceAroundBitwiseOperator = true,
                SpaceAroundAdditiveOperator = true,
                SpaceAroundMultiplicativeOperator = true,
                SpaceAroundShiftOperator = true,
                SpaceAroundNullCoalescingOperator = true,
                SpacesWithinParentheses = false,
                SpaceWithinMethodCallParentheses = false,
                SpaceWithinMethodDeclarationParentheses = false,
                SpacesWithinIfParentheses = false,
                SpacesWithinWhileParentheses = false,
                SpacesWithinForParentheses = false,
                SpacesWithinForeachParentheses = false,
                SpacesWithinCatchParentheses = false,
                SpacesWithinSwitchParentheses = false,
                SpacesWithinLockParentheses = false,
                SpacesWithinUsingParentheses = false,
                SpacesWithinCastParentheses = false,
                SpacesWithinSizeOfParentheses = false,
                SpacesWithinTypeOfParentheses = false,
                SpacesWithinCheckedExpressionParantheses = false,
                SpaceBeforeConditionalOperatorCondition = true,
                SpaceAfterConditionalOperatorCondition = true,
                SpaceBeforeConditionalOperatorSeparator = true,
                SpaceAfterConditionalOperatorSeparator = true,
                SpaceBeforeArrayDeclarationBrackets = false,

                SpacesWithinBrackets = false,
                SpacesBeforeBrackets = false,
                SpaceBeforeBracketComma = false,
                SpaceAfterBracketComma = true,

                SpaceBeforeForSemicolon = false,
                SpaceAfterForSemicolon = true,
                SpaceAfterTypecast = false,

                AlignEmbeddedStatements = true,
                //SimplePropertyFormatting = PropertyFormatting.AllowOneLine,
                //AutoPropertyFormatting = PropertyFormatting.AllowOneLine,
                //EmptyLineFormatting = EmptyLineFormatting.DoNotIndent,
                SpaceBeforeMethodDeclarationParameterComma = false,
                SpaceAfterMethodDeclarationParameterComma = true,
                SpaceAfterDelegateDeclarationParameterComma = true,
                SpaceBeforeFieldDeclarationComma = false,
                SpaceAfterFieldDeclarationComma = true,
                SpaceBeforeLocalVariableDeclarationComma = false,
                SpaceAfterLocalVariableDeclarationComma = true,

                SpaceWithinIndexerDeclarationBracket = false,
                SpaceBeforeIndexerDeclarationParameterComma = false,
                SpaceInNamedArgumentAfterDoubleColon = true,

                SpaceAfterIndexerDeclarationParameterComma = true,
                RemoveEndOfLineWhiteSpace = true,

                MinimumBlankLinesBeforeUsings = 0,
                MinimumBlankLinesAfterUsings = 1,

                MinimumBlankLinesBeforeFirstDeclaration = 0,
                MinimumBlankLinesBetweenTypes = 1,
                MinimumBlankLinesBetweenFields = 0,
                MinimumBlankLinesBetweenEventFields = 0,
                MinimumBlankLinesBetweenMembers = 1,
                MinimumBlankLinesAroundRegion = 1,
                MinimumBlankLinesInsideRegion = 1,

                KeepCommentsAtFirstColumn = true,
                ChainedMethodCallWrapping = Wrapping.DoNotModify,
                MethodCallArgumentWrapping = Wrapping.DoNotModify,
                NewLineAferMethodCallOpenParentheses = NewLinePlacement.DoNotCare,
                MethodCallClosingParenthesesOnNewLine = NewLinePlacement.DoNotCare,

                IndexerArgumentWrapping = Wrapping.DoNotModify,
                NewLineAferIndexerOpenBracket = NewLinePlacement.DoNotCare,
                IndexerClosingBracketOnNewLine = NewLinePlacement.DoNotCare,

                NewLineBeforeNewQueryClause = NewLinePlacement.NewLine
            };
        }

        /// <summary>
        /// Creates allman indent style ExpressoFormatting options used in Visual Studio.
        /// </summary>
        public static ExpressoFormattingOptions CreateAllman()
        {
            var base_options = CreateKRStyle();
            base_options.AnonymousMethodBraceStyle = BraceStyle.NextLine;

            base_options.StatementBraceStyle = BraceStyle.NextLine;
            base_options.ArrayInitializerBraceStyle = BraceStyle.NextLine;

            base_options.ElseNewLinePlacement = NewLinePlacement.NewLine;
            base_options.ElseIfNewLinePlacement = NewLinePlacement.SameLine;

            base_options.WhileNewLinePlacement = NewLinePlacement.DoNotCare;
            base_options.ArrayInitializerWrapping = Wrapping.DoNotModify;
            base_options.IndentBlocksInsideExpressions = true;

            return base_options;
        }

        /// <summary>
        /// The Whitesmiths style, also called Wishart style to a lesser extent,
        /// is less common today than the previous three.
        /// It was originally used in the documentation for the first commercial C compiler,
        /// the Whitesmiths Compiler.
        /// </summary>
        public static ExpressoFormattingOptions CreateWhitesmiths()
        {
            var base_options = CreateKRStyle();

            base_options.ClassBraceStyle = BraceStyle.NextLineShifted;
            base_options.InterfaceBraceStyle = BraceStyle.NextLineShifted;
            base_options.EnumBraceStyle = BraceStyle.NextLineShifted;
            base_options.MethodBraceStyle = BraceStyle.NextLineShifted;
            base_options.AnonymousMethodBraceStyle = BraceStyle.NextLineShifted;

            base_options.StatementBraceStyle = BraceStyle.NextLineShifted;
            base_options.IndentBlocksInsideExpressions = true;
            return base_options;
        }

        /// <summary>
        /// Like the Allman and Whitesmiths styles, GNU style puts braces on a line by themselves, indented by 2 spaces,
        /// except when opening a function definition, where they are not indented.
        /// In either case, the contained code is indented by 2 spaces from the braces.
        /// Popularised by Richard Stallman, the layout may be influenced by his background of writing Lisp code.
        /// In Lisp the equivalent to a block (a progn) 
        /// is a first class data entity and giving it its own indent level helps to emphasize that,
        /// whereas in C a block is just syntax.
        /// Although not directly related to indentation, GNU coding style also includes a space before the bracketed 
        /// list of arguments to a function.
        /// </summary>
        public static ExpressoFormattingOptions CreateGNU()
        {
            var base_options = CreateAllman();
            base_options.StatementBraceStyle = BraceStyle.NextLineShifted2;
            return base_options;
        }
    }
}

