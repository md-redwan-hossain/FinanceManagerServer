using System.Text.RegularExpressions;

namespace FinanceManager.Api.Misc;

public class SlugifyParameterTransformer : IOutboundParameterTransformer
{
    public string? TransformOutbound(object? value)
    {
        if (value is string strValue)
        {
            var regex = new Regex("([a-z])([A-Z])", RegexOptions.Compiled & RegexOptions.CultureInvariant,
                TimeSpan.FromMilliseconds(100));

            return regex.Replace(strValue, "$1-$2").ToLowerInvariant().Trim();
        }

        return null;
    }
}
