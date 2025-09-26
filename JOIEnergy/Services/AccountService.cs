using JOIEnergy.Data;
using System.Linq;

namespace JOIEnergy.Services
{
    public class AccountService : IAccountService
    { 
        private readonly JOIEnergyDbContext _context;

        public AccountService(JOIEnergyDbContext context) 
        {
            _context = context;
        }

        public string GetPricePlanIdForSmartMeterId(string smartMeterId) 
        {
            var account = _context.SmartMeterAccounts
                .FirstOrDefault(a => a.SmartMeterId == smartMeterId);
                
            return account?.PricePlanId;
        }
    }
}
