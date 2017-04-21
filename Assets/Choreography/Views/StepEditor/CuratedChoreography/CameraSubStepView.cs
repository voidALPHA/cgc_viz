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
using Choreography.CameraControl;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Choreography.Views.StepEditor.CuratedChoreography
{
    public class CameraSubStepView : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        [SerializeField]
        private int m_FrustumLayerCount = 7;
        private int FrustumLayerCount { get { return m_FrustumLayerCount; } }

        [SerializeField]
        private float m_FrustumLayerLength = 5f;
        private float FrustumLayerLength { get { return m_FrustumLayerLength; } }

        private Text m_NumberLabel;

        private Text NumberLabel
        {
            get
            {
                if (!m_NumberLabel) m_NumberLabel = transform.Find("Panel/Button Panel/SubStepNumber").GetComponent<Text>();
                return m_NumberLabel;
            }
        }

        private CameraSubStep m_SubStep;
        public CameraSubStep SubStep
        {
            get { return m_SubStep; }
            set
            {
                m_SubStep = value;
                transform.Find("Panel/Timing Panel/DelayInput").GetComponent<InputField>().text = value.Delay.ToString("F");
                transform.Find("Panel/Timing Panel/DurationInput").GetComponent<InputField>().text = value.Duration.ToString("F");
            }
        }

        private bool ShowFrustum { get; set; }
        private static Material FrustumMaterial { get; set; }
        private static Transform SubstepReferenceTransform { get; set; }


        private static Vector3[] m_FrustumPoints = null;

        private static Vector3[] FrustumPoints
        {
            get
            {
                if (m_FrustumPoints == null)
                {
                    // Per https://forum.unity3d.com/threads/drawfrustum-is-drawing-incorrectly.208081/#post-1402563
                    Vector3[] nearCorners = new Vector3[4], farCorners = new Vector3[4];
                    var camPlanes =
                        GeometryUtility.CalculateFrustumPlanes(SplineCameraControlLord.MainCamera.projectionMatrix);

                    Plane temp = camPlanes[1];
                    camPlanes[1] = camPlanes[2];
                    camPlanes[2] = temp; //swap [1] and [2] so the order is better for the loop

                    for (int i = 0; i < 4; i++)
                    {
                        nearCorners[i] = Plane3Intersect(camPlanes[4], camPlanes[i], camPlanes[(i + 1) % 4]);
                        //near corners on the created projection matrix
                        farCorners[i] = Plane3Intersect(camPlanes[5], camPlanes[i], camPlanes[(i + 1) % 4]);
                        //far corners on the created projection matrix
                    }

                    m_FrustumPoints = new Vector3[]
                    {
                        nearCorners[ 0 ], nearCorners[ 1 ], nearCorners[ 2 ], nearCorners[ 3 ], farCorners[ 0 ],
                        farCorners[ 1 ], farCorners[ 2 ], farCorners[ 3 ]
                    };
                }

                return m_FrustumPoints;
            }
        }

        public CameraSubStepView()
        {
            ShowFrustum = false;
        }

        public event Action<CameraSubStep> RemoveButtonClicked = delegate { };

        public void HandleFromCameraClicked()
        {
            SaveCameraData();
        }

        public void SaveCameraData()
        {
            SubStep.Facing = SplineCameraControlLord.MainCamera.transform.forward;
            SubStep.Position = SplineCameraControlLord.CameraParent.transform.position;

            if(SubstepReferenceTransform==null)
            {
                SubstepReferenceTransform = GameObject.Find("SubstepReferencePoint").transform;
            }
            SubstepReferenceTransform.position = SubStep.Position;
            SubstepReferenceTransform.forward = SubStep.Facing * -1f;
        }

        public void HandleToCameraClicked()
        {
            SplineCameraControlLord.CameraParent.transform.position = SubStep.Position;
            SplineCameraControlLord.MainCamera.transform.LookAt(SubStep.Facing + SubStep.Position);
        }

        public void HandleDurationValueChanged(string newDuration)
        {
            var floatValue = float.Parse(newDuration);

            SubStep.Duration = Mathf.Max(floatValue, 0.001f);
        }

        public void HandleDelayValueChanged(string newDelay)
        {
            var floatValue = float.Parse(newDelay);

            SubStep.Delay = Mathf.Max(floatValue, 0.001f);
        }

        public void HandleRemoveClicked()
        {
            RemoveButtonClicked(SubStep);
        }

        public void RefreshNumberLabel()
        {
            NumberLabel.text = transform.GetSiblingIndex().ToString("D3");
        }

        public void SetNumberLabel(int newNumber)
        {
            NumberLabel.text = newNumber.ToString("D3");
        }

        public int GetNumberLabelValue()
        {
            return int.Parse( NumberLabel.text );
        }

        

        public void OnRenderObject()
        {
            if(FrustumMaterial==null)
            {
                // Copied from https://docs.unity3d.com/ScriptReference/GL.html

                Shader shader = Shader.Find("Hidden/Internal-Colored");
                FrustumMaterial = new Material(shader) { hideFlags = HideFlags.HideAndDontSave };

                FrustumMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                FrustumMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                FrustumMaterial.SetInt("_Cull", (int)UnityEngine.Rendering.CullMode.Off);
                FrustumMaterial.SetInt("_ZWrite", 0);
            }

            if(ShowFrustum)
            {
                // Use our material
                FrustumMaterial.SetPass(0);
                // Save the "current" matrix to return to
                GL.PushMatrix();
                // Set the origin to where the substep is
                GL.MultMatrix(SubstepReferenceTransform.localToWorldMatrix);
                // Begin drawing primitive lines
                GL.Begin(GL.LINES);

                // Draw frustum lines

                // Draw intermediate lines to handle human perspective issues
                var sides = new Vector3[] { (FrustumPoints[4] - FrustumPoints[0]).normalized, (FrustumPoints[5] - FrustumPoints[1]).normalized, (FrustumPoints[6] - FrustumPoints[2]).normalized, (FrustumPoints[7] - FrustumPoints[3]).normalized };

                for(int i = FrustumLayerCount; i > -1; i--) // FrustumLayerCount intermediate rectangles
                {
                    GL.Color(new Color(0, (FrustumLayerCount - i) / (float)FrustumLayerCount, 0));

                    for(int j = 0; j < 4; j++)
                    {
                        // Across
                        GL.Vertex(FrustumPoints[j] + (sides[j] * i * FrustumLayerLength));
                        GL.Vertex(FrustumPoints[(j + 1) % 4] + (sides[(j + 1) % 4] * i * FrustumLayerLength));

                        if(i > 0)
                        {
                            // Forward
                            GL.Vertex(FrustumPoints[j] + (sides[j] * (i - 1) * FrustumLayerLength));
                            GL.Vertex(FrustumPoints[j] + (sides[j] * (i) * FrustumLayerLength));
                        }
                        
                    }
                }

                // Done drawing primitives
                GL.End();
                // Return to the "proper" matrix
                GL.PopMatrix();
            }
        }

        private static Vector3 Plane3Intersect(Plane p1, Plane p2, Plane p3)
        {
            return ((-p1.distance * Vector3.Cross(p2.normal, p3.normal)) +
                    (-p2.distance * Vector3.Cross(p3.normal, p1.normal)) +
                    (-p3.distance * Vector3.Cross(p1.normal, p2.normal))) /
                    (Vector3.Dot(p1.normal, Vector3.Cross(p2.normal, p3.normal)));
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (SubstepReferenceTransform==null)
            {
                SubstepReferenceTransform = GameObject.Find("SubstepReferencePoint").transform;
            }

            ShowFrustum = true;
            SubstepReferenceTransform.position = SubStep.Position;
            SubstepReferenceTransform.forward = SubStep.Facing * -1f;
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            ShowFrustum = false;
        }
    }
}
