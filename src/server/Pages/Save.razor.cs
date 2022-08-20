using Microsoft.AspNetCore.Components;
using server.Services;

namespace server.Pages;

public class SaveBase : PageBase
{
    [Parameter] public string? Name { get; set; }
    [Inject] private UserService UserService { get; set; }
    [Inject] private AccountService AccountService { get; set; }

    protected int MaxMoney { get; set; }
    protected int SliderMoney { get; set; }
    protected int MoneyLeftInWallet { get; set; }
    protected int Projection { get; set; }
    protected int Days { get; set; } = 1;
    protected bool Done { get; set; } = false;
    private const float _interest = .04f;

    protected override async Task OnInitializedAsyncCallback()
    {
        var money = await UserService.GetUserAsync(Name);
        MaxMoney = money.Balance;
        Done = true;
        StateHasChanged();
    }

    protected async Task Save()
    {
        // TODO: validate inputs?
        
        await AccountService.TransferToSavingsAsync(SliderMoney, Projection, DateTime.UtcNow.AddDays(Days));
        NotificationCallback("Sparat");
    }

    protected void ValidateDays(string val)
    {
        var value = 0;
        value = string.IsNullOrEmpty(val) ? 1 : int.Parse(val);
        if (value < 1) value = 1;
        if (value > 30) value = 30;
        Days = value;
        Calculate(SliderMoney.ToString());
    }
    protected int Calculate(string val)
    {
        var value = int.Parse(val);
        MoneyLeftInWallet = MaxMoney - value;
        Projection = (int)MathF.Round(value + ((value * _interest) * Days));
        return value;
    }
}