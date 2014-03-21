using System;


namespace Expresso.Formatting
{
    public enum BraceStyle
    {
        DoNotModify,
        EndOfLine,
        EndOfLineWithoutSpace,
        NextLine,
        NextLineShifted,
        NextLineShifted2,
        BannerStyle
    }

    public enum PropertyFormatting
    {
        AllowOneLine,
        ForceOneLine,
        ForceNewLine
    }

    public enum Wrapping
    {
        DoNotModify,
        DoNotWrap,
        WrapAlways,
        WrapIfTooLong
    }

    public enum NewLinePlacement
    {
        DoNotCare,
        NewLine,
        SameLine
    }

    public enum EmptyLineFormatting
    {
        DoNotModify,
        Indent,
        DoNotIndent
    }

    public class ExpressoFormattingOptions
    {
        public string Name{
            get;
            set;
        }

        public bool IsBuiltIn{
            get;
            set;
        }

        public ExpressoFormattingOptions Clone()
        {
            return (ExpressoFormattingOptions)MemberwiseClone();
        }

        #region Indentation

        public bool IndentClassBody{
            get;
            set;
        }

        public bool IndentStructBody{
            get;
            set;
        }

        public bool IndentInterfaceBody{
            get;
            set;
        }

        public bool IndentEnumBody{
            get;
            set;
        }

        public bool IndentMethodBody{
            get;
            set;
        }

        public bool IndentPropertyBody{
            get;
            set;
        }

        public bool IndentBlocks{
            get;
            set;
        }

        public bool IndentSwitchBody{
            get;
            set;
        }

        public bool IndentCaseBody{
            get;
            set;
        }

        public bool IndentBreakStatements{
            get;
            set;
        }

        public bool AlignEmbeddedStatements{
            get;
            set;
        }

        public bool AlignElseInIfStatements{
            get;
            set;
        }

        public bool AlignToMemberReferenceDot{
            get;
            set;
        }

        public bool IndentBlocksInsideExpressions{
            get;
            set;
        }

        #endregion

        #region Braces

        public BraceStyle ClassBraceStyle{ // tested
            get;
            set;
        }

        public BraceStyle InterfaceBraceStyle{ // tested
            get;
            set;
        }

        public BraceStyle StructBraceStyle{ // tested
            get;
            set;
        }

        public BraceStyle EnumBraceStyle{ // tested
            get;
            set;
        }

        public BraceStyle MethodBraceStyle{ // tested
            get;
            set;
        }

        public BraceStyle AnonymousMethodBraceStyle{
            get;
            set;
        }

        public BraceStyle ConstructorBraceStyle{  // tested
            get;
            set;
        }

        public BraceStyle DestructorBraceStyle{ // tested
            get;
            set;
        }

        public BraceStyle PropertyBraceStyle{ // tested
            get;
            set;
        }

        public BraceStyle PropertyGetBraceStyle{ // tested
            get;
            set;
        }

        public BraceStyle PropertySetBraceStyle{ // tested
            get;
            set;
        }

        public PropertyFormatting SimpleGetBlockFormatting{ // tested
            get;
            set;
        }

        public PropertyFormatting SimpleSetBlockFormatting{ // tested
            get;
            set;
        }

        public BraceStyle EventBraceStyle{ // tested
            get;
            set;
        }

        public BraceStyle EventAddBraceStyle{ // tested
            get;
            set;
        }

        public BraceStyle EventRemoveBraceStyle{ // tested
            get;
            set;
        }

        public bool AllowEventAddBlockInline{ // tested
            get;
            set;
        }

        public bool AllowEventRemoveBlockInline{ // tested
            get;
            set;
        }

        public BraceStyle StatementBraceStyle{ // tested
            get;
            set;
        }

        public bool AllowIfBlockInline{
            get;
            set;
        }

        bool allowOneLinedArrayInitialziers = true;

        public bool AllowOneLinedArrayInitialziers{
            get{
                return allowOneLinedArrayInitialziers;
            }
            set{
                allowOneLinedArrayInitialziers = value;
            }
        }

        #endregion

        #region NewLines

        public NewLinePlacement ElseNewLinePlacement{ // tested
            get;
            set;
        }

        public NewLinePlacement ElseIfNewLinePlacement{ // tested
            get;
            set;
        }

        public NewLinePlacement CatchNewLinePlacement{ // tested
            get;
            set;
        }

        public NewLinePlacement FinallyNewLinePlacement{ // tested
            get;
            set;
        }

        public NewLinePlacement WhileNewLinePlacement{ // tested
            get;
            set;
        }

        NewLinePlacement embeddedStatementPlacement = NewLinePlacement.NewLine;

        public NewLinePlacement EmbeddedStatementPlacement{
            get{
                return embeddedStatementPlacement;
            }
            set{
                embeddedStatementPlacement = value;
            }
        }

        #endregion

        #region Spaces

        // Methods
        public bool SpaceBeforeMethodDeclarationParentheses{ // tested
            get;
            set;
        }

        public bool SpaceBetweenEmptyMethodDeclarationParentheses{
            get;
            set;
        }

        public bool SpaceBeforeMethodDeclarationParameterComma{ // tested
            get;
            set;
        }

        public bool SpaceAfterMethodDeclarationParameterComma{ // tested
            get;
            set;
        }

        public bool SpaceWithinMethodDeclarationParentheses{ // tested
            get;
            set;
        }
        // Method calls
        public bool SpaceBeforeMethodCallParentheses{ // tested
            get;
            set;
        }

        public bool SpaceBetweenEmptyMethodCallParentheses{ // tested
            get;
            set;
        }

        public bool SpaceBeforeMethodCallParameterComma{ // tested
            get;
            set;
        }

        public bool SpaceAfterMethodCallParameterComma{ // tested
            get;
            set;
        }

        public bool SpaceWithinMethodCallParentheses{ // tested
            get;
            set;
        }
        // fields
        public bool SpaceBeforeFieldDeclarationComma{ // tested
            get;
            set;
        }

        public bool SpaceAfterFieldDeclarationComma{ // tested
            get;
            set;
        }
        // local variables
        public bool SpaceBeforeLocalVariableDeclarationComma{ // tested
            get;
            set;
        }

        public bool SpaceAfterLocalVariableDeclarationComma{ // tested
            get;
            set;
        }
        // constructors
        public bool SpaceBeforeConstructorDeclarationParentheses{ // tested
            get;
            set;
        }

        public bool SpaceBetweenEmptyConstructorDeclarationParentheses{ // tested
            get;
            set;
        }

        public bool SpaceBeforeConstructorDeclarationParameterComma{ // tested
            get;
            set;
        }

        public bool SpaceAfterConstructorDeclarationParameterComma{ // tested
            get;
            set;
        }

        public bool SpaceWithinConstructorDeclarationParentheses{ // tested
            get;
            set;
        }

        public NewLinePlacement NewLineBeforeConstructorInitializerColon{
            get;
            set;
        }

        public NewLinePlacement NewLineAfterConstructorInitializerColon{
            get;
            set;
        }
        // indexer
        public bool SpaceBeforeIndexerDeclarationBracket{ // tested
            get;
            set;
        }

        public bool SpaceWithinIndexerDeclarationBracket{ // tested
            get;
            set;
        }

        public bool SpaceBeforeIndexerDeclarationParameterComma{
            get;
            set;
        }

        public bool SpaceAfterIndexerDeclarationParameterComma{
            get;
            set;
        }
        // delegates
        public bool SpaceBeforeDelegateDeclarationParentheses{
            get;
            set;
        }

        public bool SpaceBetweenEmptyDelegateDeclarationParentheses{
            get;
            set;
        }

        public bool SpaceBeforeDelegateDeclarationParameterComma{
            get;
            set;
        }

        public bool SpaceAfterDelegateDeclarationParameterComma{
            get;
            set;
        }

        public bool SpaceWithinDelegateDeclarationParentheses{
            get;
            set;
        }

        public bool SpaceBeforeNewParentheses{ // tested
            get;
            set;
        }

        public bool SpaceBeforeIfParentheses{ // tested
            get;
            set;
        }

        public bool SpaceBeforeWhileParentheses{ // tested
            get;
            set;
        }

        public bool SpaceBeforeForParentheses{ // tested
            get;
            set;
        }

        public bool SpaceBeforeForeachParentheses{ // tested
            get;
            set;
        }

        public bool SpaceBeforeCatchParentheses{ // tested
            get;
            set;
        }

        public bool SpaceBeforeSwitchParentheses{ // tested
            get;
            set;
        }

        public bool SpaceBeforeLockParentheses{ // tested
            get;
            set;
        }

        public bool SpaceBeforeUsingParentheses{ // tested
            get;
            set;
        }

        public bool SpaceAroundAssignment{ // tested
            get;
            set;
        }

        public bool SpaceAroundLogicalOperator{ // tested
            get;
            set;
        }

        public bool SpaceAroundEqualityOperator{ // tested
            get;
            set;
        }

        public bool SpaceAroundRelationalOperator{ // tested
            get;
            set;
        }

        public bool SpaceAroundBitwiseOperator{ // tested
            get;
            set;
        }

        public bool SpaceAroundAdditiveOperator{ // tested
            get;
            set;
        }

        public bool SpaceAroundMultiplicativeOperator{ // tested
            get;
            set;
        }

        public bool SpaceAroundShiftOperator{ // tested
            get;
            set;
        }

        public bool SpaceAroundNullCoalescingOperator{ // Tested
            get;
            set;
        }

        public bool SpaceAfterUnsafeAddressOfOperator{ // Tested
            get;
            set;
        }

        public bool SpaceAfterUnsafeAsteriskOfOperator{ // Tested
            get;
            set;
        }

        public bool SpaceAroundUnsafeArrowOperator{ // Tested
            get;
            set;
        }

        public bool SpacesWithinParentheses{ // tested
            get;
            set;
        }

        public bool SpacesWithinIfParentheses{ // tested
            get;
            set;
        }

        public bool SpacesWithinWhileParentheses{ // tested
            get;
            set;
        }

        public bool SpacesWithinForParentheses{ // tested
            get;
            set;
        }

        public bool SpacesWithinForeachParentheses{ // tested
            get;
            set;
        }

        public bool SpacesWithinCatchParentheses{ // tested
            get;
            set;
        }

        public bool SpacesWithinSwitchParentheses{ // tested
            get;
            set;
        }

        public bool SpacesWithinLockParentheses{ // tested
            get;
            set;
        }

        public bool SpacesWithinUsingParentheses{ // tested
            get;
            set;
        }

        public bool SpacesWithinCastParentheses{ // tested
            get;
            set;
        }

        public bool SpacesWithinSizeOfParentheses{ // tested
            get;
            set;
        }

        public bool SpaceBeforeSizeOfParentheses{ // tested
            get;
            set;
        }

        public bool SpacesWithinTypeOfParentheses{ // tested
            get;
            set;
        }

        public bool SpacesWithinNewParentheses{ // tested
            get;
            set;
        }

        public bool SpacesBetweenEmptyNewParentheses{ // tested
            get;
            set;
        }

        public bool SpaceBeforeNewParameterComma{ // tested
            get;
            set;
        }

        public bool SpaceAfterNewParameterComma{ // tested
            get;
            set;
        }

        public bool SpaceBeforeTypeOfParentheses{ // tested
            get;
            set;
        }

        public bool SpacesWithinCheckedExpressionParantheses{ // tested
            get;
            set;
        }

        public bool SpaceBeforeConditionalOperatorCondition{ // tested
            get;
            set;
        }

        public bool SpaceAfterConditionalOperatorCondition{ // tested
            get;
            set;
        }

        public bool SpaceBeforeConditionalOperatorSeparator{ // tested
            get;
            set;
        }

        public bool SpaceAfterConditionalOperatorSeparator{ // tested
            get;
            set;
        }
        // brackets
        public bool SpacesWithinBrackets{ // tested
            get;
            set;
        }

        public bool SpacesBeforeBrackets{ // tested
            get;
            set;
        }

        public bool SpaceBeforeBracketComma{ // tested
            get;
            set;
        }

        public bool SpaceAfterBracketComma{ // tested
            get;
            set;
        }

        public bool SpaceBeforeForSemicolon{ // tested
            get;
            set;
        }

        public bool SpaceAfterForSemicolon{ // tested
            get;
            set;
        }

        public bool SpaceAfterTypecast{ // tested
            get;
            set;
        }

        public bool SpaceBeforeArrayDeclarationBrackets{ // tested
            get;
            set;
        }

        public bool SpaceInNamedArgumentAfterDoubleColon{
            get;
            set;
        }

        public bool RemoveEndOfLineWhiteSpace{
            get;
            set;
        }

        public bool SpaceBeforeSemicolon{
            get;
            set;
        }

        #endregion

        #region Blank Lines

        public int MinimumBlankLinesBeforeUsings{
            get;
            set;
        }

        public int MinimumBlankLinesAfterUsings{
            get;
            set;
        }

        public int MinimumBlankLinesBeforeFirstDeclaration{
            get;
            set;
        }

        public int MinimumBlankLinesBetweenTypes{
            get;
            set;
        }

        public int MinimumBlankLinesBetweenFields{
            get;
            set;
        }

        public int MinimumBlankLinesBetweenEventFields{
            get;
            set;
        }

        public int MinimumBlankLinesBetweenMembers{
            get;
            set;
        }

        public int MinimumBlankLinesAroundRegion{
            get;
            set;
        }

        public int MinimumBlankLinesInsideRegion{
            get;
            set;
        }

        #endregion

        #region Keep formatting

        public bool KeepCommentsAtFirstColumn{
            get;
            set;
        }

        #endregion

        #region Wrapping

        public Wrapping ArrayInitializerWrapping{
            get;
            set;
        }

        public BraceStyle ArrayInitializerBraceStyle{
            get;
            set;
        }

        public Wrapping ChainedMethodCallWrapping{
            get;
            set;
        }

        public Wrapping MethodCallArgumentWrapping{
            get;
            set;
        }

        public NewLinePlacement NewLineAferMethodCallOpenParentheses{
            get;
            set;
        }

        public NewLinePlacement MethodCallClosingParenthesesOnNewLine{
            get;
            set;
        }

        public Wrapping IndexerArgumentWrapping{
            get;
            set;
        }

        public NewLinePlacement NewLineAferIndexerOpenBracket{
            get;
            set;
        }

        public NewLinePlacement IndexerClosingBracketOnNewLine{
            get;
            set;
        }

        public Wrapping MethodDeclarationParameterWrapping{
            get;
            set;
        }

        public NewLinePlacement NewLineAferMethodDeclarationOpenParentheses{
            get;
            set;
        }

        public NewLinePlacement MethodDeclarationClosingParenthesesOnNewLine{
            get;
            set;
        }

        public Wrapping IndexerDeclarationParameterWrapping{
            get;
            set;
        }

        public NewLinePlacement NewLineAferIndexerDeclarationOpenBracket{
            get;
            set;
        }

        public NewLinePlacement IndexerDeclarationClosingBracketOnNewLine{
            get;
            set;
        }

        public bool AlignToFirstIndexerArgument{
            get;
            set;
        }

        public bool AlignToFirstIndexerDeclarationParameter{
            get;
            set;
        }

        public bool AlignToFirstMethodCallArgument{
            get;
            set;
        }

        public bool AlignToFirstMethodDeclarationParameter{
            get;
            set;
        }

        public NewLinePlacement NewLineBeforeNewQueryClause{
            get;
            set;
        }

        #endregion

        internal ExpressoFormattingOptions()
        {
        }
    }
}

