using System;
using System.Collections.Generic;
using System.Linq;

using Expresso.Runtime.Operations;

namespace Expresso.Interpreter
{
	/// <summary>
	/// A stack frame that is used for both evaluating sub-expressions of an AST node and a call stack.
	/// </summary>
	internal sealed class EvaluationFrame
	{
		InterpreterFlowManager manager;

		public AstNode TargetNode{get; private set;}

		public int ScopeFramePointer{get; internal set;}

		/// <summary>
		/// The index of the next child node to be evaluated.
		/// </summary>
		public int ChildCounter{get; internal set;}

		/// <summary>
		/// Indicates the next step of an operation if the TargetNode has multiple operations.
		/// </summary>
		public int StepCounter{get; internal set;}

		public EvaluationFrame(AstNode node, int framePointer, int childCounter, InterpreterFlowManager flowManager)
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
		const int MaxStackSize = 1024;

		List<EvaluationFrame> frames = new List<EvaluationFrame>();

		List<object> stack = new List<object>(MaxStackSize);

		EvaluationFrame cur_scope = null;	//現在のスコープに対応する評価フレーム

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
				throw ExpressoOps.MakeRuntimeError("Memory offset is out of range!");
#endif
			stack[offset] = value;
		}

		internal object GetValue(int offset)
		{
#if DEBUG
			if(offset < 0 || offset > stack.Count)
				throw ExpressoOps.MakeRuntimeError("Memory offset is out of range!");
#endif
			return stack[offset];
		}

		internal void RemoveAt(int offset)
		{
#if DEBUG
			if(offset < 0 || offset > stack.Count)
				throw ExpressoOps.MakeRuntimeError("Memory offset is out of range!");
#endif
			stack.RemoveAt(offset);
		}

		public void Dup(int srcIndex)
		{
#if DEBUG
			if(srcIndex < 0 || srcIndex > stack.Count)
				throw ExpressoOps.MakeRuntimeError("Memory offset is out of range!");
#endif
			stack.Add(stack[srcIndex]);
		}
		#endregion

        /// <summary>
        /// Pushes a new evaluation frame against the specified node.
        /// </summary>
        /// <param name="node">Node.</param>
        /// <param name="childCounter">Child counter.</param>
		public void Push(AstNode node, int childCounter = 0)
		{
            var frame = new EvaluationFrame(node, (cur_scope == null) ? 0 :
                                                  ShouldIntroduceScope(node) ? stack.Count : cur_scope.ScopeFramePointer,
			                                childCounter, this);
			frames.Add(frame);
			if(node is CallExpression)
				cur_scope = frame;
		}

        /// <summary>
        /// Pushes a new evaluation frame without introducing a new scope.
        /// </summary>
        /// <param name="node">Node.</param>
        /// <param name="childCounter">Child counter.</param>
        public void PushWithoutScope(AstNode node, int childCounter = 0)
        {
            var frame = new EvaluationFrame(node, (cur_scope != null) ? cur_scope.ScopeFramePointer : 0, childCounter, this);
            frames.Add(frame);
        }

        /// <summary>
        /// Pushes a value on the stack.
        /// </summary>
        /// <param name="item">Item.</param>
		public void PushValue(object item)
		{
			if(stack.Count == MaxStackSize)
				throw ExpressoOps.MakeRuntimeError("Stack overflow");

			stack.Add(item);
		}

        /// <summary>
        /// Peeks the top evaluation frame.
        /// </summary>
		public EvaluationFrame Top()
		{
			if(frames.Count == 0)
				throw new InvalidOperationException();

			return frames[frames.Count - 1];
		}

        /// <summary>
        /// Peeks the top value on the stack.
        /// </summary>
        /// <returns>The value.</returns>
		public object TopValue()
		{
			if(stack.Count == 0)
				throw new InvalidOperationException();

			return stack[stack.Count - 1];
		}

        /// <summary>
        /// Pops the top evalution frame.
        /// </summary>
        /// <returns>The removed evaluation frame.</returns>
		public EvaluationFrame Pop()
		{
			if(frames.Count == 0)
				throw new InvalidOperationException();

			var tmp = frames[frames.Count - 1];
			frames.RemoveAt(frames.Count - 1);
			if(tmp.TargetNode is ScopeStatement)	//一つ上のスコープに対応する評価フレームをセットする
				cur_scope = ((IEnumerable<EvaluationFrame>)frames).Reverse().FirstOrDefault(x => x.TargetNode is CallExpression);

			return tmp;
		}

        /// <summary>
        /// Pops the top value on the stack.
        /// </summary>
        /// <returns>The value.</returns>
		public object PopValue()
		{
			if(stack.Count == 0)
				throw new InvalidOperationException();

			var tmp = stack[stack.Count - 1];
			stack.RemoveAt(stack.Count - 1);
			return tmp;
		}

        /// <summary>
        /// Determines whether we are currently evaluating the specified node.
        /// That is, we have the specified node in any of the evaluation frames.
        /// </summary>
        /// <returns><c>true</c> if we are evaluating the specified node; otherwise, <c>false</c>.</returns>
        /// <param name="node">Node.</param>
		public bool IsEvaluating(AstNode node)
		{
			return frames.Any(x => x.TargetNode == node);
		}

        private bool ShouldIntroduceScope(AstNode targetNode)
        {
            return targetNode is CallExpression && frames[frames.Count - 1].TargetNode is CallExpression;
        }

		public override string ToString()
		{
			return string.Format("[frameCount={0}, stackCount={1}]", Count, ValueCount);
		}
	}
}

