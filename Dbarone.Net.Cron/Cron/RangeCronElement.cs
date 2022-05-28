namespace Dbarone.Net.Cron;
using System.Text.RegularExpressions;
using System;

/// <summary>
/// Cron element representing a range of numbers.
/// </summary>
public class RangeCronElement : CronElement
{
    private int Start;
    private int End;

    public override int Priority => 2;

    protected override Regex Pattern => new Regex(@"^(?<start>\d+)\-(?<end>\d+)$");

    public override bool Validate(int value)
    {
        return Start <= value && End >= value;
    }

    protected override void Hydrate(string str, int min, int max)
    {
        var start = int.Parse(this.Match(str).Groups["start"].Value);
        var end = int.Parse(this.Match(str).Groups["end"].Value);
        if (start >= min && start <= max && end >= min && end <= max && start < end){
            this.Start = start;
            this.End = end;
        }
        else
            throw new Exception("RangeCronElement.Hydrate(): Numbers out of range.");
    }
}
