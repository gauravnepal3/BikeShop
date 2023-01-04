using System;
using BikeShop.Data.Models;
using System.Text.Json;
namespace BikeShop.Data
{
	public class UsersService
	{
		//Generating seed user for very first use of app
		public const string SeedUsername = "admin";
		public const string SeedPassword = "admin";

		//Saving all the users data to a file
		public static void SaveUser(List<User> users)
		{
			string appDataDirectoryPath = Helper.GetAppDirectoryPath();
			string appUsersFilePath = Helper.GetAppUsersFilePath();
			if (!Directory.Exists(appDataDirectoryPath))
			{
				Directory.CreateDirectory(appDataDirectoryPath);
			}
			//Converting the list to json before saving records to the file
			var userJson = JsonSerializer.Serialize(users);
			File.WriteAllText(appUsersFilePath, userJson);
		}

        //Reading all users for file
        public static List<User> GetAllUsers()
		{
            string appUsersFilePath = Helper.GetAppUsersFilePath();
			if (!File.Exists(appUsersFilePath))
			{
				return new List<User>();
			}
			else
			{
				var userJson = File.ReadAllText(appUsersFilePath);
                //converting json to List after reading from file
                return JsonSerializer.Deserialize<List<User>>(userJson);
			}
        }

		//Getting number of admin in the system
		public static int GetAdminInSystem()
		{
			List<User> users = GetAllUsers();
			int userCount= users.FindAll(x => x.Role == Role.Admin).Count;
			return userCount;
				
		}

		//Adding new user to the system
		public static List<User> CreateUser(Guid userID, string username, string password, Role role)
		{
			List<User> users = GetAllUsers();
			bool usernameExists = users.Any(x => x.Username == username);
			//Checking if username is already taken
            if (usernameExists)
			{
				throw new Exception("Username already taken.");

			}
			else
			{
				if ((role == Role.Admin))
				{
					int adminCount = GetAdminInSystem();
					//Counting number of admins in the system
					if(adminCount < 2)
					{
                        users.Add(
                                        new User
                                        {
                                            Username = username,
                                            Password = Helper.HashPassword(password, ""),
                                            Role = role,
                                            CreatedBy = userID
                                        }
                                        );
                        SaveUser(users);
                        return users;
					}
					else
					{
						throw new Exception("System can only have 2 admins");
					}
				}
				else
				{
                    users.Add(
                                      new User
                                      {
                                          Username = username,
                                          Password = Helper.HashPassword(password, ""),
                                          Role = role,
                                          CreatedBy = userID
                                      }
                                      );
                    SaveUser(users);
                    return users;
                }
			
			}
		}
		//Creating seed user for first login
		public static void SeedUsers()
		{
			var users = GetAllUsers().FirstOrDefault(x => x.Role == Role.Admin);
			if (users == null)
			{
				CreateUser(Guid.Empty, SeedUsername, SeedPassword, Role.Admin);
			}
		}

		//Logging in the user
		public static User Login(string username,string password)
		{
//Checking provided username and password
			if (!(string.IsNullOrEmpty(username)) && !(string.IsNullOrEmpty(password)))
				{
			List<User> users = GetAllUsers();
			User user = users.FirstOrDefault(x => x.Username == username);
				//Checking if username exists in the system
			if (user == null)
			{
				throw new Exception("No account found on that username");
			}
//Checking if password match for the username
				if (!(Helper.HashPassword(password, "") == user.Password))
			{
				throw new Exception("Invalid username or Password");
			}
			return user;
			}
			else
			{
				throw new Exception("Username and Password cannot be empty");
			}
		}

		//Changing Password of logged in user
		public static User ChangePassword(Guid id, string currentPassword,string newPassword, string confirmPassword)
		{

			if (newPassword != confirmPassword)
			{
				throw new Exception("Confirm Password doesnot match");
			}
			if (newPassword == currentPassword)
			{
				throw new Exception("New Password must be different from old password");
			}
			List<User> users = GetAllUsers();
			User user = users.FirstOrDefault(x => x.Id == id);
			//Checking if user exists
			if (user == null)
			{
				throw new Exception("User not found");
			}
			else
			{
				//Checking provided password with saved hash password
                if (!(Helper.HashPassword(currentPassword, "") == user.Password))
                {
                    throw new Exception("Invalid current password");
				}
					string hasedPassword = Helper.HashPassword(newPassword, "");
					user.Password = hasedPassword;
					user.UsingDefaultPassword = false;
					SaveUser(users);
					return user;
            }
		}

		//Getting user info individually
		public static User GetUserById(Guid id)
		{
			List<User> users = GetAllUsers();
			User user = users.FirstOrDefault(x => x.Id == id);
			return user;
		}

		//Deleteing user from the system
        public static List<User> Delete(Guid id)
        {
            List<User> users = GetAllUsers();
            User user = users.FirstOrDefault(x => x.Id == id);
			//Checking if user exist
            if (user == null)
            {
                throw new Exception("User not found.");
            }
            users.Remove(user);
            SaveUser(users);
            return users;
        }
    }
}

