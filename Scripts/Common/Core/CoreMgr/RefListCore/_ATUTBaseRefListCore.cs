﻿using System;
using UnityEngine;
using Object = UnityEngine.Object;

namespace UTGame
{
    /*********************
     * 数据集合中心管理对象
     **/
    public abstract class _ATUTBaseRefListCore<T> : _TUTBaseRefListCore<T>, _IUTInitRefObj where T : _IUTBaseRefObj
    {
        /** 初始化状态位 */
        private bool _m_bInit;
        /** 回调处理函数 */
        private Action _m_dDelegate;
        private Action<Type, _IUTInitRefObj> _m_dFailDelegate;

        protected _ATUTBaseRefListCore()
        {
            _m_bInit = false;
        }

        public bool isInit { get { return _m_bInit; } }
        public Action finalDelegate { get { return _m_dDelegate; } }

        /***********
         * 初始化操作，开始下载并加载对应信息
         **/
        public void init(Action _doneDelegate, Action<Type, _IUTInitRefObj> _onFail)
        {
            if (_m_bInit)
            {
                if (null != _doneDelegate)
                    _doneDelegate();
                _reset();
                return;
            }

            //设置回调
            _m_dDelegate = _doneDelegate;
            _m_dFailDelegate = _onFail;

            _m_bInit = true;
            //开始加载
            UTLocalResLoaderMgr.instance.loadRefdataObjAsset< _TUTSOBaseRefSet<T>>(_assetPath, _objName, _onAssetLoaded, null, _onLocalObjLoaded);

        }

        public void discard()
        {
            _m_bInit = false;
            _clear();
            _reset();
        }

        /*******************
         * 场景信息加载完后的处理函数
         **/
        public void _onAssetLoaded(bool _isSuc, ALAssetBundleObj _assetObj)
        {
            if (!_isSuc)
            {
#if UNITY_EDITOR
                Debug.LogWarning(ToString() + " Basic Ref Download err!");
#endif
                _dealFailCallback();
                return;
            }

            //获取对象
            _TUTSOBaseRefSet<T> refSetObj = _assetObj.load(_objName) as _TUTSOBaseRefSet<T>;
            if (null == refSetObj)
            {
#if UNITY_EDITOR
                Debug.LogWarning(ToString() + " Basic Ref Load err!");
#endif
                _dealFailCallback();
                return;
            }

            //初始化数据
            initData(refSetObj);

            //调用回调处理
            _dealSuccessCallback();
        }
#if UNITY_EDITOR
        public void _onLocalObjLoaded(Object _obj)
        {
            if(null == _obj)
            {
#if UNITY_EDITOR
                Debug.LogWarning(ToString() + " Basic Ref Download err!");
#endif
                _dealFailCallback();
                return;
            }

            //获取对象
            _TUTSOBaseRefSet<T> refSetObj = _obj as _TUTSOBaseRefSet<T>;
            if(null == refSetObj)
            {
#if UNITY_EDITOR
                Debug.LogWarning(ToString() + " Basic Ref Load err!");
#endif
                _dealFailCallback();
                return;
            }

            //初始化数据
            initData(refSetObj);

            //调用回调处理
            _dealSuccessCallback();
        }
#endif

        protected void _dealSuccessCallback()
        {
            try
            {
                if(null != _m_dDelegate)
                    _m_dDelegate();
            }
            finally
            {
                _reset(); //try finally防止调用过程中有异常导致没置空
            }
        }

        protected void _dealFailCallback()
        {
            try
            {
                if(null != _m_dFailDelegate)
                    _m_dFailDelegate(typeof(T), this);
            }
            finally
            {
                _reset(); //try finally防止调用过程中有异常导致没置空
            }
        }
        
        public void forceDone()
        {
            try
            {
                if(null != _m_dDelegate)
                    _m_dDelegate();
            }
            finally
            {
                _reset(); //try finally防止调用过程中有异常导致没置空
            }
        }

        protected void _reset()
        {
            _m_dDelegate = null;
            _m_dFailDelegate = null;
        }
        
        /** 获取加载资源对象的路径 */
        protected abstract string _assetPath { get; }
        protected abstract string _objName { get; }
    }
}