using System;
using System.Collections.Generic;
using UnityEngine;

namespace Assignment.Core.DI
{
    /// <summary>
    /// Simple dependency injection container for managing services
    /// </summary>
    public class ServiceContainer
    {
        private static ServiceContainer _instance;
        public static ServiceContainer Instance => _instance ??= new ServiceContainer();

        private readonly Dictionary<Type, object> _services = new Dictionary<Type, object>();

        /// <summary>
        /// Registers a service instance
        /// </summary>
        /// <typeparam name="T">Service interface type</typeparam>
        /// <param name="service">Service implementation</param>
        public void RegisterService<T>(T service) where T : class
        {
            var serviceType = typeof(T);
            _services[serviceType] = service ?? throw new ArgumentNullException(nameof(service));
            // Debug.Log($"[ServiceContainer] Registered service: {serviceType.Name}");
        }

        /// <summary>
        /// Gets a registered service
        /// </summary>
        /// <typeparam name="T">Service interface type</typeparam>
        /// <returns>Service instance</returns>
        /// <exception cref="InvalidOperationException">Thrown when service is not registered</exception>
        public T GetService<T>() where T : class
        {
            var serviceType = typeof(T);
            if (_services.TryGetValue(serviceType, out var service))
            {
                return service as T;
            }

            throw new InvalidOperationException($"Service of type {serviceType.Name} not registered. Please register it first using RegisterService<T>().");
        }

        /// <summary>
        /// Checks if a service is registered
        /// </summary>
        /// <typeparam name="T">Service interface type</typeparam>
        /// <returns>True if service is registered</returns>
        public bool HasService<T>() where T : class
        {
            return _services.ContainsKey(typeof(T));
        }

        /// <summary>
        /// Unregisters a service
        /// </summary>
        /// <typeparam name="T">Service interface type</typeparam>
        public void UnregisterService<T>() where T : class
        {
            var serviceType = typeof(T);
            if (_services.Remove(serviceType))
            {
                Debug.Log($"[ServiceContainer] Unregistered service: {serviceType.Name}");
            }
        }

        /// <summary>
        /// Clears all registered services
        /// </summary>
        public void Clear()
        {
            _services.Clear();
        }
    }
}
