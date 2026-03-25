using Refit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.PaymentHttp
{
    public interface IPaymentApi
    {
        [Get("/payment/initiate")]
        Task<bool> InitiatePayment([Query] string orderId);

        [Get("/payment/callback")]
        Task CallBackPayment([Query] string orderId, [Query] bool status);
    }
}
