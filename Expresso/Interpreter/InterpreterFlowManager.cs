using System;
using System.Collections.Generic;
using System.Linq;

using Expresso.Ast;
using Expresso.Runtime.Operations;

namespace Expresso.Interpreter
{
	/// <summary>
	/// A stack frame that is used for both evaluating sub-expressions of an AST node and a call stack.
	/// </summary>
	internal sealed class EvaluationFrame
	{
		private InterpreterFlowManager manager;

		public Node TargetNode{get; private set;}

		public int ScopeFramePointer{get; internal set;}

		/// <summary>
		/// The index of the next child node to be evaluated.
		/// </summary>
		public int ChildCounter{get; internal set;}

		/// <summary>
		/// Indicates the next step of an operation if the TargetNode has multiple operations.
		/// </summary>
		public int StepCounter{get; internal set;}

		public EvaluationFrame(Node node, int framePointer, int childCounter, InterpreterFlowManager flowManager)
		{
			TargetNode = node;
			ScopeFramePointer = framePointer;
			ChildCounter = childCounter;
			StepCounter = 0;
			manager = flowManager;
		}

		#region Dynamic operation facilities
		public void Assign(int offset, object value)
		{
			manager.Assign(ScopeFramePointer + offset, value);
		}

		public object Get(int offset)
		{
			return manager.GetValue(ScopeFramePointer + offset);
		}

		public void Remove(int offset)
		{
			manager.RemoveAt(ScopeFramePointer + offset);
		}

		public void Clear()
		{
			for(int offset = manager.ValueCount; offset > ScopeFramePointer; --offset)
				manager.PopValue();
		}

		public void Dup(int srcIndex)
		{
			manager.Dup(srcIndex + ScopeFramePointer);
		}
		#endregion
	}

	/// <summary>
	/// Manages the tree of evaluated AST node and a call stack.
	/// </summary>
	internal sealed class InterpreterFlowManager
	{
		/// <summary>
		/// The maximum size of the (call) stack. (Maybe it's too large?)
		/// </summary>
		private const int MaxStackSize = 1024;

		private List<EvaluationFrame> frames = new List<EvaluationFrame>();

		private List<object> stack = new List<object>(MaxStackSize);

		private EvaluationFrame cur_scope = null;	//現在のスコープに対応する評価フレーム

		/// <summary>
		/// Gets the count of currently active evaluation frames.
		/// </summary>
		/// <value>
		/// The count.
		/// </value>
		public int Count{
			get{
				return frames.Count;
			}
		}

		/// <summary>
		/// Gets the count of values piled on the stack.
		/// </summary>
		/// <value>
		/// The value count.
		/// </value>
		public int ValueCount{
			get{
				return stack.Count;
			}
		}

		#region Dynamic operation facilities
		internal void Assign(int offset, object value)
		{
#if DEBUG
			if(offset < 0 || offset > stack.Count)
				throw ExpressoOps.RuntimeError("Memory offset is out of range!");
#endif
			stack[offset] = value;
		}

		internal object GetValue(int offset)
		{
#if DEBUG
			if(offset < 0 || offset > stack.Count)
				throw ExpressoOps.RuntimeError("Memory offset is out of range!");
#endif
			return stack[offset];
		}

		internal void RemoveAt(int offset)
		{
#if DEBUG
			if(offset < 0 || offset > stack.Count)
				throw ExpressoOps.RuntimeError("Memory offset is out of range!");
#endif
			stack.RemoveAt(offset);
		}

		public void Dup(int srcIndex)
		{
#if DEBUG
			if(srcIndex < 0 || srcIndex > stack.Count)
				throw ExpressoOps.RuntimeError("Memory offset is out of range!");
#endif
			stack.Add(stack[srcIndex]);
		}
		#endregion

		public void Push(Node node, int childCounter = 0)
		{
			var frame = new EvaluationFrame(node, (node is Call) ? stack.Count :
			                                	(cur_scope != null) ? cur_scope.ScopeFramePointer : 0,
			                                childCounter, this);
			frames.Add(frame);
			if(node is Call)
				cur_scope = frame;
		}

		public void PushValue(object item)
		{
			if(stack.Count == MaxStackSize)
				throw ExpressoOps.RuntimeError("Stack overflow");

			stack.Add(item);
		}

		public EvaluationFrame Top()
		{
			if(frames.Count == 0)
				throw new InvalidOperationException();

			return frames[frames.Count - 1];
		}

		public object TopValue()
		{
			if(stack.Count == 0)
				throw new InvalidOperationException();

			return stack[stack.Count - 1];
		}

		public EvaluationFrame Pop()
		{
			if(frames.Count == 0)
				throw new InvalidOperationException();

			var tmp = frames[frames.Count - 1];
			frames.RemoveAt(frames.Count - 1);
			if(tmp.TargetNode is ScopeStatement)	//一つ上のスコープに対応する評価フレームをセットする
				cur_scope = ((IEnumerable<EvaluationFrame>)frames).Reverse().FirstOrDefault(x => x.TargetNode is Call);

			return tmp;
		}

		public object PopValue()
		{
			if(stack.Count == 0)
				throw new InvalidOperationException();

			var tmp = stack[stack.Count - 1];
			stack.RemoveAt(stack.Count - 1);
			return tmp;
		}

		public bool IsEvaluating(Node node)
		{
			return frames.Any(x => x.TargetNode == node);
		}

		public override string ToString()
		{
			return string.Format("[frameCount={0}, stackCount={1}]", Count, ValueCount);
		}
	}
}

