using Abgerechnet.Core.Model;

namespace Abgerechnet.Tests;

public class IbanTests
{
    [Theory]
    [InlineData("DE89370400440532013000")]
    [InlineData("DE89 3704 0044 0532 0130 00")]
    [InlineData("de89 3704 0044 0532 0130 00")]
    [InlineData("AT611904300234573201")]
    [InlineData("CH9300762011623852957")]
    [InlineData("GB29NWBK60161331926819")]
    public void ValidIbans_AreAccepted(string iban)
    {
        Assert.True(Iban.IsValid(iban));
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    [InlineData("DE88370400440532013000")]   // wrong check digits
    [InlineData("DE89370400440532013001")]   // typo in the account number
    [InlineData("DE8937040044053201300")]    // one digit missing
    [InlineData("DE89 3704 0044 0532 0130 000")] // one digit too many
    [InlineData("1289370400440532013000")]   // no country code
    [InlineData("DEXX370400440532013000")]   // letters instead of check digits
    public void InvalidIbans_AreRejected(string? iban)
    {
        Assert.False(Iban.IsValid(iban));
    }

    [Fact]
    public void Format_GroupsByFour()
    {
        Assert.Equal("DE89 3704 0044 0532 0130 00", Iban.Format("de89370400440532013000"));
        Assert.Equal("DE89 3704 0044 0532 0130 00", Iban.Format(" DE89-3704 0044 0532 013000 "));
        Assert.Equal(string.Empty, Iban.Format(null));
    }
}
