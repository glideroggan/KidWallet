using Microsoft.AspNetCore.Components;

namespace server.Shared.Components;

public class MyRadioButtonBase : ComponentBase
{
    // TODO: make this work more like the dropdown, we need special items here too then
    [Parameter] public List<string> Options { get; set; }
    [Parameter] public RenderFragment ChildContent{get;set;}
    [Parameter] public EventCallback<string> OnSelected {get;set;}
    
    public async Task HandleSelect(ChangeEventArgs args)
    {
        await OnSelected.InvokeAsync(args.Value.ToString());
    }
}