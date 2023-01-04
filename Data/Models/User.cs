using System;
namespace BikeShop.Data.Models
{
    //Model for user data

    public class User
	{
		public Guid Id { get; set; } = Guid.NewGuid();
		public string Username { get; set; }
		public string Password { get; set; }
		public Role Role { get; set; }
		public bool UsingDefaultPassword { get; set; } = true;
		public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
		public Guid CreatedBy { get; set; }
	}
}

