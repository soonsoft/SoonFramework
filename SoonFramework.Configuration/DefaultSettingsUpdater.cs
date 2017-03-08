using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using SoonFramework.Data;
using System.Runtime.InteropServices;

namespace SoonFramework.Configuration
{
    public class DefaultSettingsUpdater : IUpdater
    {
        private bool m_disposed = false;
        private Timer m_timer = null;
        private IDictionary<string, long> m_settingChangeValues = null;
        private ApplicationSettingsDataProvider m_dataProvider = null;

        /// <summary>
        /// 系统设置更新委托
        /// </summary>
        public Action<string[]> SettingsUpdateAction { get; set; }

        private int m_pollingInterval = 60;
        /// <summary>
        /// 轮询间隔 默认1分钟，单位(秒)
        /// </summary>
        public int PollingInterval {
            get
            {
                return m_pollingInterval;
            }
            set
            {
                if (value < 1)
                    m_pollingInterval = 1;
                else
                    m_pollingInterval = value;
            }
        }

        public DefaultSettingsUpdater(ApplicationSettingsDataProvider provider)
        {
            m_dataProvider = provider;
        }

        public void Start()
        {
            if(m_timer != null)
            {
                return;
            }
            m_timer = new Timer(stateInfo => 
            {
                DefaultSettingsUpdater that = (DefaultSettingsUpdater)stateInfo;
                try
                {
                    that.CheckSettingValue();
                }
                catch(Exception e)
                {
                    Console.WriteLine(e);
                }
            }, this, TimeSpan.Zero, TimeSpan.FromMilliseconds(m_pollingInterval * 1000));
        }

        protected void CheckSettingValue()
        {
            IDictionary<string, long> values = m_dataProvider.GetSettingChangeValues();
            if(m_settingChangeValues == null)
            {
                m_settingChangeValues = values;
                return;
            }
            List<string> changedKeys = new List<string>();
            foreach(var item in values)
            {
                if(m_settingChangeValues[item.Key] != item.Value)
                {
                    changedKeys.Add(item.Key);
                }
            }
            if(changedKeys.Count > 0)
            {
                OnCheckSettingValueChanged(changedKeys.ToArray());
            }
        }

        protected void OnCheckSettingValueChanged(string[] changedKeys)
        {
            if(SettingsUpdateAction != null)
            {
                SettingsUpdateAction(changedKeys);
            }
        }

        //使用此类的地方需要显示调用这个方法或使用using
        public void Dispose()
        {
            //释放托管资源，同时是否非托管资源
            Dispose(true);
            //告诉GC不用再调用析构函数了
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if ((disposing) && (m_timer != null))
            {
                //释放托管资源
                m_timer.Dispose();
            }
            m_disposed = true;
            m_timer = null;
        }

        protected void ThrowIfDisposed()
        {
            if (this.m_disposed)
            {
                throw new ObjectDisposedException(base.GetType().Name);
            }
        }
    }
}
