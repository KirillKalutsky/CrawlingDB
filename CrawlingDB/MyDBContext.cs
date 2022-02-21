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
        public async Task<List<Event>> GetAllEventsAsync()
        {
            return await Events.ToListAsync();
        }

        public async Task<IEnumerable<District>> GetDIstrictsWithAddresses()
        {
            return await Districts.Include(d => d.Addresses)
                .ToListAsync();
        }

        public async Task<District> GetDistrictByNameAsync(string name)
        {
            var district = await Districts
                .Include(distr => distr.Events)
                .Where(distr => distr.DistrictName == name)
                .FirstOrDefaultAsync();
            return district;
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

        public async Task<List<Source>> GetSourcesAsync()
        {
            return await Sources
                .Include(source=>source.Fields)
                .Include(source =>source.Events)
                .ToListAsync();
        }

        public async Task<List<Event>> GetLastEventsByTimeAsync(DateTime minDateTime, DateTime maxDateTime)
        {
            return await Events
                .Where(ev => ev.DateOfDownload >= minDateTime && ev.DateOfDownload <= maxDateTime)
                .ToListAsync();
        }

        public async Task<List<Event>> GetEventsByPeriodTimeAsync(DateTime minDateTime)
        {
            return await Events
                .Where(ev => ev.DateOfDownload >= minDateTime)
                .ToListAsync();
        }

        public async Task<List<Event>> GetEventsByDistrictLocation(string districtName)
        {
            return await Events
                .Where(ev => ev.DistrictName == districtName)
                .ToListAsync();
        }

        public async Task<IEnumerable<District>> GetDistrictsAsync()
        {
            return await Districts.Include(d => d.Addresses).ToListAsync();
        }

        public async Task<IEnumerable<Address>> GetAddressesAsync()
        {
            return await Addresses.Include(adr => adr.District).ToListAsync();
        }

        public async Task<List<Event>> GetEventsByDistrictLocationAsync(string districtName)
        {
            return await Events
                .Where(ev => ev.DistrictName == districtName)
                .ToListAsync();
        }

        public async Task SaveAllChangesAsync()
        {
            await SaveChangesAsync();
        }

        public async Task<List<Source>> GetSourcesForCrawlingAsync()
        {
            return await Sources
                .Include(source => source.Fields)
                .Include(source => source.Events
                    .OrderBy(x => x.DateOfDownload).Take(1))
                .ToListAsync();
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
    }
}
