using System.Globalization;

namespace CinemaSeatService.Api;

public static class SeatMapper
{
    public static SeatPlan Map(UpstreamSeatMap upstream)
    {
        var start = DateTimeOffset.FromUnixTimeSeconds(long.Parse(upstream.startTime, CultureInfo.InvariantCulture))
            .ToLocalTime().ToString("HH:mm");
        var seats = new List<Seat>();
        foreach (var row in upstream.seatRows)
        {
            for (int i = 0; i < row.Value.Length; i++)
            {
                var status = row.Value[i] == '1' ? "booked" : "available";
                seats.Add(new Seat(row.Key, i + 1, status));
            }
        }
        return new SeatPlan(upstream.auditorium, upstream.filmTitle, start, seats);
    }
}
