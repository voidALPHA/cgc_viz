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
using System.Collections;
using System.Collections.Generic;
using System.Windows.Forms;
using Assets.Utility;
using Chains;
using ChainViews;
using Choreography.Views;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Utility;
using Screen = UnityEngine.Screen;

public class MutableBoxTypeIndicatorBehaviour : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField]
    private Text m_TypeIdentifierTextComponent = null;
    private Text TypeIndicatorTextComponent { get { return m_TypeIdentifierTextComponent; } }

    [SerializeField]
    private RectTransform m_TooltipPanelRoot = null;
    private RectTransform TooltipPanelRoot { get { return m_TooltipPanelRoot; } }

    [SerializeField]
    private Text m_TooltipPanelText = null;
    private Text TooltipPanelText { get { return m_TooltipPanelText; } }


    private Type m_Type;
    public Type Type
    {
        get { return m_Type; }
        set
        {
            m_Type = value;

            if ( m_Type == null )
            {
                TypeIndicatorTextComponent.text = "";
                TooltipPanelText.text = "Not specified";
                return;
            }

            if ( m_Type == typeof( Single ) )
                TypeIndicatorTextComponent.text = "F";  // Disambiguate from 'S' for String, and use more common language (float instead of single)
            else
                TypeIndicatorTextComponent.text = m_Type.Name.Substring(0, 1);

            //UpdateTooltipPanelText();
        }
    }

    private void UpdateTooltipPanelText()
    {
        TooltipPanelText.text = Type.GetGenericName();
        var lineCount = 1;

        if ( ValidValuesCallback != null )
        {
            TooltipPanelText.text += "\n<b>------------</b>";
            lineCount++;
            
            foreach ( var valueName in ValidValuesCallback() )
            {
                TooltipPanelText.text += "\n" + valueName;
                lineCount++;
            }
        }
        else if ( Type.IsEnum )
        {
            TooltipPanelText.text += "\n<b>------------</b>";
            lineCount++;

            foreach ( var valueName in Enum.GetNames( Type ) )
            {
                TooltipPanelText.text += "\n" + valueName;
                lineCount++;
            }
        }

        // Various height/2 operations are because this is center-aligned...
        //var posY = TooltipPanelRoot.position.y;
        var posY = transform.position.y;
        var height = lineCount * ( TooltipPanelText.fontSize + 1.0f ) + 12;

        var viewportHeight = Screen.height - TimelineViewBehaviour.Instance.CurrentHeight;

        if ( height > (viewportHeight * 0.66f) )
        {
            //Debug.Log( "1" );
            //height = Mathf.FloorToInt( viewportHeight );
            //posY = (Screen.height+TimelineViewBehaviour.Instance.CurrentHeight)/2.0f;
            height = Mathf.FloorToInt(viewportHeight * 0.66f);
        }
        //else if ( ( posY + height / 2 ) > Screen.height )
        //{
        //    //Debug.Log( "2" );
        //    posY -= ( posY + height / 2 ) - Screen.height;
        //}
        //else if ( ( posY - height / 2 ) < TimelineViewBehaviour.Instance.CurrentHeight )
        //{
        //    //Debug.Log( "3" );
        //    posY += TimelineViewBehaviour.Instance.CurrentHeight - ( posY - height / 2 );
        //}

        //Debug.Log( " PosY is " + posY + " and transform y is " + transform.position.y );
        if ( !Mathf.Approximately( posY, transform.position.y ) )
        {
            //Debug.Log( "has moved..." );
            TooltipPanelRoot.GetComponentInChildren< ScrollRect >().verticalNormalizedPosition = 1.0f;
        }
        else
        {
            //TooltipPanelRoot.GetComponentInChildren<ScrollRect>().verticalNormalizedPosition = 0.5f;
        }


        TooltipPanelRoot.position = new Vector2( transform.position.x + 8, posY );
        TooltipPanelRoot.sizeDelta = new Vector2( TooltipPanelRoot.sizeDelta.x, height );
    }

    public Func<List<string>> ValidValuesCallback { get; set; }

    [UsedImplicitly]
    private void Start()
    {
        HideTooltip();
    }

    private Coroutine RunningShowCoroutine { get; set; } 

    public void OnPointerEnter( PointerEventData eventData )
    {
        if ( RunningShowCoroutine != null )
            return;

        RunningShowCoroutine = StartCoroutine(ShowCoroutine());
    }

    private IEnumerator ShowCoroutine()
    {
        yield return new WaitForSeconds( 0.3f );

        ShowTooltip();

        if(TooltipPanelRoot.sizeDelta.y < (Screen.height - TimelineViewBehaviour.Instance.CurrentHeight) * 0.6f)
        {
            RunningShowCoroutine = null;
        }
        else
        {
            ChainView.Instance.AllowMouse = false;
            var pos = TooltipPanelText.transform.parent.localPosition;
            pos.y = TooltipPanelRoot.sizeDelta.y * 0.5f;

            while(true)
            {
                yield return new WaitForEndOfFrame();
                pos.y = Mathf.Clamp(pos.y - (Input.GetAxis("Mouse ScrollWheel") * 128f),
                    TooltipPanelRoot.sizeDelta.y * 0.5f,
                    ((RectTransform)TooltipPanelText.transform.parent).sizeDelta.y - (TooltipPanelRoot.sizeDelta.y * 0.5f));
                TooltipPanelText.transform.parent.localPosition = pos;
            }
        }

        
    }

    public void OnPointerExit( PointerEventData eventData )
    {
        if ( RunningShowCoroutine != null )
        {
            StopCoroutine( RunningShowCoroutine );
            RunningShowCoroutine = null;
            ChainView.Instance.AllowMouse = true;
        }

        HideTooltip();
    }


    private void ShowTooltip()
    {
        UpdateTooltipPanelText();

        TooltipPanelRoot.transform.SetParent(ChainView.TooltipCanvas, true);
        TooltipPanelRoot.gameObject.SetActive( true );
    }

    private void HideTooltip()
    {
        TooltipPanelRoot.gameObject.SetActive( false );
        TooltipPanelRoot.transform.SetParent(transform, true);
    }

}
