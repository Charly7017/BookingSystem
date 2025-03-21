using BookingSystem.Application.Common.Interfaces;
using BookingSystem.Application.Services.Interface;
using BookingSystem.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookingSystem.Application.Services.Implementation
{
    public class AmenityService : IAmenityService
    {

        private readonly IUnitOfWork _unitOfWork;

        public AmenityService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public void CreateAmenity(Amenity amenity)
        {
            ArgumentNullException.ThrowIfNull(amenity);

            _unitOfWork.Amenity.Add(amenity);
            _unitOfWork.Save();
        }

        public bool DeleteAmenity(int id)
        {
            try
            {
                var amenity = _unitOfWork.Amenity.Get(p=>p.Id == id);

                if (amenity is not null)
                {
                    _unitOfWork.Amenity.Remove(amenity);
                    _unitOfWork.Save();
                    return true;
                }

                return false;

            }
            catch (Exception)
            {
                return false;
            }
        }

        public IEnumerable<Amenity> GetAllAmenities()
        {
            return _unitOfWork.Amenity.GetAll(includeProperties: "Villa");
        }

        public Amenity GetAmenityById(int id)
        {
            return _unitOfWork.Amenity.Get(p=>p.Id == id,includeProperties: "Villa");
        }

        public void UpdateAmenity(Amenity amenity)
        {
            ArgumentNullException.ThrowIfNull(amenity);

            _unitOfWork.Amenity.Update(amenity);
            _unitOfWork.Save();
        }
    }
}