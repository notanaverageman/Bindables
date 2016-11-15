using System;

namespace Bindables.Fody
{
	public class Range : IComparable<Range>
	{
		public int Start { get; }
		public int End { get; }

		public Range(int start, int end)
		{
			if (end < start)
			{
				throw new ArgumentException("End cannot be less that start.");
			}

			Start = start;
			End = end;
		}

		public int CompareTo(Range other)
		{
			return Start - other.Start;
		}
	}
}