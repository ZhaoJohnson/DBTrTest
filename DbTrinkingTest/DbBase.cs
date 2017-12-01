using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Runtime.Remoting.Contexts;
using System.Text;
using System.Threading.Tasks;

namespace DbTrinkingTest
{
    public class DbBase: DbContext
    {
        public DbBase(string nameOrConnectionString)
            : base(nameOrConnectionString)
        {

        }
        public override int SaveChanges()
        {

            var rowsChanged = ChangeTracker.Entries();
            var dbEntityEntries = rowsChanged as IList<DbEntityEntry> ?? rowsChanged.ToList();
            if (rowsChanged == null || !dbEntityEntries.Any())
                return 0;

            bool isNewed = false;

            var transcation = this.Database.CurrentTransaction;
            if (transcation == null)
            {
                isNewed = true;
                transcation = this.Database.BeginTransaction();
            }

            var addedEntries = dbEntityEntries.Where(e => e.State == EntityState.Added).ToList();
            var modifiedEntries = dbEntityEntries.Where(e => e.State == EntityState.Deleted || e.State == EntityState.Modified).ToList();

            //TrackingSensitiveData trackingSensitiveData = new TrackingSensitiveData();
            //Dictionary<object, List<SensitiveDataTracking>> dictionarySensitiveDataTracking = new Dictionary<object, List<SensitiveDataTracking>>();
            foreach (var entry in modifiedEntries)
            {
                var keys = this.GetKeyNames(entry.Entity.GetType());

                foreach (var item in entry.GetDatabaseValues().PropertyNames)
                {

                    var e = entry.Property(item).CurrentValue;
                    var c = entry.Property(item).OriginalValue;
                    if (e.GetHashCode().Equals(c.GetHashCode()))
                    {
                        Console.WriteLine("eq");
                    }
                    else
                    {
                        //Todo:
                        Console.WriteLine("diff");
                    }
                }
                
            }
           

            int changes = base.SaveChanges();
            foreach (var entry in addedEntries)
            {
                entry.State = EntityState.Added;
                //var sensitiveData = RecordTrackingFunction(entry, trackingSensitiveData);
                //if (sensitiveData?.Count > 0)
                //    dictionarySensitiveDataTracking.Add(entry.Entity, sensitiveData);
                entry.State = EntityState.Unchanged;
            }

            changes += base.SaveChanges();

            if (isNewed)
                transcation.Commit();
 
            return changes;
        }
    }
}
