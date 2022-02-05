using Microsoft.AspNetCore.Components;
using server.Components;
using server.Data.Task;
using server.Data.User;
using server.Services;


namespace server.Pages;

public class CreateTaskBase : ComponentBase
{
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    [Inject] private UserService _userService { get; set; }
    [Inject] private TaskService _taskService { get; set; }
    [Inject] private AppState _state { get; set; }
    [Inject] private IWebHostEnvironment webHostEnvironment { get; set; }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

    protected List<string> items = new() { "roger", "Sylwia", "Sex" };

    [CascadingParameter] private Action<string> NotificationCallback { get; set; }

    protected bool monday { get; set; }
    protected bool tuesday { get; set; }
    protected bool wednesday { get; set; }
    protected bool thursday { get; set; }
    protected bool friday { get; set; }
    protected bool saturday { get; set; }
    protected bool sunday { get; set; }
    
    protected List<string> children = new();
    protected bool initialized;
    private string? _targetName = "Alla";
    private List<UserModel> childList;

    protected string Description { get; set; }
    protected int Payout { get; set; }

    protected bool Daily { get; set; }
    
    protected TaskModel CardModel { get; set; }
    protected List<string> images;
    private string ImageSelected;
    private DaysEnum _weekDay;

    protected void OnChangePayout()
    {
        CardModel.Payout = Payout;
    }

    protected void OnSelected(string selection)
    {
        ImageSelected = Path.Combine("assets/", selection);
        CardModel.ImageUrl = ImageSelected;
    }
    
    protected void OnChangeDescription()
    {
        CardModel.Description = Description;
    }

    protected void WeekDayChanged(ChangeEventArgs args)
    {
        _weekDay = (DaysEnum)Enum.Parse(typeof(DaysEnum), args.Value.ToString(), true);
        CardModel.DayInTheWeek = _weekDay;
    }
    
    protected void WhoChanged(ChangeEventArgs args)
    {
        _targetName = args.Value.ToString();
        CardModel.TargetUser = childList.FirstOrDefault(c => c.Name == _targetName);
    }
    protected async Task CreateTaskAsync()
    {
        var dayArr = new[]
        {
            monday, tuesday, wednesday, thursday, friday, saturday, sunday
        };
        uint dayOfTheWeek = 0;
        dayOfTheWeek = Daily ? DayAndWeekHelper.Encoder(dayArr) : DayAndWeekHelper.Encoder(_weekDay);
        await _taskService.CreateNewAsync(Description, Payout, Daily,
            ImageSelected,
            childList.FirstOrDefault(c => c.Name == _targetName)?.Id,
            dayOfTheWeek);


        // show notification about that it is saved
        NotificationCallback("Saved");
    }

    protected void WhenChanged(ChangeEventArgs args)
    {
        Daily = args.Value.ToString() == "Day";
    }

    protected override async Task OnInitializedAsync()
    {
        var imageFiles = Directory.GetFiles(Path.Combine(webHostEnvironment.WebRootPath, "assets")).ToList();
        images = imageFiles.Select(x => Path.GetFileName(x)).ToList();

        CardModel = new TaskModel();
        childList = await _userService.GetChildrenAsync(_state.User.Id);
        foreach (var child in childList)
        {
            children.Add(child.Name);
        }
        children.Add("Alla");
        initialized = true;
    }


}