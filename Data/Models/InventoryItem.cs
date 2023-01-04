using System;
using System.ComponentModel.DataAnnotations;

namespace BikeShop.Data.Models
{
//Model for Ietms in Inventory
    public class InventoryItem
	{
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required(ErrorMessage = "Item Name is required.")]
        public string ItemName { get; set; }
        public int Quantity { get; set; } = 0;
        public DateTime LastTransactionDate { get; set; } = DateTime.Today;
        //Minimum quantity required in 
        public int MinimumRequiredQuantity { get; set; } = 0;
        public Guid CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Today;
        public Boolean isActive { get; set; } = true;
    }
}

