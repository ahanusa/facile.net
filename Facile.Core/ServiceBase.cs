﻿using Facile;
using Facile.Core.Extensions;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Facile.Core
{
    /// <summary>
    /// Base class of all business services
    /// </summary>
    public abstract class ServiceBase<T, TKey> : IService<T, TKey> where T : IDomainObject<TKey>, new()
    {
        protected IDataProxy<T, TKey> _dataProxy;

        protected IDataProxy<T, TKey> DataProxy
        {
            get { return _dataProxy; }
        }

        public ServiceBase(IDataProxy<T, TKey> dataProxy)
        {
            _dataProxy = dataProxy;
        }

        /// <summary>
        /// Override this method to supply custom business rules to GetAllCommand() and GetByIDCommand()
        /// </summary>
        protected virtual IEnumerable<IRule> GetBusinessRulesForRetrieve(TKey id)
        {
            return Enumerable.Empty<IRule>();
        }
        
        /// <summary>
        /// Override this method to supply custom business rules to InsertCommand()
        /// </summary>
        protected virtual IEnumerable<IRule> GetBusinessRulesForInsert(T entity)
        {
            return Enumerable.Empty<IRule>();
        }

        /// <summary>
        /// Override this method to supply custom business rules to UpdateCommand()
        /// </summary>
        protected virtual IEnumerable<IRule> GetBusinessRulesForUpdate(T entity)
        {
            return Enumerable.Empty<IRule>();
        }

        /// <summary>
        /// Override this method to supply custom business rules to DeleteCommand() 
        /// </summary>
        protected virtual IEnumerable<IRule> GetBusinessRulesForDelete(TKey id)
        {
            return Enumerable.Empty<IRule>();
        }

        /// <summary>
        /// Builds validation results from the supplied list of IRule
        /// </summary>
        /// <param name="businessRules"></param>
        /// <returns></returns>
        protected virtual IEnumerable<ValidationResult> GetBusinessRulesResults(IEnumerable<IRule> businessRules)
        {
            string entityName = typeof(T).Name;

            var invalidRules = businessRules.ToArray()
                                            .ForEach(rule => rule.Validate())
                                            .Where(rule => !rule.IsValid);

            foreach (var rule in invalidRules)
            {
                yield return new ValidationResult(rule.ErrorMessage, new string[] {  entityName });
            }
        }

        /// <summary>
        /// Supplies validation results to InsertCommand()
        /// </summary>
        protected virtual IEnumerable<ValidationResult> GetValidationResultsForInsert(T entity)
        {
            foreach (var error in entity.GetValidationErrors())
                yield return error;
        }
        
        /// <summary>
        /// Supplies validation results to UpdateCommand()
        /// </summary>
        protected virtual IEnumerable<ValidationResult> GetValidationResultsForUpdate(T entity)
        {
            foreach (var error in entity.GetValidationErrors())
                yield return error;
        }

        /// <summary>
        /// Supplies validation results to DeleteCommand()
        /// </summary>

        protected virtual IEnumerable<ValidationResult> GetValidationResultsForDelete(TKey id)
        {
            yield break;
        }
        
        /// <summary>
        /// Supplies validation results to GetByIDCommand()
        /// </summary>
        protected virtual IEnumerable<ValidationResult> GetValidationResultsForGetByID(TKey id)
        {
            yield break;
        }

        /// <summary>
        /// Supplies validation results to GetAllCommand()
        /// </summary>
        protected virtual IEnumerable<ValidationResult> GetValidationResultsForGetAll()
        {
            yield break;
        }

        /// <summary>
        /// Composes a <see cref="T:Facile.ICommand`1" /> that invokes <see cref="T:Facile.IDataProxy`1" />.GetByID() upon successful execution of business and validation rules
        /// </summary>
        public virtual ICommand<T> GetByIDCommand(TKey id)
        {
            return new ServiceCommand<T>
            (
                beforeExecuteMethod: () => OnBeforeGetByIDCommandExecuted(id),
                executeMethod: () => GetByID(id),
                executeAsyncMethod: () => GetByIDAsync(id),
                getValidationResultsMethod: () => GetValidationResultsForGetByID(id),
                getBusinessRulesResultsMethod: () => GetBusinessRulesResults(GetBusinessRulesForRetrieve(id))
            );
        }

        protected void OnBeforeGetByIDCommandExecuted(TKey id)
        {
        }

        /// <summary>
        /// Composes a <see cref="T:Facile.ICommand`1" /> that invokes <see cref="T:Facile.IDataProxy`1" />.GetAll() upon successful execution of business and validation rules
        /// </summary>
        public virtual ICommand<IEnumerable<T>> GetAllCommand()
        {
            return new ServiceCommand<IEnumerable<T>>
            (
                beforeExecuteMethod: () => OnBeforeGetAllCommandExecuted(),
                executeMethod: () => GetAll(),
                executeAsyncMethod: () => GetAllAsync(),
                getValidationResultsMethod: () => GetValidationResultsForGetAll(),
                getBusinessRulesResultsMethod: () => GetBusinessRulesResults(new IRule[] { })
            );
        }

        protected void OnBeforeGetAllCommandExecuted()
        {
        }

        /// <summary>
        /// Composes a <see cref="T:Facile.ICommand`1" /> that invokes <see cref="T:Facile.IDataProxy`1" />.Insert() upon successful execution of business and validation rules
        /// </summary>
        public virtual ICommand<T> InsertCommand(T entity)
        {
            return new ServiceCommand<T>
            (
                beforeExecuteMethod: () => OnBeforeInsertCommandExecuted(entity),
                executeMethod: () => Insert(entity),
                executeAsyncMethod: () => InsertAsync(entity),
                getValidationResultsMethod: () => GetValidationResultsForInsert(entity),
                getBusinessRulesResultsMethod: () => GetBusinessRulesResults(GetBusinessRulesForInsert(entity))
            );
        }

        protected virtual void OnBeforeInsertCommandExecuted(T entity)
        {
        }
        
        /// <summary>
        /// Composes a <see cref="T:Facile.ICommand`1" /> that invokes <see cref="T:Facile.IDataProxy`1" />.Update() upon successful execution of business and validation rules
        /// </summary>
        /// <param name="entity"></param>
        public virtual ICommand<T> UpdateCommand(T entity)
        {
            return new ServiceCommand<T>
            (
                beforeExecuteMethod: () => OnBeforeUpdateCommandExecuted(entity),
                executeMethod: () => Update(entity),
                executeAsyncMethod: () => UpdateAsync(entity),
                getValidationResultsMethod: () => GetValidationResultsForUpdate(entity),
                getBusinessRulesResultsMethod: () => GetBusinessRulesResults(GetBusinessRulesForUpdate(entity))
            );
        }

        protected void OnBeforeUpdateCommandExecuted(T entity)
        {
        }

        /// <summary>
        /// Composes a <see cref="Facile.ICommand" /> that invokes <see cref="T:Facile.IDataProxy`1" />.Delete() upon successful execution of business and validation rules
        /// </summary>
        public virtual ICommand DeleteCommand(TKey id)
        {
            return new ServiceCommand
            (
                beforeExecuteMethod: () => OnBeforeDeleteCommandExecuted(id),
                executeMethod: () => Delete(id),
                executeAsyncMethod: () => DeleteAsync(id),
                getValidationResultsMethod: () => GetValidationResultsForDelete(id),
                getBusinessRulesResultsMethod: () => GetBusinessRulesResults(GetBusinessRulesForDelete(id))
            );
        }

        protected void OnBeforeDeleteCommandExecuted(TKey id)
        {
        }

        /// <summary>
        /// Invoked by GetAllCommand() if validation and business rules execute successfully
        /// </summary>
        protected virtual IEnumerable<T> GetAll()
        {
            return _dataProxy.GetAll();
        }

        /// <summary>
        /// Invoked by GetByIDCommand() if validation and business rules execute successfully
        /// </summary>
        protected virtual T GetByID(TKey id)
        {
            return _dataProxy.GetByID(id);
        }

        /// <summary>
        /// Invoked by InsertCommand() if validation and business rules execute successfully
        /// </summary>
        protected virtual T Insert(T entity)
        {
            return _dataProxy.Insert(entity);
        }

        /// <summary>
        /// Invoked by UpdateCommand() if validation and business rules execute successfully
        /// </summary>
        protected virtual T Update(T entity)
        {
            return _dataProxy.Update(entity);
        }

        /// <summary>
        /// Invoked by DeleteCommand() if validation and business rules execute successfully
        /// </summary>
        protected virtual void Delete(TKey id)
        {
            _dataProxy.Delete(id);
        }

        /// <summary>
        /// Invoked by GetAllCommand() if validation and business rules execute successfully
        /// </summary>
        protected virtual async Task<IEnumerable<T>> GetAllAsync()
        {
            return await _dataProxy.GetAllAsync();
        }

        /// <summary>
        /// Invoked by GetByIDCommand() if validation and business rules execute successfully
        /// </summary>
        protected virtual async Task<T> GetByIDAsync(TKey id)
        {
            return await _dataProxy.GetByIDAsync(id);
        }

        /// <summary>
        /// Invoked by InsertCommand() if validation and business rules execute successfully
        /// </summary>
        protected virtual async Task<T> InsertAsync(T entity)
        {
            return await _dataProxy.InsertAsync(entity);
        }

        /// <summary>
        /// Invoked by UpdateCommand() if validation and business rules execute successfully
        /// </summary>
        protected virtual async Task<T> UpdateAsync(T entity)
        {
            return await _dataProxy.UpdateAsync(entity);
        }

        /// <summary>
        /// Invoked by DeleteCommand() if validation and business rules execute successfully
        /// </summary>
        protected virtual Task DeleteAsync(TKey id)
        {
            return _dataProxy.DeleteAsync(id);
        }
    }
}
