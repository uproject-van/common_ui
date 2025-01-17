using System;
using System.Collections.Generic;
using RTLTMPro;
using UnityEngine;
using WLDDZ;

namespace NGame
{
    /// <summary>
    /// 一个简单的计数器 - 支持拖拽slider 改变数量
    /// </summary>
    public class PropCounterMono : _ABaseUIEntity
    {
        [Header("增加按钮")]
        public ClickButton increaseBtn;

        [Header("减少按钮")]
        public ClickButton decreaseBtn;

        [Header("单次增加多个按钮")]
        public ClickButton addLotBtn;

        [Header("单次减少多个按钮")]
        public ClickButton reduceLotBtn;

        [Header("单次增加的数量")]
        public int perCount = 10;

        [Header("最大数量按钮")]
        public ClickButton maxBtn;

        [Header("最小数量按钮")]
        public ClickButton minBtn;

        [Header("当前使用数量 xN")]
        public RTLTextMeshPro curCountTxt;

        [Header("达到最大数量时需要隐藏的列表")]
        public List<GameObject> maxHideGoList;

        [Header("达到最小数量时需要隐藏的列表")]
        public List<GameObject> minHideGoList;

        [Header("选择数量进度条")]
        public CommonSlider selectCountSlider;

        private Action<long> _m_dCountChangedEvent = null;
        private long _m_iSellCount = 0;
        private long _m_iTotalCount = 0;
        private long _m_perCount;

        //最大使用限制 99 
        private int _m_maxCount = 99;

        protected override void _OnInitEx()
        {
            _m_perCount = perCount;
            if (null != decreaseBtn)
                decreaseBtn.SetOnClickListener(_onDecreaseBtnClick);
            if (null != increaseBtn)
                increaseBtn.SetOnClickListener(_onIncreaseBtnClick);
            if (null != addLotBtn)
                addLotBtn.SetOnClickListener(_onAddLotBtnClick);
            if (null != reduceLotBtn)
                reduceLotBtn.SetOnClickListener(_onReduceLotBtnClick);
            if (null != minBtn)
                minBtn.SetOnClickListener(_onMinBtnClick);
            if (null != maxBtn)
                maxBtn.SetOnClickListener(_onMaxBtnClick);
        }

        protected override void _OnDestroyEx()
        {
            _m_dCountChangedEvent = null;
            _m_iSellCount = 0;
            _m_iTotalCount = 0;

            if (null != decreaseBtn)
                decreaseBtn.RemoveAllListeners();
            if (null != increaseBtn)
                increaseBtn.RemoveAllListeners();
            if (null != addLotBtn)
                addLotBtn.RemoveAllListeners();
            if (null != reduceLotBtn)
                reduceLotBtn.RemoveAllListeners();
            if (null != minBtn)
                minBtn.RemoveAllListeners();
            if (null != maxBtn)
                maxBtn.RemoveAllListeners();
        }

        protected override void _OnEnableEx()
        {
            //监听滑块滑动事件
            if (null != selectCountSlider)
            {
                selectCountSlider.onValueChanged.AddListener(_onSliderValueChg);
            }
        }

        protected override void _OnDisableEx()
        {
            //监听滑块滑动事件
            if (null != selectCountSlider)
            {
                selectCountSlider.onValueChanged.RemoveAllListeners();
            }
        }


        /// <summary>
        /// 带物品的初始化
        /// </summary>
        /// <param name="_good"></param>
        /// <param name="_useMaxCount"></param>
        public void init(GoodData _good,long _useMaxCount = -1)
        {
            _m_iSellCount = 1;
            if (_useMaxCount != -1 && _useMaxCount > 1)
                _m_maxCount = (int)_useMaxCount;

            _getCostItemCount(_good);

            setShowInfo(_m_iSellCount);
        }

        /// <summary>
        ///带个数的初始化 
        /// </summary>
        /// <param name="_totalCount"></param>
        public void init(long _totalCount)
        {
            _m_iSellCount = 1;
            _m_iTotalCount = _totalCount;
            if (_m_iTotalCount > _m_maxCount)
                _m_iTotalCount = _m_maxCount;

            setShowInfo(_m_iSellCount);
        }
        
        //获取最大使用个数 - 如果待消耗的话方便计算
        private void _getCostItemCount(GoodData _good)
        {
            long count = 0;
            if(null != _good && _good.propCount > 0)
                count = _good.propCount;
            //道具拥有的数量 
            _m_iTotalCount = count;
            if (_m_iTotalCount > _m_maxCount)
                _m_iTotalCount = _m_maxCount;
        }
        
        // 响应减少按钮点击事件
        private void _onDecreaseBtnClick()
        {
            if (_m_iSellCount <= 1)
                return;

            _m_iSellCount--;
            _onSellCountChanged(_m_iSellCount);

            UGUICommon.setGameObjEnable(minBtn, true);
        }

        // 响应增加按钮点击事件
        private void _onIncreaseBtnClick()
        {
            if (_m_iSellCount >= _m_iTotalCount)
                return;

            _m_iSellCount++;
            _onSellCountChanged(_m_iSellCount);

            UGUICommon.setGameObjEnable(maxBtn, true);
        }

        // 响应单次增加多个按钮点击事件
        private void _onAddLotBtnClick()
        {
            if (_m_iSellCount >= _m_iTotalCount)
                return;
            if (_m_iSellCount + _m_perCount >= _m_iTotalCount)
                _m_iSellCount = _m_iTotalCount;
            else
                _m_iSellCount = _m_iSellCount + _m_perCount;

            _onSellCountChanged(_m_iSellCount);
            UGUICommon.setGameObjEnable(maxBtn, true);
        }

        // 响应单次减少多个按钮点击事件
        private void _onReduceLotBtnClick()
        {
            if (_m_iSellCount <= 1)
                return;
            if (_m_iSellCount - _m_perCount <= 1)
                _m_iSellCount = 1;
            else
                _m_iSellCount = _m_iSellCount - _m_perCount;
            _onSellCountChanged(_m_iSellCount);
            UGUICommon.setGameObjEnable(minBtn, true);
        }

        // 响应最大数量按钮点击事件
        private void _onMaxBtnClick()
        {
            if (_m_iSellCount == _m_iTotalCount)
                return;

            _m_iSellCount = _m_iTotalCount;
            if (_m_iSellCount == 0)
                _m_iSellCount = 1;
            _onSellCountChanged(_m_iSellCount);
        }

        // 响应最小数量按钮点击事件
        private void _onMinBtnClick()
        {
            if (_m_iSellCount == 1)
                return;

            _m_iSellCount = 1;
            _onSellCountChanged(_m_iSellCount);
        }

        // 注册计数器更改事件
        public void regCounterChangedEvent(Action<long> _event)
        {
            if (_event == null)
                return;

            if (_m_dCountChangedEvent == null)
                _m_dCountChangedEvent = _event;
            else
                _m_dCountChangedEvent += _event;
        }

        // 响应计数器更改事件
        private void _onSellCountChanged(long _newCount)
        {
            setShowInfo(_newCount);
            if (_m_dCountChangedEvent != null)
                _m_dCountChangedEvent(_newCount);
        }

        //设置显示数据
        private void setShowInfo(long _count)
        {
            UGUICommon.setLabelTxt(curCountTxt, "x" + _count);

            if (_count >= _m_iTotalCount)
                UGUICommon.setGameObjEnable(maxHideGoList, false);
            if (_count <= 1)
                UGUICommon.setGameObjEnable(minHideGoList, false);
        }

        private void _onSliderValueChg(float _value)
        {
            if (selectCountSlider == null)
                return;

            //取整
            int curCount = (int)Math.Round(_m_iTotalCount * _value);
            if (curCount < 1)
                curCount = 1;

            _m_iSellCount = curCount;
            _onSellCountChanged(_m_iSellCount);
        }
    }
}