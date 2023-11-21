// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ViewModel.cs" company="OpenSky">
// OpenSky project 2021-2023
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace OpenSky.Client.MVVM
{
    using System;
    using System.Collections;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.ComponentModel.DataAnnotations;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Threading.Tasks;
    using System.Windows;

    using JetBrains.Annotations;

    /// -------------------------------------------------------------------------------------------------
    /// <summary>
    /// Standard viewmodel class base, simply allows property change notifications to be sent.
    /// </summary>
    /// <remarks>
    /// sushi.at, 11/03/2021.
    /// </remarks>
    /// -------------------------------------------------------------------------------------------------
    public class ViewModel : INotifyPropertyChanged, INotifyDataErrorInfo
    {
        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The validation errors of the model.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        [NotNull]
        private readonly ConcurrentDictionary<string, List<string>> errors =
            new();

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The validation lock object.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        [NotNull]
        private readonly object validationLock = new();

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Occurs when the validation errors have changed for a property or for the entire entity.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public event EventHandler<DataErrorsChangedEventArgs> ErrorsChanged;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Occurs when Property Changed.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public event PropertyChangedEventHandler PropertyChanged;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets a value indicating whether the entity has validation errors.
        /// </summary>
        /// <value>
        /// true if the entity currently has validation errors; otherwise, false.
        /// </value>
        /// <seealso cref="P:System.ComponentModel.INotifyDataErrorInfo.HasErrors"/>
        /// -------------------------------------------------------------------------------------------------
        public bool HasErrors
        {
            get
            {
                return this.errors.Any(kv => kv.Value is { Count: > 0 });
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the view reference.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public FrameworkElement ViewReference { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the validation errors for a specified property or for the entire entity.
        /// </summary>
        /// <remarks>
        /// sushi.at, 11/03/2021.
        /// </remarks>
        /// <param name="propertyName">
        /// The name of the property to retrieve validation errors for; or null or
        /// <see cref="F:System.String.Empty"/>, to retrieve entity-level errors.
        /// </param>
        /// <returns>
        /// The validation errors for the property or entity.
        /// </returns>
        /// <seealso cref="M:System.ComponentModel.INotifyDataErrorInfo.GetErrors(string)"/>
        /// -------------------------------------------------------------------------------------------------
        [NotNull]
        [ItemNotNull]
        public IEnumerable GetErrors([CanBeNull] string propertyName)
        {
            if (!string.IsNullOrWhiteSpace(propertyName))
            {
                // Return errors for specific property
                this.errors.TryGetValue(propertyName, out var errorsForName);
                return errorsForName ?? new List<string>();
            }

            // Return all errors
            var allErrors = new List<string>();
            foreach (var subErrorList in this.errors.Values)
            {
                allErrors.AddRange(subErrorList);
            }

            return allErrors;
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the validation errors (as strings) for a specified property or for the entire entity.
        /// </summary>
        /// <remarks>
        /// sushi.at, 11/03/2021.
        /// </remarks>
        /// <param name="propertyName">
        /// The name of the property to retrieve validation errors for; or null or
        /// <see cref="F:System.String.Empty"/>, to retrieve entity-level errors.
        /// </param>
        /// <returns>
        /// The validation errors for the property or entity.
        /// </returns>
        /// -------------------------------------------------------------------------------------------------
        [NotNull]
        [ItemNotNull]
        public IEnumerable<string> GetErrorStrings([CanBeNull] string propertyName)
        {
            return (IEnumerable<string>)this.GetErrors(propertyName);
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Raises the property changed event.
        /// </summary>
        /// <remarks>
        /// sushi.at, 11/03/2021.
        /// </remarks>
        /// <param name="propertyName">
        /// Name of the property.
        /// </param>
        /// -------------------------------------------------------------------------------------------------
        [NotifyPropertyChangedInvocator]
        public virtual void NotifyPropertyChanged([CallerMemberName] [NotNull] string propertyName = "")
        {
            // Store the event handler - in case it changes between
            // the line to check it and the line to fire it.
            var propertyChanged = this.PropertyChanged;

            // If the event has been subscribed to, fire it.
            propertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

            this.ValidateAsync();
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Executes the errors changed action.
        /// </summary>
        /// <remarks>
        /// sushi.at, 11/03/2021.
        /// </remarks>
        /// <param name="propertyName">
        /// Name of the property for which the errors have changed.
        /// </param>
        /// -------------------------------------------------------------------------------------------------
        public void OnErrorsChanged([NotNull] string propertyName)
        {
            var handler = this.ErrorsChanged;
            handler?.Invoke(this, new DataErrorsChangedEventArgs(propertyName));
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Validates this object.
        /// </summary>
        /// <remarks>
        /// sushi.at, 11/03/2021.
        /// </remarks>
        /// -------------------------------------------------------------------------------------------------
        public void Validate()
        {
            lock (this.validationLock)
            {
                var validationContext = new ValidationContext(this, null, null);
                var validationResults = new List<ValidationResult>();
                Validator.TryValidateObject(this, validationContext, validationResults, true);

                foreach (var kv in this.errors.ToList())
                {
                    if (validationResults.All(r => r.MemberNames.All(m => m != kv.Key)))
                    {
                        // ReSharper disable once NotAccessedVariable
                        this.errors.TryRemove(kv.Key, out _);
                        this.OnErrorsChanged(kv.Key);
                    }
                }

                var q = from r in validationResults from m in r.MemberNames group r by m into g select g;

                foreach (var prop in q)
                {
                    var messages = prop.Select(r => r.ErrorMessage).ToList();

                    if (this.errors.ContainsKey(prop.Key))
                    {
                        // ReSharper disable once NotAccessedVariable
                        this.errors.TryRemove(prop.Key, out _);
                    }

                    this.errors.TryAdd(prop.Key, messages);
                    this.OnErrorsChanged(prop.Key);
                }
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Validates the object asynchronously.
        /// </summary>
        /// <remarks>
        /// sushi.at, 11/03/2021.
        /// </remarks>
        /// <returns>
        /// The async task handle.
        /// </returns>
        /// -------------------------------------------------------------------------------------------------

        // ReSharper disable once UnusedMethodReturnValue.Global
        [NotNull]
        public Task ValidateAsync()
        {
            return Task.Run(this.Validate);
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the value of a notifying property.
        /// </summary>
        /// <remarks>
        /// sushi.at, 11/03/2021.
        /// </remarks>
        /// <param name="notifyingProperty">
        /// The notifying property.
        /// </param>
        /// <returns>
        /// The value of the notifying property.
        /// </returns>
        /// -------------------------------------------------------------------------------------------------
        [CanBeNull]
        protected object GetValue([NotNull] NotifyingProperty notifyingProperty)
        {
            return notifyingProperty.Value;
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Sets the value of the notifying property.
        /// </summary>
        /// <remarks>
        /// sushi.at, 11/03/2021.
        /// </remarks>
        /// <param name="notifyingProperty">
        /// The notifying property.
        /// </param>
        /// <param name="value">
        /// The value to set.
        /// </param>
        /// <param name="forceUpdate">
        /// If set to <c>true</c> we'll force an update of the binding by calling NotifyPropertyChanged.
        /// </param>
        /// -------------------------------------------------------------------------------------------------
        protected void SetValue(
            [NotNull] NotifyingProperty notifyingProperty,
            [CanBeNull] object value,
            bool forceUpdate = false)
        {
            // We'll only set the value and notify that it has changed if the
            // value is different - or if we are forcing an update.
            if (notifyingProperty.Value != value || forceUpdate)
            {
                // Set the value
                notifyingProperty.Value = value;

                // Notify that the property has changed
                // ReSharper disable once ExplicitCallerInfoArgument
                this.NotifyPropertyChanged(notifyingProperty.Name);
            }
        }
    }
}