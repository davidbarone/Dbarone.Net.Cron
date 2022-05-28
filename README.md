# Dbarone.Net.Cron
A simple .NET cron library.

## Cron and Crontab
The cron command-line utility (cron job) is a job scheduler on Unix-like systems. Actions in cron are driven by a simple `crontab` (a table of cron values).

This utility provides a simple way in .NET to use basic 5-part crontabs, and get information about the next datetime calculated for the crontab. This library could be easily incorporated into a broader scheduling subsystem.

Refer to https://en.wikipedia.org/wiki/Cron for more information about the cron utility.

In this library, the crontab is defined using a standard 5-part CRON string:

| Part | Description            | Allowed Values    |
| ---- | ---------------------- | ----------------- |
| MIN  | The minute of the hour | 0 - 59            |
| HOUR | The hour of the day    | 0 - 24            |
| DOM  | The day of the month   | 1 - 31            |
| MON  | The month of the year  | 1 - 12            |
| DOW  | The day of the week    | 0 (Sun) - 6 (Sat) |

Each part of the 5-part crontab string can accept the following types of value:

- Single numeric value, for example '10'.
- List or array of numbers, for example: '0,10,20,30,40,50' in the MIN field would mean every 10 minutes within the hour.
- A range of numbers, for example '16-20' in the HOUR field would mean from 4pm to 8pm.
- A repeating patten, for example '1/5' in the HOUR field would mean start at 1am and repeat every 5 hours (0100, 0600, 1100, 1600 2100).
- Wildcard: a '*' in any field represents all possible values.

Some example CRON strings are:

| CRON      | Meaning                                                    |
| --------- | ---------------------------------------------------------- |
| 0 0 1 1 * | Run once a year at midnight on 1 Jan                       |
| 0 0 1 * * | Run once a month at midnight on the first day of the month |
| 0 0 * * 0 | Run once a week at midnight on Sunday morning              |
| 0 0 * * * | Run once a day at midnight                                 |
| 0 * * * * | Run every hour on the hour                                 |
| * * * * * | Run every minute                                           |

## Using the library

The library is very simple to use, an example is shown below:

``` c#
    // Create cron instance, passing in crontab.
    var cron = Cron.Create("0 0 * * 0");

    // Calculate next Datetime from now.
    var next = cron.Next();
```