using System;
using System.Text;

namespace Bindables;

public class CodeBuilder
{
	private readonly StringBuilder _builder;

	private string _indentationString;
	private int _indentationLevel;

	public int CurrentOffset => _builder.Length;
	public int CurrentIndentation => _indentationLevel * 4;

	public CodeBuilder()
	{
		_builder = new StringBuilder();

		_indentationString = "";
		_indentationLevel = 0;
	}

	public int Append(string text, bool indent = true)
	{
		int count = text.Length;

		if (indent)
		{
			_builder.Append(_indentationString);
			count += _indentationString.Length;
		}

		_builder.Append(text);

		return count;
	}

	public int AppendLine(string text, bool indent = true)
	{
		int count = text.Length + Environment.NewLine.Length;

		if (indent)
		{
			_builder.Append(_indentationString);
			count += _indentationString.Length;
		}

		_builder.AppendLine(text);

		return count;
	}

	public int AppendLine()
	{
		_builder.AppendLine();
		return Environment.NewLine.Length;
	}

	public int Insert(string text, int offset, bool indent = false)
	{
		int count = text.Length;

		if (indent)
		{
			_builder.Insert(offset, _indentationString);
			count += _indentationString.Length;

			offset += _indentationString.Length;
		}

		_builder.Insert(offset, text);

		return count;
	}

	public void Remove(int offset, int length)
	{
		_builder.Remove(offset, length);
	}

	public void Overwrite(string text, int offset, int length)
	{
		_builder.Remove(offset, length);
		_builder.Insert(offset, text);
	}

	public void Indent()
	{
		_indentationLevel++;
		_indentationString = new string(' ', _indentationLevel * 4);
	}

	public void Unindent()
	{
		_indentationLevel--;
		_indentationString = new string(' ', _indentationLevel * 4);
	}

	public void OpenScope()
	{
		AppendLine("{");
		Indent();
	}

	public void CloseScope()
	{
		Unindent();
		AppendLine("}");
	}

	public override string ToString()
	{
		return _builder.ToString();
	}
}