using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace SpotSync.Data;

// Add profile data for application users by adding properties to the ApplicationUser class
public class ApplicationUser : IdentityUser
{
	//Sync 
	[Range(0, 23)]
	public int RefreshHour { get; set; }

	public DateTimeOffset LastRefresh { get; set; }

	public string SelectedPlaylist { get; set; }
}
