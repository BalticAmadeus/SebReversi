using System;
using System.Text;
using System.Windows;

namespace Game.AdminClient.Infrastructure
{
    public class MessageBoxDialogService : IMessageBoxDialogService
    {
        public bool OpenDialog(string message, string title = null, MessageBoxButton button = MessageBoxButton.OK)
        {
            var messageBoxResult = MessageBox.Show(message, title, button);
            return messageBoxResult == MessageBoxResult.OK || messageBoxResult == MessageBoxResult.Yes;
        }

        public bool ShowException(Exception e)
        {
            return OpenDialog(OpenException(e), "Error");
        }

        private string OpenException(Exception e)
        {
            if (e == null)
                return "";

            if (e is ViewModels.ValidationException)
                return e.Message;

            if (e is AggregateException)
            {
                var aggregate = (AggregateException)e;
                var msg = new StringBuilder();
                foreach (Exception inner in aggregate.InnerExceptions)
                {
                    if (msg.Length != 0)
                        msg.Append("-------------------\n\n");
                    msg.Append(OpenException(inner));
                }
                return msg.ToString();
            }

            if (e is ServerMessageException)
            {
                var se = (ServerMessageException)e;
                return $"Server returned:\n[{se.Status}] {se.Message}";
            }

            /* default */
            return $"[{e.GetType()}]\n{e.Message}\n\n{OpenException(e.InnerException)}";
        }
    }
}