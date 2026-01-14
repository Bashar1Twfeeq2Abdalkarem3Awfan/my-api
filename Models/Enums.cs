using System.ComponentModel.DataAnnotations;

#nullable enable

namespace MyAPIv3.Models
{
    public enum PersonTypeEnum
    {
        Customer,
        Supplier,
        Staff,
        Other
    }

    public enum InvoiceTypeEnum
    {
        Sale,
        Purchase,
        Return
    }

    public enum ReturnTypeEnum
    {
        [Display(Name = "Return From Customer")]
        ReturnFromCustomer,
        [Display(Name = "Return To Supplier")]
        ReturnToSupplier
    }
}