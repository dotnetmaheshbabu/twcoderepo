using JOIEnergy.Domain;
using JOIEnergy.Data;
using System.Collections.Generic;
using System.Linq;

namespace JOIEnergy.Services
{
    public class MeterReadingService : IMeterReadingService
    {
        private readonly JOIEnergyDbContext _context;
        
        public MeterReadingService(JOIEnergyDbContext context)
        {
            _context = context;
        }

        public List<ElectricityReading> GetReadings(string smartMeterId) 
        {
            var readings = _context.ElectricityReadings
                .Where(r => r.SmartMeterId == smartMeterId)
                .OrderBy(r => r.Time)
                .Select(r => new ElectricityReading 
                { 
                    Time = r.Time, 
                    Reading = r.Reading 
                })
                .ToList();
                
            return readings;
        }

        public void StoreReadings(string smartMeterId, List<ElectricityReading> electricityReadings) 
        {
            var entities = electricityReadings.Select(r => new ElectricityReadingEntity
            {
                SmartMeterId = smartMeterId,
                Reading = r.Reading,
                Time = r.Time
            }).ToList();

            _context.ElectricityReadings.AddRange(entities);
            _context.SaveChanges();
        }
    }
}
