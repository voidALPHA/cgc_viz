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
using Assets.Utility;
using UnityEngine;
using Utility;

namespace Visualizers.CsView
{
    public class ComponentAssembler : MonoBehaviour
    {
        [SerializeField]
        private List<CsViewComponent> m_ViewComponents;
        private List<CsViewComponent> ViewComponents { get { return m_ViewComponents; } }

        [SerializeField]
        private GameObject m_PillarPrefab;
        private GameObject PillarPrefab { get { return m_PillarPrefab; } }

        
        public ChallengeSetVisualizer ConstructCsViewStep1(
            ICsBitStream significant,
            ICsBitStream insignificant,
            int fileSize,
            float sizeOffset)
        {
            var newCsView = VisualizerFactory.InstantiateChallengeSetVisualizer();

            newCsView.DetailMaterial = GameObject.Instantiate(newCsView.DetailMaterial);

            var nComponents = ComputeNumberOfComponents(significant, insignificant, fileSize);

            var outgoingComponentStack = new SortedList<float, Transform>(new DuplicateKeyComparer<float>());

            outgoingComponentStack.Add(1, newCsView.transform);

            float componentImpactMult = 1f;

            newCsView.Components = new List<CsViewComponent>();

            int i = 0;
            do
            {
                var parent = outgoingComponentStack.Values.Last();
                outgoingComponentStack.RemoveAt(outgoingComponentStack.Count - 1);

                // construct component
                var newComponent = GenerateComponent(significant, insignificant, parent, sizeOffset,
                    i > 0);

                // assign material
                foreach (var rend in newComponent.GetComponentsInChildren<Renderer>())
                    rend.material = newCsView.DetailMaterial;

                // iterate through other potential components using the stack
                GenerateOutgoingPriorities(significant, insignificant, newComponent,
                    ref outgoingComponentStack, componentImpactMult);

                componentImpactMult *= .8f;

                newCsView.Components.Add(newComponent);

                newCsView.EncapsulatePoint( newComponent.transform.position );//+Vector3.one/2f);
                //newCsView.ComponentBounds.Encapsulate( newComponent.transform.position );
                //new UnityEngine.Bounds(newComponent.transform.position, Vector3.one) );

                i++;

            } while (i < nComponents);

            newCsView.Components.First().transform.position -= ( newCsView.ComponentBoundMax + newCsView.ComponentBoundMin ) / 2f;

            //newCsView.Components.First().transform.localPosition -= .5f*(newCsView.ComponentBoundMax - newCsView.ComponentBoundMin);

            return newCsView;
        }

        private int ComputeNumberOfComponents(ICsBitStream significant, ICsBitStream insignificant, int filesize)
        {
            return Mathf.Max(Mathf.FloorToInt(Mathf.Log(filesize - 6000, 1.7f)) - 10, 3);
        }

        private CsViewComponent GenerateComponent(ICsBitStream significant, ICsBitStream insignificant, Transform parent, float sizeOffset,
            bool generatePillar = true)
        {
            int componentTypeIndex = significant.ReadInt(8)
                % ViewComponents.Count;

            if (ViewComponents.Count < componentTypeIndex)
                throw new Exception("Index " + componentTypeIndex + " is not covered by a cs view component!");

            var newComponent = GameObject.Instantiate(ViewComponents[componentTypeIndex].gameObject);

            newComponent.transform.parent = parent;
            newComponent.transform.localScale = Vector3.one;
            newComponent.transform.localRotation = Quaternion.identity;
            newComponent.transform.localPosition = Vector3.zero + Vector3.up * sizeOffset;

            var csComponent = newComponent.GetComponent<CsViewComponent>();

            // rotate component
            var rotationMult = insignificant.ReadInt( 3 )
                * 360f / 8f;

            newComponent.transform.Rotate(Vector3.up, rotationMult, Space.Self);

            if (generatePillar)
            {
                // construct pillar
                var pillar = GameObject.Instantiate(PillarPrefab);
                pillar.transform.parent = parent;
                pillar.transform.localScale = new Vector3(1, sizeOffset * 5f, 1f);
                pillar.transform.localRotation = Quaternion.identity;
                pillar.transform.localPosition = Vector3.up * sizeOffset / 2f;
            }

            return csComponent;
        }

        private void GenerateOutgoingPriorities(ICsBitStream significant, ICsBitStream insignificant,
            CsViewComponent component, ref SortedList<float, Transform> potentialSites,
            float componentImpactMultiplier = 1f)
        {
            var criticalData = significant.ReadBits(16).ToList();

            var bitSums = new int[3];

            bitSums[0] = SumNthBits(criticalData, 2, 1);
            bitSums[1] = SumNthBits(criticalData, 2, 0);
            bitSums[2] = SumNthBits(criticalData, 3, 1);

            var cosums = new float[3];

            cosums[0] = (bitSums[0] * .76f + 2.1f) * componentImpactMultiplier;
            cosums[1] = (bitSums[1] * .35f + .1f) * componentImpactMultiplier;
            cosums[2] = (bitSums[2] * .24f + .1f) * componentImpactMultiplier;

            for (int i = 0; i < 3; i++)
                potentialSites.Add(cosums[i], component.ConnectionBones[i]);
        }

        private int SumNthBits(List<bool> bits, int nCount, int nOffset = 0)
        {
            int total = 0;
            int i = 0;
            foreach (var bit in bits)
                total += bit && (i + nOffset) % nCount == 0 ? 1 : 0;

            return total;
        }
    }
}
