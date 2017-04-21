// Copyright 2017 voidALPHA, Inc.
// This file is part of the Haxxis video generation system and is provided
// by voidALPHA in support of the Cyber Grand Challenge.
// Haxxis is free software: you can redistribute it and/or modify it under the terms
// of the GNU General Public License as published by the Free Software Foundation.
// Haxxis is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY;
// without even the implied warranty of MERCHANTABILITY or FITNESS FOR A
// PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with
// Haxxis. If not, see <http://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using JetBrains.Annotations;
using Mutation;
using UnityEngine;
using UnityEngine.UI;
using Visualizers;

namespace ChainViews.Elements
{
    public class MutableScopeView : MonoBehaviour, IBoundsChanger
    {

        public event Action BoundsChanged = delegate { };

        public RectTransform BoundsRectTransform { get { return GetComponent<RectTransform>(); } }


        #region Serialized Properties

        [Header("Component References")]

        [SerializeField]
        private Transform m_ItemsRoot = null;
        private Transform ItemsRoot { get { return m_ItemsRoot; } }

        [SerializeField]
        private Transform m_ItemsRootOverlay = null;
        private Transform ItemsRootOverlay { get { return m_ItemsRootOverlay; } }

        [SerializeField]
        private RectTransform m_HoverSizeRect = null;
        private RectTransform HoverSizeRect { get { return m_HoverSizeRect; } }

        [SerializeField]
        private Text m_LabelComponent = null;
        private Text LabelComponent { get { return m_LabelComponent; } }

        [SerializeField]
        private Text m_SelectedItemText = null;
        private Text SelectedItemText { get { return m_SelectedItemText; } }

        [SerializeField]
        private Text m_DropDownButtonTextComponent = null;
        private Text DropDownButtonTextComponent { get { return m_DropDownButtonTextComponent; } }

        [SerializeField]
        private GameObject m_MutableValueDisplayPanel = null;
        private GameObject MutableValueDisplayPanel { get { return m_MutableValueDisplayPanel; } }

        //[SerializeField]
        //private MutableBoxTypeIndicatorBehaviour m_TypeIndicatorTextComponent = null;
        //private MutableBoxTypeIndicatorBehaviour TypeIndicatorTextComponent { get { return m_TypeIndicatorTextComponent; } }

        [SerializeField]
        private List<Image> m_ErrorIndicatingImages = null;
        private List<Image> ErrorIndicatingImages { get { return m_ErrorIndicatingImages; } }

        [SerializeField]
        private RectTransform m_MainContentsPanel = null;
        private RectTransform MainContentsPanel { get { return m_MainContentsPanel; } }
        

        [Header("Prefab References")]

        [SerializeField]
        private GameObject m_MutableItemPrefab = null;
        private GameObject MutableItemPrefab { get { return m_MutableItemPrefab; } }

        [Header("Configuration")]

        [SerializeField]
        private bool m_ShowLabel = true;
        public bool ShowLabel
        {
            get { return m_ShowLabel; }
            set { m_ShowLabel = value; }
        }

        #endregion

        private static string ArraySuffix { get { return "[]"; } }


        private string LabelText
        {
            set
            {
                LabelComponent.text = value;

                gameObject.name = string.Format("Mutable Box ({0})", value);
            }
        }

        private MutableScope Scope { get; set; }

        private ISchemaProvider SchemaProvider { get; set; }

        public void Initialize(PropertyInfo propertyInfo, object propertyInfoResolutionObject, ISchemaProvider schemaProvider)
        {
            var foundScope = propertyInfo.GetValue(propertyInfoResolutionObject, null);
            var assignableScope = foundScope as MutableScope;
            Scope = assignableScope;

            SchemaProvider = schemaProvider;

            LabelText = ControllableUtility.GetControllableLabel(propertyInfo);


            if (!ShowLabel)
            {
                LabelComponent.gameObject.SetActive(false);
                MainContentsPanel.offsetMin = new Vector2(16, MainContentsPanel.offsetMin.y);
            }

            PopulateInitialValue();
        }


        #region Mutable Drop Down Items and Selection

        private readonly List<MutableBoxMutableItemBehaviour> m_MutableDropDownItems = new List<MutableBoxMutableItemBehaviour>();
        private List<MutableBoxMutableItemBehaviour> MutableDropDownItems
        {
            get { return m_MutableDropDownItems; }
        }

        private void HandleMutableItemSelected(MutableBoxMutableItemBehaviour item)
        {
            IndicateMutableValue(item);

            ShowItemDropDown = false;

            //Debug.Log("Assigning Absolute Key: " + item.AbsoluteKey);
        }

        private void IndicateMutableValue(MutableBoxMutableItemBehaviour item)
        {
            SetSelectedMutableText(item.UserFacingAbsoluteKey);

            var segmentCount = item.UserFacingAbsoluteKey.Count(c => c.Equals('.')) + 1;
            GetComponent<LayoutElement>().preferredHeight = segmentCount * 16;

            // ArraySuffix never leaves this class!
            var arrayFreeText = item.AbsoluteKey.Replace(ArraySuffix, "");

            SchemaProvider.CacheSchema();
            Scope.AbsoluteKey = arrayFreeText;
            SchemaProvider.UnCacheSchema();

            SchemaProvider.CacheSchema();
            IndicateError = !Scope.ValidateKey(SchemaProvider.Schema);
            SchemaProvider.UnCacheSchema();

            SwitchDisplayToMutableValue();

            //ShowItemDropDown = false;
        }
        
        private void SetSelectedMutableText(string absoluteKey)
        {
            var tokens = absoluteKey.Split('.');
            var text = new StringBuilder();
            var index = 0;

            foreach (var t in tokens)
            {
                for (var i = 0; i < index; i++)
                    text.Append("  ");

                text.Append(t);

                if (index != tokens.Count() - 1)
                    text.Append("\n");

                index++;
            }

            SelectedItemText.text = text.ToString();
        }


        #endregion

        #region General Drop Down Handling

        [UsedImplicitly]
        public void HandleArrowClicked()
        {
            ShowItemDropDown = !ShowItemDropDown;
        }

        private bool m_showDropDown = false;
        private bool ShowItemDropDown
        {
            get
            {
                return m_showDropDown;
            }
            set
            {
                // populate items!
                if (value)
                    PopulateOptions();
                else
                    DestroyMutableDropDownItems();

                ItemsRootOverlay.gameObject.SetActive(value);
                m_showDropDown = value;
                ChainView.Instance.AllowMouse = !value;

                DropDownButtonTextComponent.text = value ? "▲" : "▼";
            }
        }

        #endregion

        #region UI/State Management

        private void HandleValidationErrorState(bool valid)
        {
            IndicateError = !valid;
        }

        private void SwitchDisplayToMutableValue()
        {
            MutableValueDisplayPanel.SetActive(true);

            BoundsChanged();
        }

        private void PopulateInitialValue()
        {
            SetInitialMutableValue();
        }

        private void SetInitialMutableValue()
        {
            //SwitchDisplayToMutableValue();

            IndicateError = false;

            var itemGo = Instantiate( MutableItemPrefab );
            var item = itemGo.GetComponent<MutableBoxMutableItemBehaviour>();

            item.IsGlobalParameter = false;

            item.AbsoluteKey = Scope.AbsoluteKey;

            item.IsValidType = true;

            MutableDropDownItems.Add( item );

            IndicateMutableValue( item );
        }

        private bool IndicateError
        {
            set
            {
                var backgroundColor = value ? Color.red : Color.white;
                ErrorIndicatingImages.ForEach(i => i.color = backgroundColor);
            }
        }

        #endregion


        [UsedImplicitly]
        private void Start()
        {
            ShowItemDropDown = false;

            Scope.KeyValidChanged += HandleValidationErrorState;
        }

        [UsedImplicitly]
        private void OnDestroy()
        {
            Scope.KeyValidChanged -= HandleValidationErrorState;
        }

        [UsedImplicitly]
        private void Update()
        {
            if ( ChainView.Instance.Zooming || ChainView.Instance.Dragging ) return;

            CheckMouseInBounds();
        }

        private void CheckMouseInBounds()
        {
            // NOTE: Likely only works for screen space overlay rendermode.

            if (!ShowItemDropDown)
                return;

            var corners = new Vector3[4];
            HoverSizeRect.GetWorldCorners(corners);

            var mouseLoc = Vector3.zero;
            RectTransformUtility.ScreenPointToWorldPointInRectangle((RectTransform)transform, Input.mousePosition,
                ChainView.Instance.Camera, out mouseLoc);

            var mouseX = mouseLoc.x;
            var mouseY = mouseLoc.y;

            var inBounds = corners.Any(c => c.x > mouseX) && corners.Any(c => c.x < mouseX)
                           && corners.Any(c => c.y > mouseY) && corners.Any(c => c.y < mouseY);
            if (!inBounds)
                ShowItemDropDown = false;
        }


        private void PopulateOptions()
        {
            DestroyMutableDropDownItems();

            // generate items from field's schema

            EvaluateMutableValueForAdding("", SchemaProvider.Schema, false);
            GenerateMutableDropDownItems(MutableObject.IntersectOwnSchema( SchemaProvider.Schema ), new Stack<string>(), false);

            // generate items from the global variable store's schema
        }


        private void DestroyMutableDropDownItems()
        {
            foreach (var c in MutableDropDownItems)
            {
                Destroy(c.gameObject);
            }

            MutableDropDownItems.Clear();
        }


        private void GenerateMutableDropDownItems(MutableObject schemaSegment, Stack<string> ancestorTokens, bool isGlobal)
        {
            if (schemaSegment == null)
                return;

            foreach (var pair in schemaSegment)
            {
                var ancestorKeys = ancestorTokens.Any()
                    ? ancestorTokens.Reverse().Aggregate((accumulator, current) => accumulator + "." + current) +
                        "."
                    : string.Empty;

                if (pair.Value is MutableObject)
                {
                    EvaluateMutableValueForAdding(ancestorKeys + pair.Key, pair.Value, isGlobal);

                    ancestorTokens.Push(pair.Key);

                    GenerateMutableDropDownItems(pair.Value as MutableObject, ancestorTokens, isGlobal);

                    ancestorTokens.Pop();
                }
                else if (pair.Value is ICollection<MutableObject>)
                {
                    EvaluateMutableValueForAdding(ancestorKeys + pair.Key + ArraySuffix, pair.Value, isGlobal);

                    var enumerable = pair.Value as ICollection<MutableObject>;

                    if (enumerable.Any())
                    {
                        ancestorTokens.Push(pair.Key + ArraySuffix);

                        GenerateMutableDropDownItems(enumerable.First(), ancestorTokens, isGlobal);

                        ancestorTokens.Pop();
                    }
                }
                else if (pair.Value is TypeCollisionList)
                {
                    EvaluateMutableValueForAdding(ancestorKeys + pair.Key, pair.Value, isGlobal);

                    var colList = pair.Value as TypeCollisionList;

                    foreach (var obj in colList.Objects)
                    {
                        if (obj is MutableObject)
                        {
                            ancestorTokens.Push(pair.Key);

                            GenerateMutableDropDownItems(obj as MutableObject, ancestorTokens, isGlobal);

                            ancestorTokens.Pop();

                            continue;
                        }

                        if (obj is ICollection<MutableObject>)
                        {
                            var enumerable = obj as ICollection<MutableObject>;

                            if (!enumerable.Any()) continue;

                            ancestorTokens.Push(pair.Key + ArraySuffix);

                            GenerateMutableDropDownItems(enumerable.First(), ancestorTokens, isGlobal);

                            ancestorTokens.Pop();
                        }
                    }

                }
                else
                {
                    EvaluateMutableValueForAdding(ancestorKeys + pair.Key, pair.Value, isGlobal);
                }
            }
        }

        private void EvaluateMutableValueForAdding(string absoluteKey, object value, bool isGlobal)
        {
            if (value == null)
            {
                Debug.Log("Indicating error because the type of the mutable object is null.");
                IndicateError = true;
                return;
            }

            var collisionList = value as TypeCollisionList;
            bool isValidType;
            if (collisionList == null)
                isValidType = (typeof(MutableObject)).IsInstanceOfType(value)
                    || (typeof(IEnumerable<MutableObject>)).IsInstanceOfType(value);
            else
                isValidType = collisionList.ContainsType(typeof(MutableObject)) || collisionList.ContainsType( typeof(IEnumerable<MutableObject> ) );

            AddMutableItem(absoluteKey, isValidType, isGlobal, value);
        }

        private void AddMutableItem(string absoluteKey, bool isValid, bool isGlobal, object value)
        {
            var itemGo = Instantiate(MutableItemPrefab);
            var item = itemGo.GetComponent<MutableBoxMutableItemBehaviour>();

            item.IsGlobalParameter = isGlobal;

            item.SchemaSource = SchemaSource.Mutable;

            item.AbsoluteKey = absoluteKey;

            if ( value != null )
                item.Type = value.GetType();
            else
                item.Type = null;

            item.IsValidType = isValid;

            item.Selected += HandleMutableItemSelected;

            item.transform.SetParent(ItemsRoot, false);

            MutableDropDownItems.Add(item);
        }
    }
}
