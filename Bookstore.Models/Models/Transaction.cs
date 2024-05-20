using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bookstore.Models
{
    public class Transaction
    {
        [Key]
        public string TransactionCode {  get; set; }
        [NotNull]
        public string ErrorCode { get; set; }
        [NotNull]
        public string ErrorMessage { get; set; }
        public int? OrderCode { get; set; }
		[ForeignKey("OrderCode")]
		[ValidateNever]
		public virtual OrderHeader OrderHeader { get; set; }
		public double? Amount { get; set; }
        public string? Currency { get; set; }
        public string? BuyerEmail { get; set; }
        public string? BuyerPhone { get; set; }
        public string? CardNumber { get; set; }
        public string? BuyerName { get; set; }
        public string? Status { get; set; }
        public string? Reason { get; set; }
        public string? Description { get; set; }
        public bool? Installment { get; set; }
        public bool? Is3D { get; set; }
        public int? Month { get; set; }
        public string? BankCode { get; set; }
        public string? BankName { get; set; }
        public string? Method { get; set; }
        public DateTime? TransactionTime { get; set; }
        public DateTime? SuccessTime { get; set; }
        public string? BankHotline { get; set; }
        public double? MerchantFee { get; set; }
        public double? PayerFee { get; set; }
        public string? BankType { get; set; }
        public string? AuthenCode { get; set; }
    }
}
