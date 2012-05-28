using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Expresso.Emulator
{
    /// <summary>
    /// スタック内の項目の利用用途。
    /// </summary>
    /// <remarks>
    /// スタックのエミュレート自体には不要だけど、
    /// デモをわかりやすくするためと、
    /// デバッグAssert用に。
    /// </remarks>
    public enum StackItemUsage
    {
        /// <summary>
        /// ローカル変数領域。
        /// </summary>
        Local,

        /// <summary>
        /// 計算途中とかに使う。
        /// </summary>
        Temporary,

        /// <summary>
        /// 関数リターン時にBaseAddressを元に戻すために使う記憶領域。
        /// </summary>
        BaseAddress,

        /// <summary>
        /// 関数のリターン先を記憶しておくための領域。
        /// </summary>
        ReturnAddress,
    }

    /// <summary>
    /// スタック内の項目。
    /// </summary>
    public class StackItem
    {
        /// <summary>
        /// 項目の値。
        /// </summary>
        public int Value { get; internal set; }

        /// <summary>
        /// 項目の用途。
        /// </summary>
        public StackItemUsage Usage { get; internal set; }

        /// <summary>
        /// 関数呼び出しの深さ。
        /// </summary>
        public int NestLevel { get; internal set; }
    }
}
