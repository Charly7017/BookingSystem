using BookingSystem.Application.Common.Interfaces;
using BookingSystem.Domain.Entities;
using BookingSystem.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace BookingSystem.Infrastructure.Repository
{
    public class VillaRepository : Repository<Villa>, IVillaRepository
    {

        private readonly ApplicationDbContext _db;

        public VillaRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }

        public void Save()
        {
            _db.SaveChanges();
        }

        public void Update(Villa entity)
        {
            _db.Villas.Update(entity);
        }
    }
}
