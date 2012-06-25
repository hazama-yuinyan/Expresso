using System;
using System.Collections.Generic;

namespace Expresso.Emulator
{
    /// <summary>
    /// 仮想マシン語コード。
    /// </summary>
    public class Instruction
    {
        /// <summary>
        /// 命令の実行方法。
        /// </summary>
        Action<Machine> action;

        /// <summary>
        /// 命令を文字列化したもの。
        /// </summary>
        string mnemonic;

        Instruction(Action<Machine> action, string mnemonic)
        {
            this.action = action;
            this.mnemonic = mnemonic;
        }

        public override string ToString()
        {
            return this.mnemonic;
        }

        /// <summary>
        /// 命令実行。
        /// </summary>
        /// <param name="machine">実行対象の仮想マシン。</param>
        public void Action(Machine machine)
        {
            this.action(machine);
        }

        /// <summary>
        /// No Operation。ダミー。実際には使われない。
        /// </summary>
        public static readonly Instruction Nop = new Instruction(null, null);

        /// <summary>
        /// 停止命令。
        /// </summary>
        public static readonly Instruction Halt = new Instruction(m => m.Halt(), "Halt");

        /// <summary>
        /// ローカル変数用の領域をスタック上に確保。
        /// </summary>
        /// <param name="count">ローカル変数の数。</param>
        /// <returns>PrepareLocal 命令。</returns>
        public static Instruction PrepareLocal(int count)
        {
            return new Instruction(m => m.PrepareLocalVariable(count), string.Format("LocalVariable {0}", count));
        }

        /// <summary>
        /// 定数をスタックに積む。
        /// </summary>
        /// <param name="value">定数の値。</param>
        /// <returns>LoadConstant 命令。</returns>
        public static Instruction LoadConstant(int value)
        {
            return new Instruction(m => m.LoadConstant(value), string.Format("LoadConstant {0}", value));
        }

        /// <summary>
        /// ローカル変数をスタックに積む。
        /// </summary>
        /// <param name="index">変数番号。</param>
        /// <returns>LoadLocal 命令。</returns>
        public static Instruction LoadLocal(int index)
        {
            return new Instruction(m => m.LoadLocal(index), string.Format("LoadLocal {0}", index));
        }

        /// <summary>
        /// スタック最上位の値をローカル変数領域に格納。
        /// </summary>
        /// <param name="index">変数番号。</param>
        /// <returns>StoreLocal 命令。</returns>
        public static Instruction StoreLocal(int index)
        {
            return new Instruction(m => m.StoreLocal(index), string.Format("StoreLocal {0}", index));
        }

        /// <summary>
        /// 条件ジャンプ命令。
        /// </summary>
        /// <param name="jumpTo">ジャンプ先（相対アドレス）。</param>
        /// <returns>Branch 命令。</returns>
        public static Instruction Branch(int jumpTo)
        {
            return new Instruction(m => m.Branch(jumpTo), string.Format("Branch {0}", jumpTo));
        }

        /// <summary>
        /// 無条件ジャンプ命令。
        /// </summary>
        /// <param name="jumpTo">ジャンプ先（相対アドレス）。</param>
        /// <returns>Jump 命令。</returns>
        public static Instruction Jump(int jumpTo)
        {
            return new Instruction(m => m.Jump(jumpTo), string.Format("Jump {0}", jumpTo));
        }

        /// <summary>
        /// 関数呼び出し。
        /// </summary>
        /// <param name="f">関数のASTノード。</param>
        /// <param name="addressTable">関数アドレステーブル。</param>
        /// <returns>Call 命令。</returns>
        /// <summary>
        /// 関数アドレス解決は、Call が呼ばれた時点でなくても、
        /// 実際に Instruction.Action が呼ばれるまでの間に解決されてればOK。
        /// </summary>
        public static Instruction Call(Ast.Function f, Dictionary<Ast.Function, int> addressTable)
        {
            return new Instruction(
                m => m.Call(addressTable[f], f.Parameters.Count),
                string.Format("Call {0}", f.Signature())
                );
        }

        /// <summary>
        /// 関数からリターン。
        /// </summary>
        /// <returns>Return 命令。</returns>
        public static Instruction Return()
        {
            return new Instruction(
                m => m.Return(),
                "Return");
        }

        #region 算術論理演算。

        public static Instruction Add() { return new Instruction(m => m.Add(), "Add"); }
        public static Instruction Subtract() { return new Instruction(m => m.Subtract(), "Subtract"); }
        public static Instruction Multiply() { return new Instruction(m => m.Multiply(), "Multiply"); }
        public static Instruction Divide() { return new Instruction(m => m.Divide(), "Divide"); }
        public static Instruction LessEqual() { return new Instruction(m => m.LessEqual(), "LessEqual"); }
        public static Instruction LessThan() { return new Instruction(m => m.LessThan(), "LessThan"); }
        public static Instruction GreaterEqual() { return new Instruction(m => m.GreaterEqual(), "GreaterEqual"); }
        public static Instruction GreaterThan() { return new Instruction(m => m.GreaterThan(), "GreaterThan"); }
        public static Instruction Equal() { return new Instruction(m => m.Equal(), "Equal"); }
        public static Instruction NotEqual() { return new Instruction(m => m.NotEqual(), "NotEqual"); }
        public static Instruction Negate() { return new Instruction(m => m.Negate(), "Negate"); }
        public static Instruction And() { return new Instruction(m => m.And(), "And"); }
        public static Instruction Or() { return new Instruction(m => m.Or(), "Or"); }

        #endregion
    }
}
