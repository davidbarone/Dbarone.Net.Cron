namespace Dbarone.Net.Cron;
using System.Text.RegularExpressions;
using System;

/// <summary>
/// Cron element representing the wildcard character '*'.
/// </summary>
public class WildCronElement : CronElement
{
    public override int Priority => 9999;

    protected override Regex Pattern => new Regex(@"^[*]$");

    public override bool Validate(int value)
    {
        return true;
    }

    protected override void Hydrate(string str, int min, int max)
    {
        // Don't need to hydrate anything here. Always succees
    }
}

