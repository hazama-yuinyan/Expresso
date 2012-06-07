using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Expresso.Emulator
{
	/// <summary>
	/// スタック型の仮想マシン。
	/// </summary>
	public class Machine
	{
		/// <summary>
		/// スタックポインター。
		/// 今、スタックのどこまで値が積まれてるかを示す。
		/// </summary>
		public int StackPointer { get { return this.stack.Count; } }

		/// <summary>
		/// ベースポインター。
		/// 関数呼び出し時、どこからローカル変数領域として使うかとか、
		/// リターン時にどこまでポップすればいいのかを示す。
		/// </summary>
		public int BasePointer { get; private set; }

		List<StackItem> stack = new List<StackItem> ();

		/// <summary>
		/// スタック中の値一覧。
		/// </summary>
		public IEnumerable<StackItem> Stack { get { return this.stack; } }

		/// <summary>
		/// 仮想マシン語コード一覧。
		/// </summary>
		public List<Instruction> Instructions { get; private set; }

		/// <summary>
		/// プログラムカウンター。
		/// 今、マシン語コードのどこを実行しているか。
		/// </summary>
		public int ProgramCounter { get; private set; }

		/// <summary>
		/// プログラム実行停止しているかどうか。
		/// </summary>
		/// <remarks>
		/// Halt 命令が実行されたら実行停止。
		/// </remarks>
		public bool IsHalted { get; private set; }

		/// <summary>
		/// 関数呼び出しの深さ。
		/// </summary>
		public int NestLevel { get; private set; }

		/// <summary>
		/// 値の出力先。
		/// </summary>
		/// <remarks>
		/// トップレベルのスコープで Return 命令が実行された場合、
		/// 値を出力する。
		/// </remarks>
		public event Action<int> Output;

		/// <summary>
		/// 直前に実行した操作の内容。
		/// </summary>
		public string LastOperation { get; private set; }

		/// <summary>
		/// 仮想マシン語コードを与えて初期化。
		/// </summary>
		/// <param name="instructions">仮想マシン語コード。</param>
		public Machine (IEnumerable<Instruction> instructions)
		{
			this.Instructions = new List<Instruction> ();
			this.Instructions.AddRange (instructions);

			this.Reset ();
		}

		/// <summary>
		/// 値をスタックに積む。
		/// </summary>
		/// <param name="item">値。</param>
		private void Push (StackItem item)
		{
			this.stack.Add (item);
		}

		/// <summary>
		/// 値をスタックから取り出す。
		/// </summary>
		/// <returns>取り出した値。</returns>
		private StackItem Pop ()
		{
			Debug.Assert (this.stack.Count > 0);

			var item = this.stack [this.stack.Count - 1];
			this.stack.RemoveAt (this.stack.Count - 1);
			return item;
		}

		/// <summary>
		/// ポップせずに値だけ取り出す。
		/// </summary>
		private StackItem Top {
			get {
				Debug.Assert (this.stack.Count > 0);
				return this.stack [this.stack.Count - 1];
			}
		}

		/// <summary>
		/// 1命令実行。
		/// </summary>
		public void Run ()
		{
			if (this.IsHalted)
				return;

			this.Instructions [this.ProgramCounter].Action (this);
			this.ProgramCounter++;
		}

		/// <summary>
		/// 命令の実行状態を初期化。
		/// </summary>
		public void Reset ()
		{
			this.ProgramCounter = 0;
			this.BasePointer = 0;
			this.IsHalted = false;
			this.stack.Clear ();
			this.NestLevel = 0;

			this.LastOperation = "Reset";
		}

		/// <summary>
		/// 実行終了のお知らせ。
		/// </summary>
		public void Halt ()
		{
			this.IsHalted = true;

			this.LastOperation = "Halt";
		}

		/// <summary>
		/// ローカル変数用の領域を count 個作る。
		/// </summary>
		/// <param name="count">ローカル変数の数。</param>
		public void PrepareLocalVariable (int count)
		{
			for (int i = 0; i < count; i++) {
				this.Push (new StackItem
                {
                    Value = 0,
                    Usage = StackItemUsage.Local,
                    NestLevel = this.NestLevel,
                });
			}

			var op = string.Format ("Prepare {0} Local Variables", count);

			if (!string.IsNullOrEmpty (this.LastOperation) && this.LastOperation.StartsWith ("Call"))
				this.LastOperation += "\n" + op;
			else
				this.LastOperation = op;
		}

		/// <summary>
		/// ローカル変数を取り出してスタックに積む。
		/// </summary>
		/// <param name="index">ローカル変数の番号。</param>
		public void LoadLocal (int index)
		{
			var varStore = this.stack [this.BasePointer + index];
			Debug.Assert (varStore.Usage == StackItemUsage.Local);

			this.Push (new StackItem
            {
                Value = varStore.Value,
                Usage = StackItemUsage.Temporary,
                NestLevel = this.NestLevel,
            });

			this.LastOperation = string.Format ("Load {0}th Local Varible: {1}", index, varStore.Value);
		}

		/// <summary>
		/// スタックの先頭をローカル変数に格納。
		/// </summary>
		/// <param name="index">ローカル変数の番号。</param>
		public void StoreLocal (int index)
		{
			var top = this.Pop ();
			Debug.Assert (top.Usage == StackItemUsage.Temporary);

			this.SetLocal (index, top.Value);

			this.LastOperation = string.Format ("Store {1} to {0}th Local Varible", index, top.Value);
		}

		/// <summary>
		/// ポップせずにローカル変数の値だけ更新。
		/// </summary>
		/// <param name="index">ローカル変数の番号。</param>
		/// <param name="value">値。</param>
		private void SetLocal (int index, int value)
		{
			var varStore = this.stack [this.BasePointer + index];
			Debug.Assert (varStore.Usage == StackItemUsage.Local);

			varStore.Value = value;
		}

		/// <summary>
		/// 定数値をスタックに積む。
		/// </summary>
		/// <param name="value">定数。</param>
		public void LoadConstant (int value)
		{
			this.Push (new StackItem
            {
                Value = value,
                Usage = StackItemUsage.Temporary,
                NestLevel = this.NestLevel,
            });

			this.LastOperation = string.Format ("Load Constant: {0}", value);
		}

		/// <summary>
		/// 関数呼び出し。
		/// </summary>
		/// <param name="jumpTo">関数の先頭アドレス（絶対アドレス）。</param>
		private void Call (int jumpTo)
		{
			this.NestLevel++;

			this.Push (new StackItem
            {
                Value = this.BasePointer,
                Usage = StackItemUsage.BaseAddress,
                NestLevel = this.NestLevel,
            });

			this.Push (new StackItem
            {
                Value = this.ProgramCounter,
                Usage = StackItemUsage.ReturnAddress,
                NestLevel = this.NestLevel,
            });

			this.BasePointer = this.StackPointer;
			this.ProgramCounter = jumpTo - 1; // Callが呼ばれたあと、さらに pc++ されるはずなので。

			this.LastOperation = string.Format ("Call Function (Address: {0})", jumpTo);
		}

		/// <summary>
		/// 関数呼び出し。
		/// </summary>
		/// <param name="jumpTo">関数の先頭アドレス（絶対アドレス）。</param>
		/// <param name="argCount">引数の数。</param>
		public void Call (int jumpTo, int argCount)
		{
			var args = new int[argCount];

			for (int i = 0; i < argCount; i++) {
				var top = this.Pop ();
				Debug.Assert (top.Usage == StackItemUsage.Temporary);

				args [argCount - i - 1] = top.Value;
			}

			this.Call (jumpTo);
			this.PrepareLocalVariable (argCount);

			for (int i = 0; i < argCount; i++) {
				this.SetLocal (i, args [i]);
			}

			this.LastOperation += string.Format ("\nWith {0} Arguments", argCount);
		}

		/// <summary>
		/// 関数から呼び出し元に戻る。
		/// </summary>
		public void Return ()
		{
			var retVal = this.Pop ();
			Debug.Assert (retVal.Usage == StackItemUsage.Temporary);

			if (this.BasePointer == 0) {
				var o = this.Output;
				if (o != null)
					o (retVal.Value);
			} else {
				Debug.Assert (this.BasePointer > 2);

				// PC, Base の復元
				var sp = this.BasePointer - 2;

				var retrunTo = this.stack [this.BasePointer - 1];
				Debug.Assert (retrunTo.Usage == StackItemUsage.ReturnAddress);
				this.ProgramCounter = retrunTo.Value;

				var baseAddr = this.stack [this.BasePointer - 2];
				Debug.Assert (baseAddr.Usage == StackItemUsage.BaseAddress);
				this.BasePointer = baseAddr.Value;

				// 関数内で使っていた分のクリア
				while (sp < this.stack.Count)
					this.stack.RemoveAt (sp);

				this.NestLevel--;
				retVal.NestLevel = this.NestLevel;

				// return value
				this.Push (retVal);
			}

			this.LastOperation = string.Format ("Return {0}", retVal.Value);
		}

		/// <summary>
		/// 条件ジャンプ。
		/// </summary>
		/// <param name="jumpTo">ジャンプ先（相対アドレス）。</param>
		public void Branch (int jumpTo)
		{
			var top = this.Pop ();
			Debug.Assert (top.Usage == StackItemUsage.Temporary);

			if (top.Value != 0) {
				this.Jump (jumpTo);

				this.LastOperation = string.Format ("Branch To {0}", jumpTo);
			} else{
				this.LastOperation = string.Format ("Branch To {0} (Not Done)", jumpTo);
			}
		}

		/// <summary>
		/// 無条件ジャンプ。
		/// </summary>
		/// <param name="jumpTo">ジャンプ先（相対アドレス）。</param>
		public void Jump (int jumpTo)
		{
			this.ProgramCounter += jumpTo;

			this.LastOperation = string.Format ("Jump To {0}", jumpTo);
		}

        #region 算術論理演算

		private void BinaryOperation (Func<int, int, int> operation, string opStr)
		{
			var right = this.Pop ();
			Debug.Assert (right.Usage == StackItemUsage.Temporary);

			var left = this.Top;
			Debug.Assert (left.Usage == StackItemUsage.Temporary);

			var r = right.Value;
			var l = left.Value;
			var result = operation (l, r);

			left.Value = result;

			this.LastOperation = string.Format ("{1} {0} {2} → {3}", opStr, l, r, result);
		}

		public void Add ()
		{
			this.BinaryOperation ((x, y) => x + y, "+");
		}

		public void Subtract ()
		{
			this.BinaryOperation ((x, y) => x - y, "－");
		}

		public void Multiply ()
		{
			this.BinaryOperation ((x, y) => x * y, "×");
		}

		public void Divide ()
		{
			this.BinaryOperation ((x, y) => x / y, "÷");
		}

		public void Equal ()
		{
			this.BinaryOperation ((x, y) => x == y ? 1 : 0, "＝");
		}

		public void NotEqual ()
		{
			this.BinaryOperation ((x, y) => x != y ? 1 : 0, "≠");
		}

		public void LessThan ()
		{
			this.BinaryOperation ((x, y) => x < y ? 1 : 0, "＜");
		}

		public void GreaterThan ()
		{
			this.BinaryOperation ((x, y) => x > y ? 1 : 0, "＞");
		}

		public void LessEqual ()
		{
			this.BinaryOperation ((x, y) => x <= y ? 1 : 0, "≦");
		}

		public void GreaterEqual ()
		{
			this.BinaryOperation ((x, y) => x >= y ? 1 : 0, "≧");
		}

		public void And ()
		{
			this.BinaryOperation ((x, y) => x != 0 && y != 0 ? 1 : 0, "∧");
		}

		public void Or ()
		{
			this.BinaryOperation ((x, y) => x != 0 || y != 0 ? 1 : 0, "∨");
		}

		public void Negate ()
		{
			var item = this.Top;
			Debug.Assert (item.Usage == StackItemUsage.Temporary);

			var v = item.Value;

			item.Value = -v;

			this.LastOperation = string.Format ("－{0} → {1}", v, -v);
		}

        #endregion
	}
}
