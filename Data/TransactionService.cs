using System;
using BikeShop.Data.Models;
using System.Text.Json;
using System.Linq;

namespace BikeShop.Data
{
	public class TransactionService
	{
		//Saving all transaction to transactions.json
		public static void SaveTransaction(List<ItemTransaction> transactions)
		{
			string appDataDirectoryPath = Helper.GetAppDirectoryPath();
			string appTransactionsFilePath = Helper.GetAppTransactionFilePath();
			//Checking for app directory
			if (!Directory.Exists(appDataDirectoryPath))
			{
				Directory.CreateDirectory(appDataDirectoryPath);
			}
			//Converting List to json before saving
			var transactionJson = JsonSerializer.Serialize(transactions);
			File.WriteAllText(appTransactionsFilePath, transactionJson);
		}
		public static List<ItemTransaction> GetAllTransactions()
		{
			string transactionFilePath = Helper.GetAppTransactionFilePath();
			//Checking if file exist in app directory
			if (!File.Exists(transactionFilePath))
			{
				return new List<ItemTransaction>();
			}
			else
			{

				var transactionJson = File.ReadAllText(transactionFilePath);
				//Converting Json to list of ItemTransaction
				return JsonSerializer.Deserialize<List<ItemTransaction>>(transactionJson);
			}
		}

//Getting Transaction by its status
		public static List<ItemTransaction> GetTransactionsByStatus(Status status)
		{
			List<ItemTransaction> transactions = GetAllTransactions();
			List<ItemTransaction> transactionsByStatus = transactions.FindAll(x => x.TransactionStatus == status);
			return transactionsByStatus;
		}

        //Getting Transaction by its transaction type
        public static List<ItemTransaction> GetTransactionsByType(TransactionType transactionType)
		{
            List<ItemTransaction> transactions = GetAllTransactions();
            List<ItemTransaction> transactionsByType = transactions.FindAll(x => x.TransactionType == transactionType);
            return transactionsByType;
        }

		//Adding record of new transaction
		public static List<ItemTransaction> AddTranasction(Guid itemID,int quantity,Guid requestedUser,TransactionType transactionType)
		{
			List<ItemTransaction> transactions = GetAllTransactions();
			//Checking transaction type
			if (transactionType == TransactionType.Insert)
			{
				//Checking role of user
				if(Helper.IsAuthorized(requestedUser))
				{
					transactions.Add(
						new ItemTransaction
						{
							ItemID = itemID,
							Quantity = quantity,
							RequestedBy = requestedUser,
							IsApproved = true,
							ApprovedBy = requestedUser,
							TransactionStatus=Status.Approved,
							TransactionType = transactionType
						});
					SaveTransaction(transactions);
					return transactions;
				}
				else
				{
					throw new Exception("Unauthorized Action");
				}
			}else
			{
				if (Helper.IsSystemAvailable())
				{
					transactions.Add(
								 new ItemTransaction
								 {
									 ItemID = itemID,
									 Quantity = quantity,
									 RequestedBy = requestedUser,
									 TransactionType = transactionType
								 }
					);
                    SaveTransaction(transactions);
                    return transactions;
				}
				else
				{
					//Throwing error if system is unavailable
                    throw new Exception("Requesting items is only available in office hours. Mon-Fri(9AM-4PM)");

                }

            }
		}

		//Getting transaction record for items 
		public static List<ItemGroupTransaction> GetGroupedTransaction()
		{
			List<InventoryItem> items = ItemsService.GetAllItems();
			List<ItemTransaction> transactions = GetTransactionsByType(TransactionType.Remove);
            //Grouping transaction by its ID and summing the quantity in the transaction
            List<ItemGroupTransaction> transactionGroup= transactions.FindAll(x=>x.TransactionStatus==Status.Approved).GroupBy(a => a.ItemID)
             .Select(a => new ItemGroupTransaction{ ItemId = a.Key, Quantity = a.Sum(c => c.Quantity),TransactionType=TransactionType.Remove }).ToList();
			return transactionGroup;
        }

		//Getting tranasaction requested by individiual user
        public static List<ItemTransaction> GetUserRequest(Guid userID)
		{
			List<ItemTransaction> transactions = GetAllTransactions();
			List<ItemTransaction> userTransactions = transactions.FindAll(x => x.RequestedBy == userID);
			List<ItemTransaction> userRequestTransaction = userTransactions.FindAll(x => x.TransactionType == TransactionType.Remove);
			return userRequestTransaction;
		}

		//Method to Approve transaction request
		public static List<ItemTransaction> ApproveTransaction(Guid userID,Guid transactionID,Status transactionStatus)
		{
			//Checking user role 
			if (Helper.IsAuthorized(userID))
			{
				List<ItemTransaction> transactions = GetAllTransactions();
				ItemTransaction transaction = transactions.FirstOrDefault(x => x.Id == transactionID);
			//Checking if item exist in the system
				if (transaction == null)
				{
					throw new Exception("Transaction couldnot be found");
				}
				else
				{
					//Checking system time to approve the transaction
					if (Helper.IsSystemAvailable())
					{
					if (transactionStatus == Status.Approved)
					{
							//Checking available product before approval 
						if (ItemsService.GetItemById(transaction.ItemID).Quantity >= transaction.Quantity)
						{
                            transaction.ApprovedBy = userID;
                            transaction.IsApproved = true;
                            transaction.TransactionStatus = transactionStatus;
                            SaveTransaction(transactions);
							//AddTranasction(transaction.itemID, transaction.Quantity, transaction.RequestedBy, TransactionType.Remove);
							ItemsService.RemoveQuantity(transaction.ItemID, transaction.Quantity);
                            return transactions;
                        }
						else
						{
							throw new Exception("Insufficent stock available for the product!");
						}
					}else if (transactionStatus == Status.Rejected)
					{
						transaction.ApprovedBy = userID;
						transaction.IsApproved = false;
						transaction.TransactionStatus = transactionStatus;
                        SaveTransaction(transactions);
                        return transactions;
					}
					else
					{
						return transactions;
					}
				
					}
					else
					{
						throw new Exception("Changing Item status is only available in office hours. Mon-Fri(9AM-4PM)");
					}
				}
			}
			else
			{
                throw new Exception("Unauthorized Action!!");
            }
		}

		//Getting transaction report based on month
		public static List<ItemTransaction> GetMonthlyTransactions(int year,int month) {
			List<ItemTransaction> transactions = GetTransactionsByType(TransactionType.Remove);
			List<ItemTransaction> transactionOnYear = transactions.FindAll(x => x.TransactionTime.Year == year);
			List<ItemTransaction> transactionOnMonth = transactionOnYear.FindAll(x => x.TransactionTime.Month == month);
			return transactionOnMonth;
		}

	}
}

