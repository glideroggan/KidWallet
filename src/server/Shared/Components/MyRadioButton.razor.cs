#nullable enable
using Microsoft.AspNetCore.Components;

namespace server.Shared.Components;

public class MyRadioButtonBase : ComponentBase
{
    [Parameter] public string Id { get; set; }
    [Parameter] public List<string> Options { get; set; }
    [Parameter] public RenderFragment ChildContent{get;set;}
    [Parameter] public EventCallback<string> OnSelected {get;set;}

    public async Task HandleSelect(ChangeEventArgs args) => await OnSelected.InvokeAsync(args.Value.ToString());
}