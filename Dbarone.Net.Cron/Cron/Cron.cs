namespace Dbarone.Net.Cron;
using System;
using System.Collections.Generic;
using Dbarone.Net.Extensions.String;

/// <summary>
/// Parses cron expressions, and calculates the next scheduled date for a cron expression.
/// </summary>
public class Cron
{
    #region Cron string fields

    private string Minute { get; set; } = default!;
    private string Hour { get; set; } = default!;
    private string DayOfMonth { get; set; } = default!;
    private string Month { get; set; } = default!;
    private string DayOfWeek { get; set; } = default!;

    #endregion

    #region Date Part Functions

    delegate int DatePartFunction(DateTime dt);
    private DatePartFunction DatePartMonth = (d) => d.Month;
    private DatePartFunction DatePartDay = (d) => d.Day;
    private DatePartFunction DatePartDayOfWeek = (d) => (int)d.DayOfWeek;
    private DatePartFunction DatePartHour = (d) => (int)d.Hour;
    private DatePartFunction DatePartMinute = (d) => (int)d.Minute;

    #endregion

    /// <summary>
    /// Creates a cron record from a 5-part crontab expression.
    /// Note that this requires 6-part format: [min] [hr] [dom] [mon] [dow].
    /// Each of the cron fields only accept the following syntax:
    /// <para/>
    /// BNF syntax is as follows:
    /// <numberNode> ::= <integer>
    /// <rangeNode> ::= <integer>-<integer>
    /// <repeatNode> ::= <integer>/<integer>
    /// <wildcardNode> ::= '*'
    /// <node> ::= <numberNode> | <rangeMode> | <repeatNode> | <wildcardNode>
    /// <expr> ::= <node> | <node>,<node>
    /// </summary>
    /// <param name="s">The crontab string expression.</param>
    /// <returns>A configured, validated Cron instance.</returns>
    public static Cron Create(string s)
    {
        Cron cron = new Cron();
        var args = s.ParseArgs();

        // Need min 5 args.
        if (args.Length < 5)
        {
            throw new Exception("Too few arguments for cron expression.");
        }

        cron.Minute = args[0];
        cron.Hour = args[1];
        cron.DayOfMonth = args[2];
        cron.Month = args[3];
        cron.DayOfWeek = args[4];

        cron.Validate();
        return cron;
    }

    #region Cron nodes

    private IEnumerable<CronElement> nMin = default!;
    private IEnumerable<CronElement> nHr = default!;
    private IEnumerable<CronElement> nDOM = default!;
    private IEnumerable<CronElement> nMon = default!;
    private IEnumerable<CronElement> nDOW = default!;

    #endregion

    #region Validation

    /// <summary>
    /// Validates the string arguments passed in during the cron instantiation.
    /// Throws an exeption if anything is wrong.
    /// </summary>
    private void Validate()
    {
        // process arguments and store in ICronNode lists.
        this.nMin = createElements(Minute, 0, 59);
        this.nHr = createElements(Hour, 0, 23);
        this.nDOM = createElements(DayOfMonth, 1, 31);
        this.nMon = createElements(Month, 1, 12);
        this.nDOW = createElements(DayOfWeek, 0, 6);
    }

    private IEnumerable<CronElement> createElements(string str, int min, int max)
    {
        List<CronElement> result = new List<CronElement>();
        var elements = str.Split(',');
        foreach (var element in elements)
        {
            var cronElement = CronElement.Create(str, min, max);
            result.Add(cronElement);
        }
        return result;
    }

    #endregion

    #region Next() Methods

    /// <summary>
    /// Gets the next scheduled occurence based on the crontab.
    /// If no date in the future is matched, returns null.
    /// </summary>
    /// <param name="startDateTime">Starting date to use.</param>
    /// <returns>The next scheduled occurence.</returns>
    public DateTime? Next(DateTime startDateTime)
    {
        DateTime startDate = startDateTime.Date;

        // Get next date
        DateTime? nextDate = GetDate(startDate);

        // Get next date/time
        if (nextDate==null) {
            return nextDate;
        } else
        {
            if (nextDate.Value.Date == startDate.Date)
                nextDate = startDateTime;
        }

        DateTime? nextDateTime = GetDateTime(nextDate.Value);

        if (nextDate.Value.Date == startDate.Date && nextDateTime == null)
        {
            // if the next date is same as start date
            // and no time match, get the next date (without time)
            // on schedule
            nextDate = GetDate(nextDate.Value.AddDays(1).Date);
            if (nextDate==null) {
                nextDateTime = null;
            } else
            {
                nextDateTime = GetDateTime(nextDate.Value);
            }
        }

        // At this point, if no nextDateTime
        // then null value will automatically be
        // returned.
        return nextDateTime;
    }

    /// <summary>
    /// Gets the next scheduled occurence based on the crontab.
    /// If no date in the future is matched, returns null.
    /// </summary>
    /// <returns>The next scheduled occurence from now.</returns>
    /// <returns>Next occurence of schedule after startDate.</returns>
    public DateTime? Next()
    {
        return Next(DateTime.Now);
    }

    #endregion

    #region Private Methods

    private bool MatchCron(DateTime dateTime, DatePartFunction datePart, IEnumerable<CronElement> cronElements)
    {
        int part = datePart.Invoke(dateTime);

        foreach (var element in cronElements)
        {
            if (element.Validate(part)) {
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// Returns the next date match or DateTime.Now + 1 year, whichever is sooner.
    /// </summary>
    /// <param name="startDate">The start date to check.</param>
    /// <returns></returns>
    private DateTime? GetDate(DateTime startDate)
    {
        while (startDate <= DateTime.Now.AddDays(366))
        {
            if (
                MatchCron(startDate, DatePartMonth, nMon) &&
                MatchCron(startDate, DatePartDay, nDOM) &&
                MatchCron(startDate, DatePartDayOfWeek, nDOW))
                return startDate;

            startDate = startDate.AddDays(1);
        }
        return null;
    }

    /// <summary>
    /// Returns the time within the matched date where
    /// the first match occurs. If no match found on the 
    /// date (for example found on today's date, but
    /// today's schedule already passed), then null value
    /// is returned.
    /// </summary>
    /// <param name="startDateTime">The start date / time.</param>
    /// <returns></returns>
    private DateTime? GetDateTime(DateTime startDateTime)
    {
        DateTime endDateTime = startDateTime;
        while (endDateTime.Date == startDateTime.Date)
        {
            if (
                MatchCron(endDateTime, (d) => { return d.Hour; }, nHr) &&
                MatchCron(endDateTime, (d) => { return d.Minute; }, nMin))
                break;

            endDateTime = endDateTime.AddSeconds(1);
        }
        if (endDateTime.Date == startDateTime.Date)
            // If we're still in the same day, then previous
            // loop successfully broken early.
            return endDateTime;
        else
            // no match for time.
            return null;
    }

    #endregion
}
