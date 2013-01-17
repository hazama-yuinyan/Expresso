using System;

namespace Expresso.Runtime
{
	public interface ICodeFormattable
	{
		string Repr(CodeContext context);
	}
}

