﻿using AnadoluParamApi.Base.LogOperations.Abstract;
using AnadoluParamApi.Dto.Dtos;
using AnadoluParamApi.Service.Abstract;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using Newtonsoft.Json;
using System.Text;

namespace AnadoluParamApi.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class CategoryController : ControllerBase
    {
        private readonly ICategoryService categoryService;
        private readonly IDistributedCache distributedCache;
        private readonly ILogHelper logHelper;
        public CategoryController(ICategoryService categoryService,IMapper mapper,IDistributedCache distributedCache,ILogHelper logHelper)
        {
            this.categoryService = categoryService;
            this.distributedCache = distributedCache;
            this.logHelper = logHelper;
        }

        [Authorize(Roles = "admin")]
        [HttpGet]
        public async Task<IActionResult> GetAllCategories() //For admin
        {
            var categories = await GetRedis(1);
            return Ok(categories);
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> GetDefaultCategories() //For members or admins
        {
            var categories = await categoryService.GetDefaultCategoriesAsync();
            return Ok(categories);
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> GetCategoryById([FromQuery] int id) //For members
        {
            var result = await categoryService.GetCategoryByIdAsync(id);
            if (result == null)
                return NotFound("Category not found!");
            return Ok(result);
        }

        [Authorize(Roles = "admin")]
        [HttpPost]
        public async Task<IActionResult> InsertCategory([FromBody] CategoryDto model) //For admin
        {
            if (!ModelState.IsValid)
                BadRequest(model);
            var result = await categoryService.InsertCategoryAsync(model);
            return Ok(result);
        }

        [Authorize(Roles = "admin")]
        [HttpPut]
        public async Task<IActionResult> UpdateCategory([FromBody] UpdateCategoryDto model) //For admin
        {
            if (!ModelState.IsValid)
                return BadRequest(model);

            var result = await categoryService.UpdateCategoryAsync(model);
            return Ok(result);
        }

        [Authorize(Roles = "admin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCategory(int id) //For admin
        {
            var result = await categoryService.RemoveCategoryAsync(id);
            return Ok(result);
        }

        [NonAction]
        public async Task<IEnumerable<CategoryDto>> GetRedis(int id)
        {           
            string cacheKey = id.ToString();
            IEnumerable<CategoryDto> categoryDtos;
            string json;

            try
            {
                var categoriesFromCache = await distributedCache.GetAsync(cacheKey);
                if (categoriesFromCache != null)
                {
                    json = Encoding.UTF8.GetString(categoriesFromCache);
                    categoryDtos = JsonConvert.DeserializeObject<List<CategoryDto>>(json);
                    return categoryDtos;
                }
                else
                {
                    var categories = await categoryService.GetCategoriesAsync();

                    json = JsonConvert.SerializeObject(categories);
                    categoriesFromCache = Encoding.UTF8.GetBytes(json);
                    var options = new DistributedCacheEntryOptions()
                            .SetSlidingExpiration(TimeSpan.FromDays(1)) // expires if a certain time has not been reached
                            .SetAbsoluteExpiration(DateTime.Now.AddMonths(1)); // expires after a certain time.
                    await distributedCache.SetAsync(cacheKey, categoriesFromCache, options);
                    return categories;
                }
            }
            catch (Exception ex)
            {
                var logDetails = logHelper.CreateLog("Category", ControllerContext.HttpContext.Request.Method, ex.StackTrace, ex.InnerException != null ? ex.InnerException.Message : ex.Message, "Token creation error.");
                logHelper.InsertLogDetails(logDetails);
                return null;
            }
            
        }

        [NonAction]
        public void DeleteCache(int id)
        {
            // remove cashe
            distributedCache.Remove(id.ToString());
        }
    }
}
