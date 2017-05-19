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
using Adapters.GlobalParameters;
using JetBrains.Annotations;
using Mutation;
using UnityEngine;
using UnityEngine.UI;
using Utility;
using Utility.InputManagement;
using Utility.Undo;
using Visualizers;

namespace ChainViews.Elements
{
    public class MutableBoxBehaviour : MonoBehaviour, IBoundsChanger
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
        private GameObject m_LiteralValueDisplayPanel = null;
        private GameObject LiteralValueDisplayPanel { get { return m_LiteralValueDisplayPanel; } }

        [SerializeField]
        private GameObject m_MutableValueDisplayPanel = null;
        private GameObject MutableValueDisplayPanel { get { return m_MutableValueDisplayPanel; } }
        
        [SerializeField]
        private InputField m_LiteralValueInputFieldComponent = null;
        private InputField LiteralValueInputFieldComponent { get { return m_LiteralValueInputFieldComponent; } }

        [SerializeField]
        private MutableBoxTypeIndicatorBehaviour m_TypeIndicatorTextComponent = null;
        private MutableBoxTypeIndicatorBehaviour TypeIndicatorTextComponent { get { return m_TypeIndicatorTextComponent; } }

        [SerializeField]
        private List< Image > m_ErrorIndicatingImages = null;
        private List< Image > ErrorIndicatingImages { get { return m_ErrorIndicatingImages; } }

        [SerializeField]
        private RectTransform m_MainContentsPanel = null;
        private RectTransform MainContentsPanel { get { return m_MainContentsPanel; } }


        [Header("Prefab References")]

        [SerializeField]
        private GameObject m_MutableItemPrefab = null;
        private GameObject MutableItemPrefab { get { return m_MutableItemPrefab; } }

        //[SerializeField]
        //private GameObject m_StringBasedLiteralItemPrefab;
        //private GameObject StringBasedLiteralItemPrefab { get { return m_StringBasedLiteralItemPrefab; } }


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

                gameObject.name = string.Format( "Mutable Box ({0})", value );
            }
        }

        private bool SuppressLiteralEvaluation { get; set; }

        private Type m_Type;
        private Type Type
        {
            get { return m_Type; }
            set
            {
                m_Type = value;

                TypeIndicatorTextComponent.Type = m_Type;
            }
        }

        private IMutableField MutableField { get; set; }

        private ISchemaProvider SchemaProvider { get; set; }

        private Vector3[] m_Corners = new Vector3[4];
        private Vector3[] Corners { get { return m_Corners; } }

        public void Initialize( PropertyInfo propertyInfo, object propertyInfoResolutionObject, ISchemaProvider schemaProvider )
        {
            var foundMutableField = propertyInfo.GetValue( propertyInfoResolutionObject, null );
            var assignableMutableField = foundMutableField as IMutableField;
            MutableField = assignableMutableField;

            SchemaProvider = schemaProvider;

            Type = propertyInfo.PropertyType.GetGenericArguments().First();

            var controllableAttribute = propertyInfo.GetCustomAttributes( typeof( ControllableAttribute ), true ).Cast<ControllableAttribute>().FirstOrDefault();
            if ( controllableAttribute != null )
            {
                ConnectValidValuesProperty( propertyInfoResolutionObject, controllableAttribute );
            }

            LabelText = ControllableUtility.GetControllableLabel( propertyInfo );


            if ( !ShowLabel )
            {
                LabelComponent.gameObject.SetActive( false );
                MainContentsPanel.offsetMin = new Vector2( 16, MainContentsPanel.offsetMin.y );
            }

            PopulateInitialValue();
        }

        private void ConnectValidValuesProperty( object propertyInfoResolutionObject, ControllableAttribute controllableAttribute )
        {
            if ( string.IsNullOrEmpty( controllableAttribute.ValidValuesListName ) )
                return;

            var foundProperty =
                propertyInfoResolutionObject.GetType()
                    .GetProperty( controllableAttribute.ValidValuesListName,
                        BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic );
            
            if ( foundProperty == null )
                return;
            

            TypeIndicatorTextComponent.ValidValuesCallback = () =>
            {
                var value = foundProperty.GetValue( propertyInfoResolutionObject, null ) as List< string >;

                return value;
            };
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

        //private void HandleGlobalParameterSelected( MutableBoxMutableItemBehaviour item )
        //{
        //    IndicateGlobalValue(item);
        //
        //    ShowItemDropDown = false;
        //}

        private void IndicateMutableValue(MutableBoxMutableItemBehaviour item)
        {
            SetSelectedMutableText(item.UserFacingAbsoluteKey);

            var segmentCount = item.UserFacingAbsoluteKey.Count(c => c.Equals('.')) + 1;
            GetComponent<LayoutElement>().preferredHeight = segmentCount * 16;

            UndoLord.Instance.Push(new MutableFieldChange(this, MutableField, item.UserFacingAbsoluteKey));

            // ArraySuffix never leaves this class!
            var arrayFreeText = item.AbsoluteKey.Replace(ArraySuffix, "");

            SchemaProvider.CacheSchema();
            MutableField.AbsoluteKey = arrayFreeText;

            MutableField.SchemaSource = item.SchemaSource;

            //////
            //MutableField.UseMutableValue = !item.useCachedData?
            //////

            //MutableField.UseMutableData = true;

            bool fieldValid;
            
            try
            {
                fieldValid = MutableField.ValidateKey(SchemaProvider.Schema);
            }
            catch ( NullReferenceException )
            {
                fieldValid = false;
            }
            SchemaProvider.UnCacheSchema();

            IndicateError = !fieldValid;

            SwitchDisplayToMutableValue();

            //ShowItemDropDown = false;
        }
        
        private void SetSelectedMutableText( string absoluteKey )
        {
            var tokens = absoluteKey.Split( '.' );
            var text = new StringBuilder();
            var index = 0;

            foreach ( var t in tokens )
            {
                for ( var i = 0; i < index; i++ )
                    text.Append( "  " );

                text.Append( t );

                if ( index != tokens.Count() - 1 )
                    text.Append( "\n" );

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

                ItemsRootOverlay.gameObject.SetActive( value );
                m_showDropDown = value;
                ChainView.Instance.AllowMouse = !value;

                DropDownButtonTextComponent.text = value ? "▲" : "▼";
            }
        }

        #endregion


        #region Literal Value Stuff

        [UsedImplicitly]
        public void HandleLiteralValueSelected()
        {
            // This merely switches to using literal values--nothing to do with the value itself!
            
            UndoLord.Instance.Push(new MutableFieldChange(this, MutableField, "Literal." + MutableField.GetLiteralValueAsString()));

            SwitchDisplayToLiteralValue();
        }


        private void IndicateLiteralValue()
        {
            SuppressLiteralEvaluation = true;

            LiteralValueInputFieldComponent.text = MutableField.GetLiteralValueAsString();

            SuppressLiteralEvaluation = false;
        }

        [UsedImplicitly]
        public void HandleLiteralValueCheck()
        {
            ChangeLiteralValue( false );
        }

        [UsedImplicitly]
        public void HandleLiteralValueApply()
        {
            ChangeLiteralValue( true );
        }

        private void ChangeLiteralValue(bool applyValue)
        {
            if (SuppressLiteralEvaluation)
                return;

            var text = LiteralValueInputFieldComponent.text;

            object result = null;

            var success = text.StringToValueOfType(Type, ref result);

            if(applyValue)
            {
                UndoLord.Instance.Push(new MutableFieldChange(this, MutableField, "Literal." + text));
            }

            if(success && applyValue)
                IndicateError = !MutableField.SetLiteralValueAsObject(result);
            else
                IndicateError = !success;
        }


        #endregion


        #region UI/State Management

        private void HandleValidationErrorState( bool valid )
        {
            //if ( MutableField.UseLiteralValue )
            //    return;
            IndicateError = !valid;
        }

        private void SwitchDisplayToLiteralValue()
        {
            LiteralValueDisplayPanel.SetActive( true );
            MutableValueDisplayPanel.SetActive( false );

            GetComponent<LayoutElement>().preferredHeight = 16;

            if(ShowItemDropDown)
                ShowItemDropDown = false;

            BoundsChanged();

            // when 'use literal value' is selected, the mutable
            //  field should use its literal even if the user doesn't
            //  then change the listed value.
            //HandleLiteralValueSelected();
        }

        private void SwitchDisplayToMutableValue()
        {
            LiteralValueDisplayPanel.SetActive( false );
            MutableValueDisplayPanel.SetActive( true );

            BoundsChanged();
        }

        #endregion


        #region Initial Value

        private void PopulateInitialValue()
        {
            if (MutableField.UseLiteralValue)
            {
                SwitchDisplayToLiteralValue();
                SetInitialLiteralValue();
            }
            else
            {
                SetInitialMutableValue();
                //if ( MutableField.UseGlobalParameter )
                //    SetInitialGlobalValue();
                //else
                //    SetInitialMutableValue();
            }
        }

        private void SetInitialLiteralValue()
        {
            IndicateLiteralValue();
        }

        private void SetInitialMutableValue()
        {
            var userFacingAbsKey = MutableField.AbsoluteKey;
            switch(MutableField.SchemaSource)
            {
                case SchemaSource.Mutable:
                    SetSelectedMutableText("Local Payload" + (userFacingAbsKey.Length == 0 ? "" : ".") + userFacingAbsKey);
                    break;
                case SchemaSource.Global:
                    SetSelectedMutableText("Global Payload" + (userFacingAbsKey.Length == 0 ? "" : ".") + userFacingAbsKey);
                    break;
            }

            var segmentCount = MutableField.AbsoluteKey.Count(c => c.Equals('.')) + 2;
            GetComponent<LayoutElement>().preferredHeight = segmentCount * 16;

            SchemaProvider.CacheSchema();
            bool fieldValid;
            try
            {
                fieldValid = MutableField.ValidateKey(SchemaProvider.Schema);
            }
            catch(NullReferenceException)
            {
                fieldValid = false;
            }
            SchemaProvider.UnCacheSchema();

            IndicateError = !fieldValid;
        }

        //private void SetInitialGlobalValue()
        //{
        //    var foundItem = MutableDropDownItems.FirstOrDefault(i => i.RelativeKey.Equals(MutableField.GlobalParameterKey) && i.IsGlobalParameter);
        //    if (foundItem == null)
        //    {
        //        SwitchDisplayToMutableValue();

        //        //Debug.LogWarningFormat( "Cannot set option to {0} because it's not a valid option. (MutableField labeled {1}).", MutableField.AbsoluteKey, LabelText );

        //        Indicate = true;

        //        return;
        //    }

        //    IndicateError = false;


        //    //IndicateGlobalValue(foundItem);
        //}

        private bool IndicateError
        {
            set
            {
                var backgroundColor = value ? new Color(1.0f, 0.2f, 0.2f, 1.0f) : Color.white;
                ErrorIndicatingImages.ForEach(i => i.color = backgroundColor);
            }
        }

        #endregion


        [UsedImplicitly]
        private void Start()
        {
            DestroyMutableDropDownItems();
            ItemsRootOverlay.gameObject.SetActive(false);
            m_showDropDown = false;
            DropDownButtonTextComponent.text = "▼";

            IndicateError = !MutableField.KeyValid;

            MutableField.KeyValidChanged += HandleValidationErrorState;
            SatelliteUpdateLord.mbbUpdate += CheckMouseInBounds;
        }

        [UsedImplicitly]
        private void OnDestroy()
        {
            MutableField.KeyValidChanged -= HandleValidationErrorState;
            SatelliteUpdateLord.mbbUpdate -= CheckMouseInBounds;
        }
        

        private void CheckMouseInBounds()
        {
            // NOTE: Likely only works for screen space overlay rendermode.

            if(!ShowItemDropDown)
                return;

            HoverSizeRect.GetWorldCorners(Corners);

            var mouseLoc = Vector3.zero;
            RectTransformUtility.ScreenPointToWorldPointInRectangle((RectTransform)transform, Input.mousePosition, ChainView.Instance.Camera, out mouseLoc);

            var mouseX = mouseLoc.x;
            var mouseY = mouseLoc.y;

            var inBounds = Corners.Any(c => c.x > mouseX) && Corners.Any(c => c.x < mouseX) && Corners.Any(c => c.y > mouseY) && Corners.Any(c => c.y < mouseY);
            if(!inBounds)
                ShowItemDropDown = false;
        }


        private void PopulateOptions()
        {
            DestroyMutableDropDownItems();

            // generate items from field's schema

            SchemaProvider.CacheSchema();

            EvaluateMutableValueForAdding("", SchemaSource.Mutable, SchemaProvider.Schema);
            GenerateMutableDropDownItems(MutableObject.IntersectOwnSchema(SchemaProvider.Schema), SchemaSource.Mutable, new Stack<string>());

            // generate items from cached schema
            if(CachedMutableDataStore.DataCached)
            {
                EvaluateMutableValueForAdding("", SchemaSource.Cached, CachedMutableDataStore.DataStore);
                GenerateMutableDropDownItems(MutableObject.IntersectOwnSchema(CachedMutableDataStore.DataStore), SchemaSource.Cached, new Stack<string>());
            }

            // generate items from the global variable store's schema

            AddMutableItem("", SchemaSource.Global, false, null);
            GenerateMutableDropDownItems(GlobalVariableDataStore.Instance.Schema, SchemaSource.Global, new Stack<string>());

            SchemaProvider.UnCacheSchema();
        }


        private void DestroyMutableDropDownItems()
        {
            foreach(var c in MutableDropDownItems)
            {
                Destroy(c.gameObject);
            }

            MutableDropDownItems.Clear();
        }

        private void GenerateMutableDropDownItems(MutableObject schemaSegment, SchemaSource source, Stack<string> ancestorTokens)
        {
            if(schemaSegment == null)
                return;

            foreach(var pair in schemaSegment)
            {
                var ancestorKeys = ancestorTokens.Any() ? ancestorTokens.Reverse().Aggregate((accumulator, current) => accumulator + "." + current) + "." : string.Empty;

                if(pair.Value is MutableObject)
                {
                    EvaluateMutableValueForAdding(ancestorKeys + pair.Key, source, pair.Value);

                    ancestorTokens.Push(pair.Key);

                    GenerateMutableDropDownItems(pair.Value as MutableObject, source, ancestorTokens);

                    ancestorTokens.Pop();
                }
                else if(pair.Value is ICollection<MutableObject>)
                {
                    EvaluateMutableValueForAdding(ancestorKeys + pair.Key + ArraySuffix, source, pair.Value);

                    var enumerable = (pair.Value as ICollection<MutableObject>).ToList();

                    if(enumerable.Any())
                    {
                        ancestorTokens.Push(pair.Key + ArraySuffix);

                        GenerateMutableDropDownItems(enumerable.First(), source, ancestorTokens);

                        ancestorTokens.Pop();
                    }
                }
                else if(pair.Value is TypeCollisionList)
                {
                    EvaluateMutableValueForAdding(ancestorKeys + pair.Key, source, pair.Value);

                    var colList = pair.Value as TypeCollisionList;

                    foreach(var obj in colList.Objects)
                    {
                        if(obj is MutableObject)
                        {
                            ancestorTokens.Push(pair.Key);

                            GenerateMutableDropDownItems(obj as MutableObject, source, ancestorTokens);

                            ancestorTokens.Pop();

                            continue;
                        }

                        if(obj is ICollection<MutableObject>)
                        {
                            var enumerable = obj as ICollection<MutableObject>;

                            if(!enumerable.Any()) continue;

                            ancestorTokens.Push(pair.Key + ArraySuffix);

                            GenerateMutableDropDownItems(enumerable.First(), source, ancestorTokens);

                            ancestorTokens.Pop();
                        }
                    }
                }
                else
                {
                    EvaluateMutableValueForAdding(ancestorKeys + pair.Key, source, pair.Value);
                }
            }
        }

        private void EvaluateMutableValueForAdding(string absoluteKey, SchemaSource source, object value)
        {
            if(value == null)
            {
                Debug.Log("Indicating error because the type of the mutable object is null.");
                IndicateError = true;
                return;
            }

            var isValidType = false;

            var collisionList = value as TypeCollisionList;
            if(collisionList == null)
                isValidType = Type.IsInstanceOfType(value);
            else
                isValidType = collisionList.ContainsType(Type);

            AddMutableItem(absoluteKey, source, isValidType, value);
        }

        private void AddMutableItem(string absoluteKey, SchemaSource schemaSource, bool isValid, object value)
        {
            var itemGo = Instantiate(MutableItemPrefab, ItemsRoot);
            itemGo.transform.localScale = Vector3.one;
            var item = itemGo.GetComponent<MutableBoxMutableItemBehaviour>();

            item.SchemaSource = schemaSource;

            item.AbsoluteKey = absoluteKey;

            if(value != null)
                item.Type = value.GetType();
            else
                item.Type = null;

            item.IsValidType = isValid;

            item.Selected += HandleMutableItemSelected;

            MutableDropDownItems.Add(item);
        }

        public void DoUndo(bool toLiteral, string value)
        {
            if(LiteralValueDisplayPanel.activeInHierarchy != toLiteral)
            {
                if(toLiteral) SwitchDisplayToLiteralValue();
                else SwitchDisplayToMutableValue();
            }

            if(toLiteral)
            {
                LiteralValueInputFieldComponent.text = value;

                object result = null;

                var success = value.StringToValueOfType(Type, ref result);

                if(success)
                    IndicateError = !MutableField.SetLiteralValueAsObject(result);
            }
            else //This was a Mutable
            {
                var segmentCount = value.Count(c => c.Equals('.')) + 1;
                GetComponent<LayoutElement>().preferredHeight = segmentCount * 16;

                SetSelectedMutableText(value);

                var schemaSource = value.StartsWith("Local") ? SchemaSource.Mutable : SchemaSource.Global;
                value = value.Substring(value.IndexOf(".", StringComparison.CurrentCulture) + 1);

                // ArraySuffix never leaves this class!
                var arrayFreeText = value.Replace(ArraySuffix, "");

                SchemaProvider.CacheSchema();
                MutableField.AbsoluteKey = arrayFreeText;

                MutableField.SchemaSource = schemaSource;

                bool fieldValid;

                try
                {
                    fieldValid = MutableField.ValidateKey(SchemaProvider.Schema);
                }
                catch(NullReferenceException)
                {
                    fieldValid = false;
                }
                SchemaProvider.UnCacheSchema();

                IndicateError = !fieldValid;

                SwitchDisplayToMutableValue();
            }
        }
    }
}
