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
using UnityEngine;

namespace Visualizers.AnnotationIcon
{
    [Serializable]
    public class AnnotationDescriptionPair
    {
        [SerializeField]
        private string m_Type;
        public string Type { get { return m_Type; } set { m_Type = value; } }

        [SerializeField]
        private AnnotationIcon m_AnnotationIcon;
        public AnnotationIcon AnnotationIcon { get { return m_AnnotationIcon; } set { m_AnnotationIcon = value; } }
    }

    public class AnnotationMappingLord : MonoBehaviour
    {
        private static AnnotationMappingLord AnnotationsMaster { get; set; }

        [SerializeField]
        private List<AnnotationDescriptionPair> m_AnnotationTypes;
        private List<AnnotationDescriptionPair> AnnotationTypes { get { return m_AnnotationTypes; } set { m_AnnotationTypes = value; } }

        private Dictionary<String, AnnotationIcon> AnnotationIcons { get; set; }

        public Dictionary<String, List<GameObject>> IconInstances { get; set; }
        public Dictionary<String, bool> IconTypesActive { get; set; }

        public void Awake()
        {
            if (AnnotationsMaster != null)
                return;

            AnnotationsMaster = this;

            IconInstances = new Dictionary<string, List<GameObject>>();

            AnnotationIcons = new Dictionary<string, AnnotationIcon>();

            IconTypesActive = new Dictionary<string, bool>();

            foreach (var annotationType in AnnotationTypes)
            {
                AnnotationIcons.Add(annotationType.Type, annotationType.AnnotationIcon);
                IconInstances.Add(annotationType.Type, new List<GameObject>());
                IconTypesActive.Add(annotationType.Type, true);
            }
        }

        public static AnnotationIcon GenerateAnnotationIcon(String annotationType)
        {
            if (!AnnotationsMaster.AnnotationIcons.ContainsKey(annotationType))
            {
                //throw new Exception("This type ("+annotationType+") doesn't have a corresponding icon!");
                var defaultIcon = Instantiate(AnnotationsMaster.AnnotationIcons["default"].gameObject);
                return defaultIcon.GetComponent<AnnotationIcon>();
            }

            var newIcon = Instantiate(AnnotationsMaster.AnnotationIcons[annotationType].gameObject);

            AnnotationsMaster.IconInstances[annotationType].Add(newIcon);

            newIcon.SetActive(AnnotationsMaster.IconTypesActive[annotationType]);

            newIcon.GetComponent<AnnotationIcon>().AnnotationType = annotationType;

            return newIcon.GetComponent<AnnotationIcon>();
        }

        public static void RemoveAnnotationIcon(AnnotationIcon annotation)
        {
            if (annotation.AnnotationType == null ||
                !AnnotationsMaster.IconTypesActive.ContainsKey(annotation.AnnotationType))
            {
                AnnotationsMaster.IconInstances["default"].Remove(annotation.gameObject);
                return;
            }

            AnnotationsMaster.IconInstances[annotation.AnnotationType].Remove(annotation.gameObject);
        }

        public static void ToggleIconVisibility(string annotationType, bool state)
        {
            if (annotationType == null ||
                !AnnotationsMaster.IconTypesActive.ContainsKey(annotationType))
                return;

            AnnotationsMaster.IconTypesActive[annotationType] = state;

            foreach (var icon in AnnotationsMaster.IconInstances[annotationType])
            {
                icon.gameObject.SetActive(state);
            }
        }

        public static void ToggleStartEndVisibility(bool state)
        {
            var keyNames = AnnotationsMaster.IconTypesActive.Keys.ToList();

            foreach (var annotationName in keyNames)
            {
                if (annotationName.StartsWith("start") || annotationName.StartsWith("end"))
                {
                    ToggleIconVisibility(annotationName, state);
                }
            }
        }
    }
}
