using SmartMovieBooking.Models;

namespace SmartMovieBooking.Services;

public interface ISeatLayoutFactory
{
    IReadOnlyList<Seat> Build(string showtimeId, decimal basePrice);
}

/// <summary>8 rows × 14 cols, aisle between col 7 and 8.</summary>
public sealed class SeatLayoutFactory : ISeatLayoutFactory
{
    private const int Rows = 8;
    private const int Columns = 14;

    public IReadOnlyList<Seat> Build(string showtimeId, decimal basePrice)
    {
        var rng = new Random(HashCode.Combine(showtimeId));
        var seats = new List<Seat>(Rows * Columns);

        for (int r = 0; r < Rows; r++)
        {
            var tier = r switch { < 3 => SeatTier.Premium, < 6 => SeatTier.Executive, _ => SeatTier.Normal };
            var tierPrice = tier switch
            {
                SeatTier.Premium   => basePrice + 6m,
                SeatTier.Executive => basePrice + 2m,
                _ => basePrice
            };
            var row = ((char)('A' + r)).ToString();

            for (int c = 1; c <= Columns; c++)
            {
                if (c == 8)
                {
                    seats.Add(new Seat { Id = $"{row}-Aisle-{c}", Row = row, Column = c, State = SeatState.Aisle });
                    continue;
                }
                var state = rng.Next(100) < 35 ? SeatState.Occupied : SeatState.Available;
                seats.Add(new Seat
                {
                    Id = $"{row}{c}",
                    Row = row,
                    Column = c,
                    State = state,
                    Tier = tier,
                    Price = tierPrice
                });
            }
        }
        return seats;
    }
}
