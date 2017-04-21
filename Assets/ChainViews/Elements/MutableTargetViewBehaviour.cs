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
using Utility;
using Utility.Undo;
using Visualizers;

namespace ChainViews.Elements
{
    public class MutableTargetViewBehaviour : MonoBehaviour, IBoundsChanger
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
        private Text m_MutableLabelComponent = null;
        private Text MutableLabelComponent { get { return m_MutableLabelComponent; } }

        [SerializeField]
        private Text m_StringKeyLabelComponent = null;
        private Text StringKeyLabelComponent { get { return m_StringKeyLabelComponent; } set { m_StringKeyLabelComponent = value; } }

        [SerializeField]
        private Text m_SelectedItemText = null;
        private Text SelectedItemText { get { return m_SelectedItemText; } }

        [SerializeField]
        private Text m_DropDownButtonTextComponent = null;
        private Text DropDownButtonTextComponent { get { return m_DropDownButtonTextComponent; } }

        [SerializeField]
        private InputField m_NewFieldValueInputFieldComponent = null;
        private InputField NewFieldValueInputFieldComponent { get { return m_NewFieldValueInputFieldComponent; } }

        [SerializeField]
        private List<Image> m_ErrorIndicatingImages = null;
        private List<Image> ErrorIndicatingImages { get { return m_ErrorIndicatingImages; } }

        [Header("Prefab References")]

        [SerializeField]
        private GameObject m_MutableItemPrefab = null;
        private GameObject MutableItemPrefab { get { return m_MutableItemPrefab; } }

        [SerializeField]
        private int m_PreferredHeight = 38;
        private int PreferredHeight { get { return m_PreferredHeight; } set { m_PreferredHeight = value; } }

        #endregion


        private static string ArraySuffix { get { return "[]"; } }


        private string StringKeyLabelText
        {
            set
            {
                StringKeyLabelComponent.text = value;
            }
        }

        private string MutableLabelText
        {
            set
            {
                MutableLabelComponent.text = value;

                gameObject.name = string.Format("Mutable Target ({0})", value);
            }
        }


        private MutableTarget MutableTarget { get; set; }
        
        private Vector3[] m_Corners = new Vector3[4];
        private Vector3[] Corners { get { return m_Corners; } }

        private ISchemaProvider SchemaProvider { get; set; }

        public void Initialize( PropertyInfo propertyInfo, object propertyInfoResolutionObject, ISchemaProvider schemaProvider )
        {
            var foundMutableField = propertyInfo.GetValue( propertyInfoResolutionObject, null );
            var assignableMutableField = foundMutableField as MutableTarget;
            MutableTarget = assignableMutableField;


            SchemaProvider = schemaProvider;


            StringKeyLabelText = ControllableUtility.GetControllableLabel( propertyInfo ) + " Value Name";

            MutableLabelText = ControllableUtility.GetControllableLabel( propertyInfo ) + " Path";


            PopulateInitialValue();
        }

        private void PopulateInitialValue()
        {
            SetInitialMutableValue();
        }

        private void SetInitialMutableValue()
        {
            var itemGo = Instantiate( MutableItemPrefab );
            var item = itemGo.GetComponent<MutableBoxMutableItemBehaviour>();

            var initialKeyTokens = GetInitialDisplayString( null, MutableTarget.AbsoluteKey ).Reverse();

            item.SchemaSource = SchemaSource.Mutable;

            item.AbsoluteKey = string.Join( ".", initialKeyTokens.WithoutLast().ToArray() );

            item.IsValidType = true;

            MutableDropDownItems.Add( item );

            IndicateError = false;

            IndicateMutableValue( item );

            NewFieldValueInputFieldComponent.text = initialKeyTokens.Last();
        }


        //public ChainNode ChainNode { get; set; }

        #region Mutable Drop Down Items and Selection

        private readonly List<MutableBoxMutableItemBehaviour> m_MutableDropDownItems = new List<MutableBoxMutableItemBehaviour>();
        private List<MutableBoxMutableItemBehaviour> MutableDropDownItems
        {
            get { return m_MutableDropDownItems; }
        }

        private void HandleMutableItemSelected(MutableBoxMutableItemBehaviour item)
        {
            UndoLord.Instance.Push(new MutableTargetChange(this, (MutableTarget.SchemaSource == SchemaSource.Mutable ? "Local Payload." : "Global Payload.") + MutableTarget.AbsoluteKey, item.UserFacingAbsoluteKey + "." + MutableTarget.LastKey));

            IndicateMutableValue( item );


            // ArraySuffix never leaves this class!
            var arrayFreeText = item.AbsoluteKey.Replace( ArraySuffix, "" );

            if ( MutableTarget.ValidateKey( SchemaProvider.Schema ) )
            {
                MutableTarget.IntermediateKeyString = arrayFreeText;
            }

            IndicateError = !MutableTarget.KeyValid;

            ShowItemDropDown = false;
        }

        private void IndicateMutableValue( MutableBoxMutableItemBehaviour item )
        {
            SetSelectedMutableText(item.UserFacingAbsoluteKey);

            var segmentCount = item.UserFacingAbsoluteKey.Count(c => c.Equals('.')) + 1;
            GetComponent<LayoutElement>().preferredHeight = segmentCount * 16 + PreferredHeight - 16 + 4;    // -16 for keypanel, +4 for layout group padding.

            // ArraySuffix never leaves this class!
            var arrayFreeText = item.AbsoluteKey.Replace(ArraySuffix, "");

            SchemaProvider.CacheSchema();
            MutableTarget.IntermediateKeyString = arrayFreeText;
            SchemaProvider.UnCacheSchema();

            SchemaProvider.CacheSchema();
            MutableTarget.ValidateKey(SchemaProvider.Schema);
            SchemaProvider.UnCacheSchema();

            IndicateError = !MutableTarget.KeyValid;

            //ShowItemDropDown = false;
        }

        private void SetSelectedMutableText(string intermediateKey)
        {
            var tokens = intermediateKey.Split('.');
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

        #region New Field String

        [UsedImplicitly]
        public void HandleCheckStringKeyValue()
        {
            ChangeStringKeyValue( false );
        }

        [UsedImplicitly]
        public void HandleApplyStringKeyValue()
        {
            ChangeStringKeyValue(true);
        }

        [UsedImplicitly]
        private void ChangeStringKeyValue(bool applyValue)
        {
            var text = NewFieldValueInputFieldComponent.text;

            if(applyValue)
                UndoLord.Instance.Push(new MutableTargetChange(this,
                    (MutableTarget.SchemaSource == SchemaSource.Mutable ? "Local Payload." : "Global Payload.") + MutableTarget.AbsoluteKey,
                    (MutableTarget.SchemaSource == SchemaSource.Mutable ? "Local Payload." : "Global Payload.") + MutableTarget.IntermediateKeyString
                        + (string.IsNullOrEmpty(MutableTarget.IntermediateKeyString) ? "" : ".") + text));

            if (text.Contains('.'))
            {
                IndicateError = true;
                return;
            }

            if ( !applyValue )
                return;

            // Send on up
            SchemaProvider.CacheSchema();
            MutableTarget.LastKey = text;
            SchemaProvider.UnCacheSchema();
        }

        #endregion

        #region UI/State Management

        private void HandleValidationErrorState(bool valid)
        {
            IndicateError = !valid;
        }

        private bool IndicateError
        {
            set
            {
                //var textColor = value ? Color.red : Color.black;
                //LiteralValueInputFieldComponent.textComponent.color = textColor;
                //SelectedItemText.color = textColor;


                var backgroundColor = value ? Color.red : Color.white;
                ErrorIndicatingImages.ForEach(i => i.color = backgroundColor);

                //if ( value )
                //    Debug.Log( "Indicating Error" );
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

            MutableTarget.KeyValidChanged += HandleValidationErrorState;
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

            HoverSizeRect.GetWorldCorners(Corners);

            var mouseLoc = Vector3.zero;
            RectTransformUtility.ScreenPointToWorldPointInRectangle((RectTransform)transform, Input.mousePosition,
                ChainView.Instance.Camera, out mouseLoc);

            var mouseX = mouseLoc.x;
            var mouseY = mouseLoc.y;

            var inBounds = Corners.Any(c => c.x > mouseX) && Corners.Any(c => c.x < mouseX)
                           && Corners.Any(c => c.y > mouseY) && Corners.Any(c => c.y < mouseY);
            if (!inBounds)
                ShowItemDropDown = false;
        }


        private void PopulateOptions()
        {
            DestroyMutableDropDownItems();

            AddMutableItem("", true, null);

            GenerateMutableDropDownItems(MutableObject.IntersectOwnSchema( SchemaProvider.Schema ),
                new Stack<string>());
        }

        private void DestroyMutableDropDownItems()
        {
            foreach (var c in MutableDropDownItems)
            {
                Destroy(c.gameObject);
            }

            MutableDropDownItems.Clear();
        }

        private void GenerateMutableDropDownItems(MutableObject schema, Stack<string> ancestorTokens)
        {
            if (schema == null)
                return;

            foreach (var pair in schema)
            {
                var ancestorKeys = ancestorTokens.Any()
                    ? ancestorTokens.Reverse().Aggregate((accumulator, current) => accumulator + "." + current) +
                        "."
                    : string.Empty;

                if (pair.Value is MutableObject)
                {
                    EvaluateMutableValueForAdding(ancestorKeys + pair.Key, pair.Value);

                    ancestorTokens.Push(pair.Key);

                    GenerateMutableDropDownItems(pair.Value as MutableObject, ancestorTokens);

                    ancestorTokens.Pop();
                }
                else if (pair.Value is ICollection<MutableObject>)
                {
                    EvaluateMutableValueForAdding(ancestorKeys + pair.Key + ArraySuffix, pair.Value);

                    var enumerable = pair.Value as ICollection<MutableObject>;

                    if (enumerable.Any())
                    {
                        ancestorTokens.Push(pair.Key + ArraySuffix);

                        GenerateMutableDropDownItems(enumerable.First(), ancestorTokens);

                        ancestorTokens.Pop();
                    }
                }
                else
                {
                    EvaluateMutableValueForAdding(ancestorKeys + pair.Key, pair.Value);
                }
            }
        }



        private void EvaluateMutableValueForAdding(string absoluteKey, object value)
        {
            if (value == null)
            {
                Debug.Log("Indicating error because the type of the mutable object is null.");
                IndicateError = true;
                return;
            }

            var isValidType = value is MutableObject || value is IEnumerable<MutableObject>
                || (value is TypeCollisionList && (
                (value as TypeCollisionList).ContainsType( typeof(MutableObject) )
                || (value as TypeCollisionList).ContainsType( typeof(IEnumerable<MutableObject> ))));

            AddMutableItem(absoluteKey, isValidType, value);
        }

        private void AddMutableItem(string absoluteKey, bool isValid, object value)
        {
            var itemGo = Instantiate(MutableItemPrefab);
            var item = itemGo.GetComponent<MutableBoxMutableItemBehaviour>();

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

        private Stack< string > GetInitialDisplayString( MutableObject schema, string absoluteKey )
        {
            // Null schema implies we're in initialize behaviour

            var displayKeys = new Stack<string>( MutableTarget.AbsoluteKey.Split( '.' ) );
            
            if ( schema == null || !MutableTarget.ValidateKey( schema ) )
            {
                if ( schema != null )
                    IndicateError = true;

                return displayKeys;
            }


            displayKeys = GetDisplayString( schema, absoluteKey.Split( '.' ).ToList() );

            return displayKeys;
        }

        public Stack<string> GetDisplayString( MutableObject mutable, List<string> keys )
        {
            var displayStack = new Stack<string>();
            GetDisplayString( displayStack, mutable, 0, keys );
            return displayStack;
        }

        private void GetDisplayString( Stack<string> displayStack, MutableObject mutable, int depth, List<string> keys )
        {
            if ( depth > keys.Count )
                return;

            var nextLevel = mutable[keys[depth]];

            MutableObject resolutionTarget;

            if ( nextLevel is IEnumerable<MutableObject> )
            {
                displayStack.Push( keys[depth] + ArraySuffix );
                resolutionTarget = ( nextLevel as IEnumerable<MutableObject> ).First();
            }
            else
            {
                displayStack.Push( keys[depth] );
                resolutionTarget = nextLevel as MutableObject;
            }

            if ( resolutionTarget == null )
                return;

            GetDisplayString( displayStack, resolutionTarget, depth + 1, keys );
        }

        public void DoUndo(string newKey)
        {
            var parts = newKey.Split('.');

            SetSelectedMutableText(string.Join(".", parts, 0, parts.Length - 1));

            var segmentCount = parts.Length - 1;
            GetComponent<LayoutElement>().preferredHeight = segmentCount * 16 + PreferredHeight - 16 + 4;    // -16 for keypanel, +4 for layout group padding.

            // ArraySuffix never leaves this class!
            var arrayFreeText = string.Join(".", parts, 1, parts.Length - 2).Replace(ArraySuffix, "");

            SchemaProvider.CacheSchema();
            MutableTarget.IntermediateKeyString = arrayFreeText;
            SchemaProvider.UnCacheSchema();

            var textKey = parts[parts.Length - 1];
            NewFieldValueInputFieldComponent.text = textKey;

            var indicError = false;

            if(textKey.Contains('.'))
            {
                indicError = true;
            }
            else
            {
                SchemaProvider.CacheSchema();
                MutableTarget.LastKey = textKey;
                SchemaProvider.UnCacheSchema();
            }

            SchemaProvider.CacheSchema();
            MutableTarget.ValidateKey(SchemaProvider.Schema);
            SchemaProvider.UnCacheSchema();

            IndicateError = indicError | !MutableTarget.KeyValid;
        }
    }
}
