using server.Services;

namespace server.Data.Task
{
    public enum StatusEnum
    {
        None = 0,
        Available,
        OnGoing,
        WaitingForApproval,
        Payout,
    }
    public class TaskModel
    {
        public int Id { get; set; }
        public User.UserModel? User { get; set; }
        public User.UserModel? TargetUser { get; set; }
        public StatusEnum Status { get; set; }
        public string? Description { get; set; }
        public string? ImageUrl { get; set; }
        public int Payout { get; set; }
        public DaysEnum DayInTheWeek { get; set; }
        public DateTime NotBefore { get; set; }
        public DateTime Created { get; set; }
    }
}
