﻿using System.Linq;

namespace OnUtils.Application.Modules.Extensions
{
    using Architecture.AppCore;
    using Modules;

#pragma warning disable CS1591 // todo внести комментарии.
    public class ModuleExtension : CoreComponentBase<ApplicationCore>, IComponentTransient<ApplicationCore>
    {
        public      string _mModuleName;
        public      int _mModuleID = 0;
        public      string _mCheckDataIDObj = null;
        public      string _mCheckFuncName = "";
        public      string _GetDataFuncName = "";

        public      int ModuleID_Override = 0;

        internal protected ModuleCore Module = null;
        protected object DataPostResult = null;
        public object ControllerBase = null;
        public object ModelStateBase = null;

        public ModuleExtension()
        {
            var attrs = GetType().GetCustomAttributes(typeof(ModuleExtensionAttribute), true);
            if (attrs != null && attrs.Length > 0)
            {
                var attr = attrs.FirstOrDefault() as ModuleExtensionAttribute;
                if (attr != null)
                {
                    Attributes = attr;
                }
            }
        }

        #region CoreComponentBase
        /// <summary>
        /// </summary>
        protected sealed override void OnStart()
        {
        }

        /// <summary>
        /// </summary>
        protected sealed override void OnStop()
        {
        }
        #endregion

        internal virtual void _initializeCustoms()
        {
        }

        #region Property
        /*
         * Атрибуты текущего расширения
         * */
        public ModuleExtensionAttribute Attributes
        {
            get;
            private set;
        } = new ModuleExtensionAttribute("", false);

        #endregion

        /////*
        //// * Устанавливаем ссылку на объект, который будет искать запись с id в базе.
        //// * */
        ////public void _setDataIDHandler()
        ////{
        ////    if (!is_object($handler) || !method_exists($handler,$this._mCheckFuncName)) return false;
        ////    $this._mCheckDataIDObj = $handler;
        ////}

        /////*
        ////Проверка существования записи с таким id в базе
        ////*/
        ////public function _checkDataID($id=0)
        ////{
        ////    if ($this._mCheckDataIDObj != NULL) 
        ////    {
        ////        return call_user_func_array(array($this._mCheckDataIDObj, $this._mCheckFuncName), array($id));
        ////    }
        ////    if (method_exists($this.Module, "_checkDataID")) 
        ////    {
        ////        return call_user_func_array(array($this.Module, "_checkDataID"), array($id));
        ////    }
        ////    return "Не удалось найти объект в базе!";
        ////}

        public virtual Types.NestedLinkCollection getAdminMenu()
        {
            return null;
        }

        protected int GetModuleID()
        {
            return this.ModuleID_Override > 0 ? this.ModuleID_Override : this.Module.ID;
        }

    }

}