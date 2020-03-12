using Core;
using Core.Security;
using DB;
using DB.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace Repository
{
    public class GeneralRepository<T> : IRepository<T> where T : BaseEntity
    {
        private readonly VbtContext _context;
        private DbSet<T> _entities;
        private readonly IEncryption _encryption;

        public GeneralRepository(VbtContext context, IEncryption encryption)
        {
            _context = context;
            _entities = context.Set<T>();
            _encryption = encryption;
        }

        public virtual T GetById(object id)
        {
            return Entities.Find(id);
        }

        /// <summary>
        ///     Insert entity
        /// </summary>
        /// <param name="entity">Entity</param>
        public void Insert(T entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));
            //Set Default UsedTime Parameter ==> UsedTime BaseEntity Property.

            //Entity'ye ait MetaData varsa bulunur
            System.ComponentModel.DataAnnotations.MetadataTypeAttribute[] metadataTypes = entity.GetType().GetCustomAttributes(true).OfType<System.ComponentModel.DataAnnotations.MetadataTypeAttribute>().ToArray();
            foreach (System.ComponentModel.DataAnnotations.MetadataTypeAttribute metadata in metadataTypes)
            {
                System.Reflection.PropertyInfo[] properties = metadata.MetadataClassType.GetProperties();
                //Metadata atanmış entity'nin tüm propertyleri tek tek alınır.
                foreach (System.Reflection.PropertyInfo pi in properties)
                {                 
                    //Eğer ilgili property ait CryptoData flag'i var ise ilgili deger encrypt edilir. 
                    if (Attribute.IsDefined(pi, typeof(DB.PartialEntites.CryptoData)))
                    {
                        _context.Entry(entity).Property(pi.Name).CurrentValue = _encryption.EncryptText(_context.Entry(entity).Property(pi.Name).CurrentValue.ToString());
                    }
                }
            }

            //_context.Entry(entity).Property("UsedTime").CurrentValue = DateTime.Now; //UsedTime Entity'de görülmez [NotMapped] olduğu için.
            entity.UsedTime = DateTime.Now;//02.26.2020
            //---------------

            _entities.Add(entity);
            _context.SaveChanges();
        }

        /// <summary>
        ///     Insert entities
        /// </summary>
        /// <param name="entities">Entities</param>
        public virtual void Insert(IEnumerable<T> entities)
        {
            if (entities == null)
                throw new ArgumentNullException(nameof(entities));

            foreach (var entity in entities)
                Entities.Add(entity);

            _context.SaveChanges();
        }

        /// <summary>
        ///     Update entity
        /// </summary>
        /// <param name="entity">Entity</param>
        public void Update(T entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));
            _context.SaveChanges();
        }


        /// <summary>
        ///     Update entities
        /// </summary>
        /// <param name="entities">Entities</param>
        public virtual void Update(IEnumerable<T> entities)
        {
            if (entities == null)
                throw new ArgumentNullException(nameof(entities));

            _context.SaveChanges();
        }

        /// <summary>
        ///     Update entities
        /// </summary>
        /// <param name="entities">Entities</param>
        public virtual void UpdateMatchEntity(T updateEntity, T setEntity)
        {
            if (setEntity == null)
                throw new ArgumentNullException(nameof(setEntity));

            if (updateEntity == null)
                throw new ArgumentNullException(nameof(updateEntity));

            /*Custom Metadata Attribute'ü olan Entity Propertyleri Yakalanır 
             Örneğin Bu örnekte[SetCurrentDate] attributes'ü atanmış colonlara bugünün Tarihi Global olarak atanır.
             */
            System.ComponentModel.DataAnnotations.MetadataTypeAttribute[] metadataTypes = setEntity.GetType().GetCustomAttributes(true).OfType<System.ComponentModel.DataAnnotations.MetadataTypeAttribute>().ToArray();
            foreach (System.ComponentModel.DataAnnotations.MetadataTypeAttribute metadata in metadataTypes)
            {
                System.Reflection.PropertyInfo[] properties = metadata.MetadataClassType.GetProperties();
                foreach (System.Reflection.PropertyInfo pi in properties)
                {
                    if (Attribute.IsDefined(pi, typeof(DB.PartialEntites.SetCurrentDate)))
                    {
                        // here it is found
                        _context.Entry(setEntity).Property(pi.Name).CurrentValue = DateTime.Now;
                    }
                }
            }
            /*-------------------*/

            _context.Entry(updateEntity).CurrentValues.SetValues(setEntity);//Tüm kayıtlar, kolon eşitlemesine gitmeden 1 entity'den diğerine atanır.

            //Olmayan yani null gelen kolonlar, var olan tablonun üstüne ezilmesin diye ==> "IsModified = false" olarak atanır ve var olan kayıtların null olarak güncellenmesi engellenir.
            foreach (var property in _context.Entry(setEntity).Properties)
            {
                if (property.CurrentValue == null) { _context.Entry(updateEntity).Property(property.Metadata.Name).IsModified = false; }
            }

            _context.SaveChanges();
        }

        /// <summary>
        ///     Delete entity
        /// </summary>
        /// <param name="entity">Entity</param>
        public void Delete(T entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));
            Entities.Remove(entity);
            _context.SaveChanges();
        }

        /// <summary>
        ///     Delete entities
        /// </summary>
        /// <param name="entities">Entities</param>
        public virtual void Delete(IEnumerable<T> entities)
        {
            if (entities == null)
                throw new ArgumentNullException(nameof(entities));

            foreach (var entity in entities)
                Entities.Remove(entity);
            _context.SaveChanges();
        }

        public IQueryable<T> IncludeMany(params Expression<Func<T, object>>[] includes)
        {
            return _entities.IncludeMultiple(includes);
        }

        public IEnumerable<T> GetSql(string sql)
        {
            return Entities.FromSqlRaw(sql);
        }

        /// <summary>
        ///     Gets a table
        /// </summary>
        public virtual IQueryable<T> Table => Entities;

        /// <summary>
        ///     Gets a table with "no tracking" enabled (EF feature) Use it only when you load record(s) only for read-only
        ///     operations
        /// </summary>
        public virtual IQueryable<T> TableNoTracking => Entities.AsNoTracking();

        /// <summary>
        ///     Entities
        /// </summary>
        protected virtual DbSet<T> Entities => _entities ?? (_entities = _context.Set<T>());

    }
}
