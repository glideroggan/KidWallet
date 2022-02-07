using System.Diagnostics;
using Microsoft.AspNetCore.Components;
using server.Components;

namespace server.Shared
{
	public class MainLayoutBase : LayoutComponentBase, IDisposable
	{
#pragma warning disable CS8618
        [Inject] protected AppState State { get; private set; } = null!;
        [Inject] private NavigationManager Nav { get; set; }
#pragma warning restore CS8618

        protected NotificationMessage? Message { get; set; }

        protected Action<string> NotificationCallback { get; set; }

        private Action _handler = null!;

        protected override Task OnInitializedAsync()
        {
            NotificationCallback = str =>
            {
                Message = new NotificationMessage(str);
                InvokeAsync(() => StateHasChanged());
            };
            return Task.CompletedTask;
        }

        protected override void OnAfterRender(bool firstRender)
        {
            if (firstRender)
            {
                _handler = StateChange;
                State.OnChange += _handler;
            }
        }
        
        private void StateChange()
        {
            InvokeAsync(() => StateHasChanged());
        }
        
        public void Dispose()
        {
            _handler = StateChange;
            State.OnChange -= _handler;
        }

        protected void Logout()
        {
            State.User = null;
            Nav.NavigateTo("/");
            State.NotifyStateChanged();
        }
    }
}
