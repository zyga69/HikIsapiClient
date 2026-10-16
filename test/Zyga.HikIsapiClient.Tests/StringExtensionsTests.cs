using Xunit;
using Zyga.HikIsapiClient.ExtensionMethods;

namespace Zyga.HikIsapiClient.Tests;

public class StringExtensionsTests
{
    [Fact]
    public void ToSha256Hash_MatchesKnownVector()
    {
        // Well-known SHA-256("hello") test vector.
        Assert.Equal(
            "2cf24dba5fb0a30e26e83b2ac5b9e29e1b161e5c1fa7425e73043362938b9824",
            "hello".ToSha256Hash());
    }

    [Fact]
    public void ToPasswordHash_Reversible_SingleIteration_ReturnsUnhashedConcatenation()
    {
        // iterations = 1 means the loop body never runs, so the "reversible" path
        // returns the raw password+challenge concatenation.
        var result = "pass".ToPasswordHash("chal", "user", "salt", iterations: 1, isIrreversible: false);

        Assert.Equal("passchal", result);
    }

    [Fact]
    public void ToPasswordHash_Reversible_AdditionalIterations_HashesRepeatedly()
    {
        var expected = "passchal".ToSha256Hash();

        var result = "pass".ToPasswordHash("chal", "user", "salt", iterations: 2, isIrreversible: false);

        Assert.Equal(expected, result);
    }

    [Fact]
    public void ToPasswordHash_Irreversible_HashesUserSaltPasswordThenChallenge()
    {
        var loginHash = "usersaltpass".ToSha256Hash();
        var expected = $"{loginHash}chal".ToSha256Hash();

        var result = "pass".ToPasswordHash("chal", "user", "salt", iterations: 2, isIrreversible: true);

        Assert.Equal(expected, result);
    }

    [Fact]
    public void ToPasswordHash_Irreversible_AdditionalIterations_HashesRepeatedly()
    {
        var loginHash = "usersaltpass".ToSha256Hash();
        var expected = $"{loginHash}chal".ToSha256Hash().ToSha256Hash();

        var result = "pass".ToPasswordHash("chal", "user", "salt", iterations: 3, isIrreversible: true);

        Assert.Equal(expected, result);
    }
}
