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
        Task<PageList<District>> GetDistrictsAsync(int pageNumber, int pageSize);
        Task<PageList<Address>> GetAddressesAsync(int pageNumber, int pageSize);
        Task<PageList<Event>> GetEventsByPeriodTimeAsync(DateTime minDateTime, int pageNumber, int pageSize);
        Task<PageList<Event>> GetLastEventsByTimeAsync(DateTime minDateTime, DateTime maxDateTime, int pageNumber, int pageSize);
        Task AddEventAsync(Event ev);
        Task<PageList<Event>> GetSourceEventsAsync(int sourceId, int pageNumber, int pageSize);
        Task<PageList<Event>> GetDistrictEventsAsync(int districtId, int pageNumber, int pageSize);
        Task<PageList<Event>> GetEventsAsync(int pageNumber, int pageSize);
        Task SaveAllChangesAsync();
        Task<PageList<Source>> GetSourcesForCrawlingAsync(int pageNumber, int pageSize);
        Task<bool> SourcesIsEmptyAsync();
        Task<bool> LocationsIsEmptyAsync();
        Task AddSourceAsync(Source source);
        Task AddAddressAsync(Address address);
        Task AddDistrictAsync(District district);

    }
}
