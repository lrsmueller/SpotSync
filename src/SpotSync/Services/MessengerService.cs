using SpotSync.Interfaces;
using SpotSync.Models;

namespace SpotSync.Services;

public class MessengerService : IMessageService
{
	public event Action<MessengerMessage> OnMessage;
	public event Action OnClear;

	public void AddMessage(MessengerMessage message)
	{
		OnMessage.Invoke(message);
	}

	public void Clear()
	{
		OnClear.Invoke();
	}
}
