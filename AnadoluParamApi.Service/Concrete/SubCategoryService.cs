﻿using AnadoluParamApi.Data.Model;
using AnadoluParamApi.Data.UnitOfWork.Abstract;
using AnadoluParamApi.Dto.Dtos;
using AnadoluParamApi.Service.Abstract;
using AutoMapper;

namespace AnadoluParamApi.Service.Concrete
{
    public class SubCategoryService : ISubCategoryService
    {
        private IUnitOfWork _unitOfWork;
        private IMapper _mapper;

        public SubCategoryService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<List<SubCategoryDto>> GetDefaultSubCategoriesAsync() //For member role
        {
            var subCategoryList = await _unitOfWork.SubCategoryRepository.GetFilteredFirstOrDefaults(selector: x => new SubCategoryDto
            {
                SubCategoryName = x.SubCategoryName,
                Status = x.Status,
                Description = x.Description
            },
            expression: x => x.Status == Base.Types.Status.Active || x.Status == Base.Types.Status.Updated
            );

            return subCategoryList;
        }

        public async Task<List<SubCategoryDto>> GetSubCategoriesAsync() //For adminrole
        {
            var subCategoryList = await _unitOfWork.SubCategoryRepository.GetFilteredFirstOrDefaults(selector: x => new SubCategoryDto
            {
                SubCategoryName = x.SubCategoryName,
                Description = x.Description,
                Status = x.Status
            },
            expression: x => x.Status == Base.Types.Status.Active || x.Status == Base.Types.Status.Updated || x.Status == Base.Types.Status.Deleted
            );

            return subCategoryList;
        }

        public async Task<UpdateSubCategoryDto> GetSubCategoryByIdAsync(int id)
        {
            var subCategory = await _unitOfWork.SubCategoryRepository.GetFilteredFirstOrDefault(selector: x => new SubCategory
            {
                ID = x.ID,
                SubCategoryName = x.SubCategoryName,
                Description = x.Description
            },
            expression: x => x.ID == id);

            var model = _mapper.Map<UpdateSubCategoryDto>(subCategory);

            return model;
        }

        public async Task<string> InsertSubCategoryAsync(SubCategoryDto model)
        {
            try
            {
                var subCategory = _mapper.Map<SubCategory>(model);

                var result = await _unitOfWork.SubCategoryRepository.Any(x => x.SubCategoryName == model.SubCategoryName);

                if (result)
                    return "This SubCategory already exists!";

                await _unitOfWork.SubCategoryRepository.InsertAsync(subCategory);
                await _unitOfWork.CompleteAsync();

                return "SubCategory added!";
            }
            catch (Exception ex)
            {
                //todo:Log ll be added..
                return ex.Message;
            }
        }

        public async Task<string> RemoveSubCategoryAsync(int id)
        {
            try
            {
                var deleted = await _unitOfWork.SubCategoryRepository.GetByIdAsync(id);
                if (deleted == null)
                    return "SubCategory not found!";

                await _unitOfWork.SubCategoryRepository.DeleteAsync(deleted);
                await _unitOfWork.CompleteAsync();

                return "SubCategory deleted!";
            }
            catch (Exception ex)
            {
                //todo:Log ll be added..
                return ex.Message;
            }
        }

        public async Task<string> UpdateSubCategoryAsync(UpdateSubCategoryDto model)
        {
            try
            {
                var updated = _mapper.Map<SubCategory>(model);
                var updatedExist = await _unitOfWork.SubCategoryRepository.Any(x => x.ID == updated.ID);

                if (!updatedExist)
                    return "SubCategory not found!";

                updated.Status = Base.Types.Status.Updated;
                updated.UpdatedDate = DateTime.Now;

                _unitOfWork.SubCategoryRepository.Update(updated);
                _unitOfWork.CompleteAsync();

                return "SubCategory updated!";
            }
            catch (Exception ex)
            {
                //todo:Log ll be added..
                return ex.Message;
            }
        }
    }
}
