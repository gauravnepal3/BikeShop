using System;
using System.Security.Cryptography;
using System.Text;
using BikeShop.Data.Models;
namespace BikeShop.Data
{
	public class Helper
	{
		private static string _secretKey = "jnPbQwmQ904mishv8MYmWY!6PjAz3c^OP#@W!3Com*O#cBPSas";
		public static string HashPassword(string input,string secretKey)
		{
			secretKey = _secretKey;
			using (var hmac=new HMACSHA512())
			{
				hmac.Key = Encoding.UTF8.GetBytes(secretKey);
				byte[] data = hmac.ComputeHash(Encoding.UTF8.GetBytes(input));
				var sBuilder = new StringBuilder();
				for(int i = 0; i < data.Length; i++)
				{
					sBuilder.Append(data[i].ToString("x2"));
				}
				return sBuilder.ToString();
			}

		}
		//Getting path of directory that stores data of the app
		public static string GetAppDirectoryPath()
		{
			return Path.Combine(
				Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Bike-Shop");
		}

        //Getting path of user file that stores data of the user
        public static string GetAppUsersFilePath()
		{
//Combining main directory path with file name
			return Path.Combine(GetAppDirectoryPath(), "users.json");
		}

		public static string GetAppItemsFilePath()
		{
            //Combining main directory path with file name
            return Path.Combine(GetAppDirectoryPath(), "items.json");
		}
        public static string GetAppTransactionFilePath()
        {
            //Combining parent path with file name
            return Path.Combine(GetAppDirectoryPath(), "transaction.json");
        }

		//Checking role of the user
		public static bool IsAuthorized(Guid userID)
        {
            List<User> users = UsersService.GetAllUsers();
            User user = users.FirstOrDefault(x => x.Id == userID);
			//Confirming logged in user is admin or not
            if (user.Role == Role.Admin)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

//Checking for office hour (9:00AM - 4:00PM)
		public static bool IsOfficeHour()
		{
			TimeSpan officeStart = TimeSpan.Parse("9:00");
			TimeSpan officeEnd = TimeSpan.Parse("16:00");
			DateTime currentTime = DateTime.Now;
			if(officeStart<=currentTime.TimeOfDay && officeEnd >= currentTime.TimeOfDay)
			{
				return true;
			}
			else
			{
				return false;
			}
        }

		//Checking Day of the Week
		public static bool IsSystemAvailable()
		{
			DayOfWeek wk = DateTime.Now.DayOfWeek;
			switch (wk)
			{
				case DayOfWeek.Saturday:
					return false;
				
				case DayOfWeek.Sunday:
					return false;
				//if not Saturday or Sunday
				default:
					if (IsOfficeHour())
					{
						return true;
					}
					else
					{
						return false;
					}
			}
		}

	}
}

