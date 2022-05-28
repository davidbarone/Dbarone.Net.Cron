namespace Dbarone.Net.Cron.Tests;
using Xunit;
using System;
using System.Collections.Generic;

public class CronTests
{
    [Theory]
    [InlineData("* * * * *")]
    [InlineData("0 * * * *")]
    [InlineData("59 * * * *")]
    [InlineData("0-10 * * * *")]
    [InlineData("0/2 * * * *")]
    [InlineData("* 0 * * *")]
    [InlineData("* 23 * * *")]
    [InlineData("* 0-10 * * *")]
    [InlineData("* 0/2 * * *")]
    [InlineData("* * 1 * *")]
    [InlineData("* * 31 * *")]
    [InlineData("* * 1-10 * *")]
    [InlineData("* * 1/2 * *")]
    [InlineData("* * * 1 *")]
    [InlineData("* * * 12 *")]
    [InlineData("* * * 1-10 *")]
    [InlineData("* * * 1/2 *")]
    [InlineData("* * * * 0")]
    [InlineData("* * * * 6")]
    [InlineData("* * * * 5-6")]
    [InlineData("* * * * 0/2")]
    public void Create_WhenPassedValidCron_CreatesInstance(string value)
    {
        // Arrange

        // Act
        var cron = Cron.Create(value);

        // Assert
        Assert.NotNull(cron);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("* * * *")]         // Too few parameters
    [InlineData("60 * * * *")]      // Minutes should be 0-59
    [InlineData("-1 * * * *")]
    [InlineData("a * * * *")]
    [InlineData("* 24 * * *")]      // Hours should be 0-23
    [InlineData("* -1 * * *")]
    [InlineData("* a * * *")]
    [InlineData("* * 32 * *")]      // DoM should be 1-31
    [InlineData("* * -1 * *")]
    [InlineData("* * a * *")]
    [InlineData("* * * 13 *")]      // Month should be 1-12
    [InlineData("* * * -1 *")]
    [InlineData("* * * a *")]
    [InlineData("* * * * 7")]       // DoW should be 0-7
    [InlineData("* * * * -1")]
    [InlineData("* * * * a")]
    public void Create_WhenPassedInvalidCron_ThrowsException(string value)
    {
        // Arrange

        // Act
        var exception = Record.Exception(() => Cron.Create(value));

        // Assert
        Assert.NotNull(exception);
    }

    public class NextTestTheoryData
    {
        public NextTestTheoryData(string startDt, string cron, string expectedNextDt)
        {
            this.StartDt = DateTime.Parse(startDt);
            this.Cron = cron;
            this.ExpectedNextDt = expectedNextDt == "" ? null : DateTime.Parse(expectedNextDt);
        }

        public DateTime StartDt { get; set; }
        public string Cron { get; set; }
        public DateTime? ExpectedNextDt { get; set; }
    }

    public static IEnumerable<object[]> NextTheoryData
    {
        get
        {
            var sampleDataList = new NextTestTheoryData[]
            {
                    new NextTestTheoryData("01-JAN-2000", "* * * * *", "01-JAN-2000 00:00:00"),
                    new NextTestTheoryData("01-JAN-2000", "0 * * * *", "01-JAN-2000 00:00:00"),
                    new NextTestTheoryData("01-JAN-2000", "6 * * * *", "01-JAN-2000 00:06:00"),
                    new NextTestTheoryData("01-JAN-2000", "10/2 * * * *", "01-JAN-2000 00:10:00"),
                    new NextTestTheoryData("01-JAN-2000", "0 2 * * *", "01-JAN-2000 02:00:00"),
                    new NextTestTheoryData("01-JAN-2000", "* 2 * * *", "01-JAN-2000 02:00:00"),
                    new NextTestTheoryData("01-JAN-2000", "30 2 * * *", "01-JAN-2000 02:30:00"),
                    new NextTestTheoryData("01-JAN-2000", "* * 5 * *", "05-JAN-2000 00:00:00"),
                    new NextTestTheoryData("01-JAN-2000", "0 0 5 * *", "05-JAN-2000 00:00:00"),
                    new NextTestTheoryData("01-JAN-2000", "45 13 5 * *", "05-JAN-2000 13:45:00"),
                    new NextTestTheoryData("01-JAN-2000", "45 13 5 5 *", "05-MAY-2000 13:45:00"),
                    new NextTestTheoryData("01-JAN-2000", "45 13 5 5/2 *", "05-MAY-2000 13:45:00"),
                    new NextTestTheoryData("01-JAN-2000", "45 13 * * 5", "07-JAN-2000 13:45:00"),
                    new NextTestTheoryData("01-JAN-2000", "45 13-15 * 11 5", "03-NOV-2000 13:45:00"),
                    new NextTestTheoryData("01-JAN-2000", "* * 30 2 *", "") // 30-Feb is invalid, and should not match any date.
            };

            // xUnit expected specific return type
            var retVal = new List<object[]>();
            foreach (var sampleData in sampleDataList)
            {
                //Add the strongly typed data to an array of objects with one element. This is what xUnit expects.
                retVal.Add(new object[] { sampleData });
            }
            return retVal;
        }
    }

    [Theory]
    [MemberData(nameof(NextTheoryData))]
    public void Next(NextTestTheoryData data)
    {
        // Arrange

        // Act
        var c = Cron.Create(data.Cron);
        var nextDt = c.Next(data.StartDt);

        // Assert
        if (data.ExpectedNextDt != null)
        {
            Assert.NotNull(nextDt);
            Assert.IsType<DateTime>(nextDt);
            Assert.Equal<DateTime>(data.ExpectedNextDt.Value, nextDt!.Value);
        } else {
            Assert.Null(nextDt);
        }
    }
}