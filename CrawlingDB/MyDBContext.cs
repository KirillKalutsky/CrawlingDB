using Microsoft.EntityFrameworkCore;
using System;
using CoreModels;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace DB
{
    public class MyDBContext: DbContext, IDBContext
    {
        //DbSet<> список событий(по идеи валуе обжект, так как с равным полями одно и то же, но с индексацией)
        // список источников (ентити, )
        // список полей для источников(ентити)
        public DbSet<Event> Events { get; set; }
        public DbSet<Source> Sources { get; set; }
        public DbSet<SourceFields> SourceFields { get; set; }
        public DbSet<District> Districts { get; set; }
        public DbSet<Address> Addresses { get; set; }

        public MyDBContext()
        {
            Database.EnsureCreated();
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Source>()
                .HasOne(x => x.Fields)
                .WithOne(f => f.Source)
                .HasForeignKey<SourceFields>(x => x.SourceId);

            modelBuilder.Entity<Event>()
                .HasOne<Source>()
                .WithMany(s => s.Events)
                .HasForeignKey(e => e.IdSource);

            modelBuilder.Entity<District>()
                .HasMany(d => d.Addresses)
                .WithOne(a => a.District);

            modelBuilder.Entity<Event>()
               .HasOne<District>()
               .WithMany(d => d.Events)
               .HasForeignKey(e=>e.DistrictName);
        }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseNpgsql("Host=localhost;Port=5432;Database=redZone;Username=postgres;Password=abrakadabra77");
        }

        public async Task AddSourceAsync(Source source)
        {
            await Sources.AddAsync(source);
        }

        
        public async Task AddEventAsync(Event ev)
        {
            Debug.Print("Add Event");
            if(await Events.FindAsync(ev.Link)==null)
            {
                await Events.AddAsync(ev);
            }
            Debug.Print("Succes Add Event");
        }

        public async Task SaveAllChangesAsync()
        {
            await SaveChangesAsync();
        }

        public Task<bool> SourcesIsEmptyAsync()
        {
            return Task.Run(() => !Sources.Any());
        }

        public Task<bool> LocationsIsEmptyAsync()
        {
            return Task.Run(() => !(Addresses.Any() && Districts.Any()));
        }

        public async Task AddAddressAsync(Address address)
        {
            await Addresses.AddAsync(address);
        }

        public async Task AddDistrictAsync(District district)
        {
            await Districts.AddAsync(district);
        }

        public async Task<PageList<District>> GetDistrictsAsync(int pageNumber, int pageSize)
        {
            var districts = await Districts
                .Include(d => d.Addresses)
                .ToListAsync();
            return CreatePageList(districts, pageNumber, pageSize);
        }

        public async Task<PageList<Address>> GetAddressesAsync(int pageNumber, int pageSize)
        {
            var addreses = await Addresses
                .Include(adr => adr.District)
                .OrderBy(adr => adr.AddressName)
                .ToListAsync();
            return CreatePageList(addreses, pageNumber, pageSize);
        }

        public async Task<PageList<Event>> GetwDistrictEventsAsync(string districtName, int pageNumber, int pageSize)
        {
            var events = await Events
                .Where(ev => ev.DistrictName == districtName)
                .OrderBy(ev => ev.DateOfDownload)
                .ToListAsync();
            return CreatePageList(events, pageNumber, pageSize);
        }

        public async Task<PageList<Event>> GetEventsByPeriodTimeAsync(DateTime minDateTime, int pageNumber, int pageSize)
        {
            var events = await Events
                .Where(ev => ev.DateOfDownload >= minDateTime)
                .OrderBy(ev => ev.DateOfDownload)
                .ToListAsync();
            return CreatePageList(events, pageNumber, pageSize);
        }

        public async Task<PageList<Event>> GetLastEventsByTimeAsync(DateTime minDateTime, DateTime maxDateTime, int pageNumber, int pageSize)
        {
            var events = await Events
                .Where(ev => ev.DateOfDownload >= minDateTime 
                    && ev.DateOfDownload <= maxDateTime)
                .OrderBy(ev => ev.DateOfDownload)
                .ToListAsync();
            return CreatePageList(events, pageNumber, pageSize);
        }

        public async Task<PageList<Event>> GetEventsAsync(int pageNumber, int pageSize)
        {
            var events = await Events
                .OrderBy(ev => ev.DateOfDownload)
                .ToListAsync();
            return CreatePageList(events, pageNumber, pageSize);
        }

        public async Task<PageList<Source>> GetSourcesForCrawlingAsync(int pageNumber, int pageSize)
        {
            var sources = await Sources
               .Include(source => source.Fields)
               .Include(source => source.Events
                   .OrderBy(ev => ev.DateOfDownload).Take(1))
               .OrderBy(source => source.Id)
               .ToListAsync();
            return CreatePageList(sources, pageNumber, pageSize);
        }

        public Task<PageList<Event>> GetSourceEventsAsync(int sourceId, int pageNumber, int pageSize)
        {
            var events = Sources
                .Include(source => source.Events)
                .Where(source => source.Id == sourceId)
                .SingleOrDefault()
                .Events
                .OrderBy(ev => ev.DateOfDownload);
            return Task.Run(()=>CreatePageList(events, pageNumber, pageSize));
        }

        public Task<PageList<Event>> GetDistrictEventsAsync(string districtName, int pageNumber, int pageSize)
        {
            var events = Districts
                .Include(distr => distr.Events)
                .Where(distr => distr.DistrictName == districtName)
                .SingleOrDefault()
                .Events
                .OrderBy(ev => ev.DateOfDownload);
            return Task.Run(()=>CreatePageList(events, pageNumber, pageSize));
        }

        private PageList<T> CreatePageList<T>(IEnumerable<T> collect, int pageNumber, int pageSize)
        {
            if (pageNumber < 1)
                pageNumber = 1;

            if (pageSize < 1)
                pageSize = 100;

            return new PageList<T>
            (
                collect.Skip(pageNumber - 1 * pageSize).Take(pageSize).ToList(),
                collect.Count(),
                pageNumber,
                pageSize
            );
        }

    }
}
