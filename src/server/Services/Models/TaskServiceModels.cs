namespace server.Services.Models;

public record CreateTask(string Description, int Payout, bool Daily, string ImageUrl, int? SpecificUserId,
    uint DayOfTheWeek, bool Once = false);

/*
 * string description, int payout, bool daily, string imageUrl,
        int? specificUserId, 
        uint dayOfTheWeek
 */