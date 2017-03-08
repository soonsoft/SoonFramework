using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SoonFramework.Core
{
    public static class AsyncHelper
    {
        private static readonly TaskFactory _myTaskFactory;

        static AsyncHelper()
        {
            _myTaskFactory = new TaskFactory(
                CancellationToken.None, 
                TaskCreationOptions.None, 
                TaskContinuationOptions.None, 
                TaskScheduler.Default);
        }

        public static void RunSync(Func<Task> func)
        {
            CultureInfo cultureUi = CultureInfo.CurrentUICulture;
            CultureInfo culture = CultureInfo.CurrentCulture;
            _myTaskFactory.StartNew<Task>(() =>
            {
                Thread.CurrentThread.CurrentCulture = culture;
                Thread.CurrentThread.CurrentUICulture = cultureUi;
                return func();
            }).Unwrap().GetAwaiter().GetResult();

        }

        public static TResult RunSync<TResult>(Func<Task<TResult>> func)
        {
            CultureInfo cultureUi = CultureInfo.CurrentUICulture;
            CultureInfo culture = CultureInfo.CurrentCulture;
            return _myTaskFactory.StartNew<Task<TResult>>(() =>
            {
                Thread.CurrentThread.CurrentCulture = culture;
                Thread.CurrentThread.CurrentUICulture = cultureUi;
                return func();
            }).Unwrap<TResult>().GetAwaiter().GetResult();

        }
    }
}
