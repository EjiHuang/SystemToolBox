using System;
using System.ComponentModel;
using System.Linq.Expressions;

namespace SystemToolBox.Framwork
{
    public class BaseNotifyPropertyChanged : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Fire this event when a Property has changed
        /// Notify all obj that are listining
        /// </summary>
        protected void FirePropertyChanged<T>(Expression<Func<T>> propertyExpression)
        {
            var propertyName = ((MemberExpression) propertyExpression.Body).Member.Name;
            if (null != PropertyChanged)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
