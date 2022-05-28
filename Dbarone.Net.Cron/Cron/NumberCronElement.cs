namespace Dbarone.Net.Cron;
using System.Text.RegularExpressions;

/// <summary>
/// Cron element representing a single number / value.
/// </summary>
public class NumberCronElement : CronElement
{
    private int Number;

    public override int Priority => 1;

    protected override Regex Pattern => new Regex(@"^(?<num>\d+)$");

    public override bool Validate(int value)
    {
        return value == Number;
    }

    protected override void Hydrate(string str, int min, int max)
    {
        var number = int.Parse(this.Match(str).Groups["num"].Value);
        if (number >= min && number <= max)
            this.Number = number;
        else
            throw new Exception("NumberCronElement.Hydrate(): Number out of range.");
    }
}
