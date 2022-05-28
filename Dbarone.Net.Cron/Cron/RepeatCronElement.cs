namespace Dbarone.Net.Cron;
using System.Text.RegularExpressions;
using System;

/// <summary>
/// Cron element representing a repeating set of values. For example, 1/5 represents 'repeat every 5 units, starting at 1'.
/// </summary>
public class RepeatCronElement : CronElement
{
    public int Start;
    public int Repeat;

    public override int Priority => 3;

    protected override Regex Pattern => new Regex(@"^(?<start>\d+)\/(?<repeat>\d+)$");

    public override bool Validate(int value)
    {
        return (value >= Start) && ((value - Start) % Repeat == 0);
    }

    protected override void Hydrate(string str, int min, int max)
    {
        var start = int.Parse(this.Match(str).Groups["start"].Value);
        var repeat = int.Parse(this.Match(str).Groups["repeat"].Value);
        if (start >= min && start <= max && repeat >= 1 && repeat <= max) {
            this.Start = start;
            this.Repeat = repeat;
        }
        else
            throw new Exception("RepeatCronElement(): Numbers out of range.");
    }
}
