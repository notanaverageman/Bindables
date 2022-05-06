namespace Bindables;

public enum CheckResult
{
	Valid,
	Invalid
}

public static class CheckResultExtensions
{
	public static CheckResult Combine(this CheckResult first, CheckResult second)
	{
		if (first == CheckResult.Invalid || second == CheckResult.Invalid)
		{
			return CheckResult.Invalid;
		}

		return CheckResult.Valid;
	}
}