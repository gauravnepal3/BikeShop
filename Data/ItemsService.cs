using System;
using BikeShop.Data.Models;
using System.Text.Json;
namespace BikeShop.Data
{
	public class ItemsService
	{
		//Saving all items transaction in file
		public static void SaveItem(List<InventoryItem> items)
		{
			string appDataDirectoryPath = Helper.GetAppDirectoryPath();
			string appItemsFilePath = Helper.GetAppItemsFilePath();
			if (!Directory.Exists(appDataDirectoryPath))
			{
				Directory.CreateDirectory(appDataDirectoryPath);
			}
			//converting List to json before saving
			var itemJson = JsonSerializer.Serialize(items);
			File.WriteAllText(appItemsFilePath, itemJson);
		}

        //Reading all items active transaction for file
        public static List<InventoryItem> GetAllItems()
		{
			string itemsFilePath = Helper.GetAppItemsFilePath();
			if (!File.Exists(itemsFilePath))
			{
				return new List<InventoryItem>();
			}
			var itemJson = File.ReadAllText(itemsFilePath);
            //converting json to List after reading from file
            return JsonSerializer.Deserialize<List<InventoryItem>>(itemJson);
		}

		//Getting Individual Items details using ID
		public static InventoryItem GetItemById(Guid id)
		{
			List < InventoryItem > items= GetAllItems();
			InventoryItem item = items.FirstOrDefault(x => x.Id == id);
			return item;

		}

		//Adding Quantity to existing items
		public static InventoryItem AddQuantity(Guid userID, Guid itemID,int quantity)
		{
			//Checking role of user trying to add item
			if (Helper.IsAuthorized(userID))
			{
				List<InventoryItem> items = GetAllItems();
				InventoryItem item = items.FirstOrDefault(x => x.Id == itemID);
				//Checking if item with ID exists or not
				if (item == null)
				{
					throw new Exception("Item with that ID not found");
				}
				else
				{
					//Adding quantity to existing quantity
					item.Quantity = item.Quantity + quantity;
					item.LastTransactionDate = DateTime.UtcNow;
					//Updating item and saving the transaction 
					SaveItem(items);
					TransactionService.AddTranasction(itemID, quantity, userID, TransactionType.Insert);
                    return item;
				}

			}
			else
			{
				throw new Exception("Unauthorized Action!!");
			}
		}

        //Removing Quantity of existing items
        public static InventoryItem RemoveQuantity(Guid itemID,int quantity)
		{
			//Getting All items 
			List<InventoryItem> items = GetAllItems();
			InventoryItem item = items.FirstOrDefault(x=>x.Id==itemID);
			//Checking if item exists or not
			if (item != null)
			{
				//Deducting quantity of the item
				item.Quantity = item.Quantity - quantity;
				item.LastTransactionDate = DateTime.UtcNow;
				SaveItem(items);
				return item;
			}
			else
			{
				throw new Exception("Item with ID not found");
			}
		
		}

		//Getting all items by its stock
		public static List<InventoryItem> GetItemByStock(string stock)
		{
            List<InventoryItem> items = GetItemsByAvailablity(true);
			//Checking stock type
            if (stock == "IN")
			{
				List<InventoryItem> inStock = items.FindAll(x => x.Quantity > x.MinimumRequiredQuantity);
				return inStock;
			}
			else if(stock == "LOW")
			{
                List<InventoryItem> lowStock = items.FindAll(x => x.Quantity <= x.MinimumRequiredQuantity);
				return lowStock;
			}
			else 
			{
				return items;
			}
		}


		public static List<InventoryItem> GetItemsByAvailablity(Boolean isActive)
		{
			List<InventoryItem> items = GetAllItems();
			List<InventoryItem> itemByAvailabilty = items.FindAll(x => x.isActive == isActive);
			return itemByAvailabilty;
		}
		//Adding new items to the system
		public static List<InventoryItem> AddItem(Guid userID,string productName, int openingStock,int minimumRequiredStock)
		{
			//Checking role of user trying to add new item
			if (Helper.IsAuthorized(userID))
			{
				//Checking if input is empty
				if (!(string.IsNullOrEmpty(productName)))
				{
				List<InventoryItem> items = GetAllItems();
				bool itemNameTaken = items.Any(x => x.ItemName == productName);
					//Checking for duplicate entry
				if (itemNameTaken)
				{
					throw new Exception("Item with same name already exists");
				}
				else
				{
					items.Add(
							new InventoryItem
							{
								ItemName = productName,
								Quantity = openingStock,
								MinimumRequiredQuantity = minimumRequiredStock,
								CreatedBy=userID

							}
						);
					//Saving details of item along with its transaction
					SaveItem(items);
					Guid insertID = items.LastOrDefault().Id;
					TransactionService.AddTranasction(insertID, openingStock, userID, TransactionType.Insert);
					return items;

				}
				}
				else
				{
					throw new Exception("Product Name cannot be empty");
				}
			}
			else
			{
				throw new Exception("Unauthorized aciton! Admin can only add items");
			}
		}
		public static List<InventoryItem> DeleteToogle(Guid itemID)
		{
			List<InventoryItem> items = GetAllItems();
			InventoryItem item = items.FirstOrDefault(x => x.Id == itemID);
			if (item == null)
			{
                throw new Exception("Item not found.");
            }
			List<ItemTransaction> transactions = TransactionService.GetAllTransactions().FindAll(x => x.ItemID == itemID).FindAll(x => x.TransactionStatus == Status.Pending);
			if (transactions.Count>0)
			{
				throw new Exception("Pending Approval for the item was found. Please clear the approval.");
			}
            item.isActive = !item.isActive;
			SaveItem(items);
			items.Remove(item);
			if (items.Count > 0)
			{
			return items;
			}
			else
			{
				return new List<InventoryItem>();
			}

		}
	}

}

