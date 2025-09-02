using CinemaSeatService.Api;
using Xunit;

public class SeatMapperTests
{
    [Fact]
    public void MapsSeatsAndStartTime()
    {
        var upstream = new UpstreamSeatMap(
            "Main-Hall",
            "Space Odyssey",
            "1753804800",
            new Dictionary<string, string> { ["A"] = "10" });

        var plan = SeatMapper.Map(upstream);

        var expectedTime = DateTimeOffset.FromUnixTimeSeconds(1753804800).ToLocalTime().ToString("HH:mm");
        Assert.Equal(expectedTime, plan.StartTime);

        Assert.Contains(plan.Seats, s => s.Row == "A" && s.Number == 1 && s.Status == "booked");
        Assert.Contains(plan.Seats, s => s.Row == "A" && s.Number == 2 && s.Status == "available");
    }
}
