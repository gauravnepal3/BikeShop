using System;
using System.ComponentModel.DataAnnotations;
namespace BikeShop.Data.Models
{
//Model for Transaction of each item
	public class ItemTransaction
	{
		public Guid Id { get; set; } = Guid.NewGuid();

		[Required(ErrorMessage ="Item Id for tranasction is required")]
		public Guid ItemID { get; set; }
		public int Quantity { get; set; }
		public DateTime TransactionTime { get; set; } = DateTime.UtcNow;
		public Guid RequestedBy { get; set; }
		public bool IsApproved { get; set; } = false;
		public Status TransactionStatus { get; set; } = Status.Pending;
		public Guid ApprovedBy { get; set; }

//tacking both addition and removal of items
		public TransactionType TransactionType { get; set; }
	}
}

