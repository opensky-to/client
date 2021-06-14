// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AsynchronousCommand.cs" company="OpenSky">
// OpenSky project 2021
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace OpenSky.Client.MVVM
{
    using System;
    using System.ComponentModel;
    using System.Threading;
    using System.Windows.Threading;

    using JetBrains.Annotations;

    /// -------------------------------------------------------------------------------------------------
    /// <summary>
    /// The AsynchronousCommand is a Command that runs on a thread from the thread pool.
    /// </summary>
    /// <remarks>
    /// sushi.at, 11/03/2021.
    /// </remarks>
    /// <seealso cref="T:JiraWorklog.MVVM.Command"/>
    /// <seealso cref="T:System.ComponentModel.INotifyPropertyChanged"/>
    /// -------------------------------------------------------------------------------------------------
    public class AsynchronousCommand : Command, INotifyPropertyChanged
    {
        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Flag indicated that cancellation has been requested.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private bool isCancellationRequested;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Flag indicating that the command is executing.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private bool isExecuting;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Initializes a new instance of the <see cref="AsynchronousCommand"/> class.
        /// </summary>
        /// <remarks>
        /// sushi.at, 11/03/2021.
        /// </remarks>
        /// <param name="action">
        /// The action.
        /// </param>
        /// <param name="canExecute">
        /// if set to <c>true</c> the command can execute.
        /// </param>
        /// -------------------------------------------------------------------------------------------------
        public AsynchronousCommand([NotNull] Action action, bool canExecute = true)
            : base(action, canExecute)
        {
            // Construct the cancel command.
            this.CancelCommand = new Command(
                () =>
                {
                    // Set the Is Cancellation Requested flag.
                    this.IsCancellationRequested = true;
                });
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Initializes a new instance of the <see cref="AsynchronousCommand"/> class.
        /// </summary>
        /// <remarks>
        /// sushi.at, 11/03/2021.
        /// </remarks>
        /// <param name="parameterizedAction">
        /// The parameterized action.
        /// </param>
        /// <param name="canExecute">
        /// if set to <c>true</c> [can execute].
        /// </param>
        /// -------------------------------------------------------------------------------------------------
        public AsynchronousCommand([NotNull] Action<object> parameterizedAction, bool canExecute = true)
            : base(parameterizedAction, canExecute)
        {
            // Construct the cancel command.
            this.CancelCommand = new Command(
                () =>
                {
                    // Set the Is Cancellation Requested flag.
                    this.IsCancellationRequested = true;
                });
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Occurs when the command is cancelled.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public event EventHandler<CommandEventArgs> Cancelled;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The property changed event.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public new event PropertyChangedEventHandler PropertyChanged;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the cancel command.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        [NotNull]
        public Command CancelCommand { get; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets a value indicating whether this instance is cancellation requested.
        /// </summary>
        /// <value>
        /// True if this instance is cancellation requested; otherwise, false.
        /// </value>
        /// -------------------------------------------------------------------------------------------------
        public bool IsCancellationRequested
        {
            get => this.isCancellationRequested;

            set
            {
                if (this.isCancellationRequested != value)
                {
                    this.isCancellationRequested = value;
                    this.NotifyPropertyChanged(nameof(this.IsCancellationRequested));
                }
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets a value indicating whether this instance is executing.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is executing; otherwise, <c>false</c>.
        /// </value>
        /// -------------------------------------------------------------------------------------------------
        public bool IsExecuting
        {
            get => this.isExecuting;

            set
            {
                if (this.isExecuting != value)
                {
                    this.isExecuting = value;
                    this.NotifyPropertyChanged(nameof(this.IsExecuting));
                }
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the calling dispatcher.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        [CanBeNull]
        protected Dispatcher CallingDispatcher { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Cancels the command if requested.
        /// </summary>
        /// <remarks>
        /// sushi.at, 11/03/2021.
        /// </remarks>
        /// <returns>
        /// True if the command has been cancelled and we must return.
        /// </returns>
        /// -------------------------------------------------------------------------------------------------
        public bool CancelIfRequested()
        {
            // If we haven't requested cancellation, there's nothing to do.
            return this.IsCancellationRequested;
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Executes the command.
        /// </summary>
        /// <remarks>
        /// sushi.at, 11/03/2021.
        /// </remarks>
        /// <param name="param">
        /// The param.
        /// </param>
        /// <seealso cref="M:JiraWorklog.MVVM.Command.DoExecute(object)"/>
        /// -------------------------------------------------------------------------------------------------
        public override void DoExecute(object param)
        {
            // If we are already executing, do not continue.
            if (this.IsExecuting)
            {
                return;
            }

            // Invoke the executing command, allowing the command to be cancelled.
            var args = new CancelCommandEventArgs { Parameter = param, Cancel = false };
            this.InvokeExecuting(args);

            // If the event has been cancelled, bail now.
            if (args.Cancel)
            {
                return;
            }

            // We are executing.
            this.IsExecuting = true;

            // Store the calling dispatcher.
            this.CallingDispatcher = Dispatcher.CurrentDispatcher;

            // Run the action on a new thread from the thread pool (this will therefore work in SL and WP7 as well).
            ThreadPool.QueueUserWorkItem(
                _ =>
                {
                    // Invoke the action.
                    this.InvokeAction(param);

                    // Fire the executed event and set the executing state.
                    this.ReportProgress(
                        () =>
                        {
                            // We are no longer executing.
                            this.IsExecuting = false;

                            // If we were cancelled, invoke the cancelled event - otherwise invoke executed.
                            if (this.IsCancellationRequested)
                            {
                                this.InvokeCancelled(new CommandEventArgs { Parameter = param });
                            }
                            else
                            {
                                this.InvokeExecuted(new CommandEventArgs { Parameter = param });
                            }

                            // We are no longer requesting cancellation.
                            this.IsCancellationRequested = false;
                        });
                });
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Reports progress on the thread which invoked the command.
        /// </summary>
        /// <remarks>
        /// sushi.at, 11/03/2021.
        /// </remarks>
        /// <param name="action">
        /// The action.
        /// </param>
        /// <param name="synchronousExecution">
        /// True to synchronously execute the action.
        /// </param>
        /// -------------------------------------------------------------------------------------------------
        public void ReportProgress([NotNull] Action action, bool synchronousExecution = false)
        {
            if (this.IsExecuting && this.CallingDispatcher != null)
            {
                if (this.CallingDispatcher.CheckAccess())
                {
                    action();
                }
                else
                {
                    if (!synchronousExecution)
                    {
                        this.CallingDispatcher.BeginInvoke(action);
                    }
                    else
                    {
                        this.CallingDispatcher.Invoke(action);
                    }
                }
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Invokes the cancelled event.
        /// </summary>
        /// <remarks>
        /// sushi.at, 11/03/2021.
        /// </remarks>
        /// <param name="args">
        /// The <see cref="CommandEventArgs"/> instance containing the event data.
        /// </param>
        /// -------------------------------------------------------------------------------------------------
        protected void InvokeCancelled([CanBeNull] CommandEventArgs args)
        {
            // Call the cancelled event.
            this.Cancelled?.Invoke(this, args);
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
        private void NotifyPropertyChanged([NotNull] string propertyName)
        {
            // Store the event handler - in case it changes between
            // the line to check it and the line to fire it.
            PropertyChangedEventHandler propertyChanged = this.PropertyChanged;

            // If the event has been subscribed to, fire it.
            propertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}