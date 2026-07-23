using System;
using System.ComponentModel.DataAnnotations;

namespace PoliticalLeaderPortal.Areas.Admin.ViewModels.ElectionWarRoom
{
    public class EventExpenseVM
    {
        public int EventExpenseId { get; set; }

        [Required(ErrorMessage = "Please select an event.")]
        [Display(Name = "Event")]
        public int EventId { get; set; }

        public string EventTitle { get; set; }

        [Required(ErrorMessage = "Expense category is required.")]
        [Display(Name = "Expense Category")]
        public string ExpenseCategory { get; set; }

        [Required(ErrorMessage = "Expense head is required.")]
        [Display(Name = "Expense Head")]
        public string ExpenseHead { get; set; }

        [Display(Name = "Vendor Name")]
        public string VendorName { get; set; }

        [Display(Name = "Vendor Mobile")]
        public string VendorMobile { get; set; }

        [Display(Name = "Bill Number")]
        public string BillNumber { get; set; }

        [DataType(DataType.Date)]
        [Display(Name = "Bill Date")]
        public DateTime? BillDate { get; set; }

        [Display(Name = "Quantity")]
        public decimal? Quantity { get; set; }

        [Display(Name = "Unit")]
        public string Unit { get; set; }

        [Required(ErrorMessage = "Rate is required.")]
        [Display(Name = "Rate")]
        public decimal Rate { get; set; }

        [Required(ErrorMessage = "Amount is required.")]
        [Display(Name = "Amount")]
        public decimal Amount { get; set; }

        [Display(Name = "Payment Mode")]
        public string PaymentMode { get; set; }

        [Display(Name = "Payment Status")]
        public string PaymentStatus { get; set; }

        [Display(Name = "Expense Status")]
        public string ExpenseStatus { get; set; }

        // UI helper (not stored in database)
        public string ExpenseStatusColor { get; set; }

        [Display(Name = "Paid To")]
        public string PaidTo { get; set; }

        [Display(Name = "Paid By")]
        public string PaidBy { get; set; }

        [Display(Name = "Transaction Reference")]
        public string TransactionReference { get; set; }

        [Required(ErrorMessage = "Expense date is required.")]
        [DataType(DataType.Date)]
        [Display(Name = "Expense Date")]
        public DateTime ExpenseDate { get; set; }

        [Display(Name = "Approved By")]
        public string ApprovedBy { get; set; }

        [Display(Name = "Verified By")]
        public int? VerifiedBy { get; set; }

        public string VerifiedByName { get; set; }

        [DataType(DataType.Date)]
        [Display(Name = "Verified Date")]
        public DateTime? VerifiedDate { get; set; }

        [Display(Name = "Remarks")]
        public string Remarks { get; set; }

        [Display(Name = "Active")]
        public bool IsActive { get; set; }

        [Display(Name = "Created By")]
        public int? CreatedBy { get; set; }

        public string CreatedByName { get; set; }

        [Display(Name = "Created Date")]
        public DateTime CreatedDate { get; set; }

        [Display(Name = "Updated By")]
        public int? UpdatedBy { get; set; }

        public string UpdatedByName { get; set; }

        [Display(Name = "Updated Date")]
        public DateTime? UpdatedDate { get; set; }
    }
}