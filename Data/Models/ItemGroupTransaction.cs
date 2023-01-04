using System;
namespace BikeShop.Data.Models
{

	public class ItemGroupTransaction
	{
		public Guid ItemId { get; set; }
		public int Quantity { get; set; }
		public TransactionType TransactionType { get; set; }
	}
}

