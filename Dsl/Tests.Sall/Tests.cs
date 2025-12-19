using Sall;
using Utils.Tests;
using Xunit.Abstractions;

namespace TestProject1;

public class Tests
{
    public Tests(ITestOutputHelper output)
    {
        Output = output;
    }

    public ITestOutputHelper Output { get; }

    [Theory]
    [InlineData("syntax.sall", "syntax.sall.txt")]
    public void SnapshotTest(string inputFileName, string snapshotFileName)
    {
        var expected = DataAccess.ReadTestDataSnapshots(snapshotFileName);
        var actual = ToAstString(inputFileName);
        Assert.Equal(expected, actual);
    }

    private static string ToAstString(string inputFileName)
    {
        return Serializer.Serialize(SallVisitor.Visit(DataAccess.ReadTestDataInput(inputFileName)));
    }
}
