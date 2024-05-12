using Microsoft.AspNetCore.Components;
using SpotSync.Interfaces;
using SpotSync.Models;

namespace SpotSync.Components.Messages;

public partial class Messenger : ComponentBase, IDisposable
{
	/// <summary>
	/// Position of the messages. The default value is <see cref="ToastContainerPosition.None"/>.
	/// </summary>
	//[Parameter] public ToastContainerPosition Position { get; set; } = ToastContainerPosition.None;

	/// <summary>
	/// Additional CSS class.
	/// </summary>
	[Parameter] public string CssClass { get; set; }

	[Inject] protected IMessageService MessageService { get; set; }
	[Inject] protected NavigationManager NavigationManager { get; set; }

	private List<MessengerMessage> _messages = new List<MessengerMessage>();

	protected override void OnInitialized()
	{
		MessageService.OnMessage += HandleMessage;
		MessageService.OnClear += HandleClear;
	}

	private void HandleMessage(MessengerMessage message)
	{
		_ = InvokeAsync(() =>
		{
			_messages.Add(message);

			StateHasChanged();
		});
	}

	private void HandleClear()
	{
		_ = InvokeAsync(() =>
		{
			_messages.Clear();

			StateHasChanged();
		});
	}

	/// <summary>
	/// Receive notification from <see cref="HxToast"/> when the message is hidden.
	/// </summary>
	private void HandleToastHidden(MessengerMessage message)
	{
		_messages.Remove(message);
	}

	public void Dispose()
	{
		Dispose(true);
	}

	protected virtual void Dispose(bool disposing)
	{
		if (disposing)
		{
			MessageService.OnMessage -= HandleMessage;
			MessageService.OnClear -= HandleClear;
		}
	}
}