using CoreModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DB
{
    public interface IDBContext
    {
        Task<IEnumerable<District>> GetDistrictsAsync();
        Task<IEnumerable<Address>> GetAddressesAsync();
        Task<List<Event>> GetEventsByDistrictLocationAsync(string districtName);
        Task<List<Event>> GetEventsByPeriodTimeAsync(DateTime minDateTime);
        Task<List<Event>> GetLastEventsByTimeAsync(DateTime minDateTime, DateTime maxDateTime);
        Task<List<Source>> GetSourcesAsync();
        Task AddEventAsync(Event ev);
        Task<District> GetDistrictByNameAsync(string name);
        Task<List<Event>> GetAllEventsAsync();
    }
}
