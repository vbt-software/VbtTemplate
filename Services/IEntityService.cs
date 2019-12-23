using Core.ApiResponse;
using Core.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Services
{
    public interface IEntityService<T> where T : BaseModel
    {
        /// <summary>
        /// Gets the list of given entity.
        /// </summary>
        /// <param name="page">The page.</param>
        /// <param name="pageSize">Size of the page.</param>
        /// <returns></returns>
        IServiceResponse<T> GetAll(int page, int pageSize, int userId);

        /// <summary>
        /// Gets the entity by identifier.
        /// </summary>
        /// <param name="id">The entity identifier.</param>
        IServiceResponse<T> GetById(long id, int userId);

        /// <summary>
        /// Creates the entity.
        /// </summary>
        /// <param name="viewModel">The entity view model.</param>
        IServiceResponse<T> Insert(T entityViewModel,int userId);

        /// <summary>
        /// Updates the entity.
        /// </summary>
        /// <param name="id">The entity identifier.</param>
        /// <param name="entityViewModel">The entity view model.</param>
        IServiceResponse<T> Update(T entityViewModel, int userId);

        /// <summary>
        /// Deletes the entity.
        /// </summary>
        /// <param name="id">The entity identifier.</param>
        IServiceResponse<bool> Delete(long id,int userId);

    }
}
