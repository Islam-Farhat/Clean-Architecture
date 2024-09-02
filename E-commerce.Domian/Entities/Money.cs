using System.ComponentModel.DataAnnotations.Schema;

namespace E_commerce.Domian
{
    public record Money(string Currency,decimal Amount);//Value Object
}
