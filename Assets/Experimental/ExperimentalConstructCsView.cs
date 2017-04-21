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


using UnityEngine;
using Utility;
using Visualizers.CsView;
using Visualizers.CsView.Texturing;

namespace Assets.Experimental
{
    [ExecuteInEditMode]
    public class ExperimentalConstructCsView : MonoBehaviour
    {
        [SerializeField]
        private bool m_Exec = false;
        private bool Exec { get { return m_Exec; } set { m_Exec = value; } }

        [SerializeField]
        private int m_FileSize = 70000;
        private int FileSize { get { return m_FileSize; } }

        [SerializeField]
        private string m_InputString = "Um";
        private string InputString { get { return m_InputString; } }

        [SerializeField]
        private float m_SizeOffset = .2f;
        private float SizeOffset { get { return m_SizeOffset; } }

        [SerializeField]
        private Material m_TestMaterial;
        private Material TestMaterial { get { return m_TestMaterial; } }

        [SerializeField]
        private int m_TextureSize = 1024;
        private int TextureSize { get { return m_TextureSize; } }

        [SerializeField]
        private int m_LowerOctave = 4;
        private int LowerOctave { get { return m_LowerOctave; } }

        [SerializeField]
        private int m_UpperOctave = 8;
        private int UpperOctave { get { return m_UpperOctave; } }

        [SerializeField]
        private float m_Persistence = .8f;
        private float Persistence { get { return m_Persistence; } }
        
        [SerializeField]
        private ComponentAssembler m_Assembler;
        private ComponentAssembler Assembler { get { return m_Assembler; } }

        [SerializeField]
        private float m_SpectrumMultiplier = 1f;
        private float SpectrumMultiplier { get { return m_SpectrumMultiplier; } }

        [SerializeField]
        private float m_OffsetMult = 3f;
        private float OffsetMult { get { return m_OffsetMult; } set { m_OffsetMult = value; } }

        [SerializeField]
        private OpcodeContainer m_Opcodes;
        private OpcodeContainer Opcodes { get { return m_Opcodes; } }

        [SerializeField]
        private Color m_PrimaryColor = Color.green;
        private Color PrimaryColor { get { return m_PrimaryColor; } }

        [SerializeField]
        private int m_NumberOfBands = 4;
        private int NumberOfBands { get { return m_NumberOfBands; } }

        [SerializeField]
        private float m_BandWidth = .02f;
        private float BandWidth { get { return m_BandWidth; } }

        [SerializeField]
        private float m_BandEdgeWidth = .01f;
        private float BandEdgeWidth { get { return m_BandEdgeWidth; } }

        [SerializeField]
        private float m_PrimaryImpactOnBands = .4f;
        private float PrimaryImpactOnBands { get { return m_PrimaryImpactOnBands; } }

        [SerializeField]
        private bool m_UseDebugGrayscaleGradient = false;
        private bool UseDebugGrayscaleGradient { get { return m_UseDebugGrayscaleGradient; } }

        [SerializeField]
        private bool m_UseDebugColorGradient = false;
        private bool UseDebugColorGradient { get { return m_UseDebugColorGradient; } }

        public void Update()
        {
            if (!Exec || Assembler == null)
                return;
            Exec = false;

            var significantStream = InternalArrayBitstream.GenerateBitStreamFromLetterNumbers(InputString);
            significantStream.AdvanceByBytes = true;

            var insignificantStream = InternalArrayBitstream.GenerateBitStreamFromLetterNumbers(InputString);
            insignificantStream.AdvanceByBytes = false;


            var csView = Assembler.ConstructCsViewStep1(significantStream, insignificantStream, FileSize, SizeOffset);


            csView.transform.parent = transform;
            OffsetMult += 1f;
            csView.transform.localPosition = Vector3.right * OffsetMult;
            csView.name = InputString + " view parent";

            var localMaterial = GameObject.Instantiate(TestMaterial);
            localMaterial.name = "Instance Material";


            ColorGradient localGradient;

            if (!UseDebugColorGradient)
            {
                //localGradient = OpcodeToTextureBanding.GenerateDebugGradient(
                //    Opcodes.Opcodes, PrimaryColor, NumberOfBands);
                localGradient = OpcodeToTextureBanding.GenerateTextureBanding(
                    Opcodes.Opcodes, PrimaryColor, PrimaryImpactOnBands, NumberOfBands,
                    BandWidth, 1f, 1f, BandEdgeWidth);
            }
            else
            {
                localGradient = new ColorGradient(2);
                localGradient.ColorKeys.Add(0, new GradientColorKey(Color.black, 0));
                localGradient.ColorKeys.Add(1, new GradientColorKey(Color.white, 1));
            }

            var texture = UseDebugGrayscaleGradient
                ? ConstructNoiseTexture.GenerateGradientTexture( TextureSize,
                    localGradient )
                : ConstructNoiseTexture.GenerateNoiseTexture( TextureSize,
                    LowerOctave, UpperOctave, Persistence, localGradient, 64,
                    SpectrumMultiplier );

            localMaterial.SetTexture("_MainTex",texture);
            localMaterial.SetTexture("_EmissionMap", texture);

            foreach (var component in csView.Components)
                foreach (var rend in component.Geometry.GetComponents<Renderer>())
                    rend.material = localMaterial;
        }
    }
}
