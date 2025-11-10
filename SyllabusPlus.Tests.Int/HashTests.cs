
using FluentAssertions;
using SyllabusPlusPanopto.Domain;
using Xunit;

public class HashTests
{
    [Fact]
    public void Integration_placeholder()
    {
        var a = RowHash.Compute("A","2025-10-21T09:00:00Z","R1","REC");
        var b = RowHash.Compute("A","2025-10-21T09:00:00Z","R1","REC");
        a.Should().Be(b);
    }
}
