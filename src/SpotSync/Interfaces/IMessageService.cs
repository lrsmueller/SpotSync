using SpotSync.Models;

namespace SpotSync.Interfaces;

public interface IMessageService
{
	/// <summary>
	/// Subscription seam for the HxMessenger component to be able to receive messages.
	/// </summary>
	public event Action<MessengerMessage> OnMessage;

	/// <summary>
	/// Subscription seam for the HxMessenger component to be able to receive the Clear() command.
	/// </summary>
	public event Action OnClear;

	/// <summary>
	/// Adds and shows a message.
	/// </summary>
	public void AddMessage(MessengerMessage message);

	/// <summary>
	/// Removes all messages.
	/// </summary>
	public void Clear();
}
