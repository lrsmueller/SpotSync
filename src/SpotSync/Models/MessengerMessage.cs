using Microsoft.AspNetCore.Components;

namespace SpotSync.Models;

public class MessengerMessage
{
    /// <summary>
    /// Key. Used for component pairing during rendering (@key).
    /// </summary>
    public string Key { get; } = Guid.NewGuid().ToString("N");

    /// <summary>
    /// CSS class.
    /// </summary>
    public string CssClass { get; set; }

    /// <summary>
    /// Message title (header).
    /// </summary>
    public string Title { get; set; }

    /// <summary>
    /// Custom message header.
    /// </summary>
    public RenderFragment HeaderTemplate { get; set; }

    /// <summary>
    /// Message text (body).
    /// </summary>
    public string Text { get; set; }

    /// <summary>
    /// Custom message body (content).
    /// </summary>
    public RenderFragment ContentTemplate { get; set; }
}
