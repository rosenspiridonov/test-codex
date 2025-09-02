using System.Text.Json.Serialization;

namespace CinemaSeatService.Api;

public record SeatPlan(
    string Auditorium,
    string FilmTitle,
    string StartTime,
    List<Seat> Seats);

public record Seat(
    string Row,
    [property: JsonPropertyName("seat")] int Number,
    string Status);

public record UpstreamSeatMap(
    string auditorium,
    string filmTitle,
    string startTime,
    Dictionary<string,string> seatRows);

