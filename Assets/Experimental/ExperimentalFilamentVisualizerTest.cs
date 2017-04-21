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

namespace Experimental
{
    public class ExperimentalFilamentVisualizerTest : MonoBehaviour
    {
        [SerializeField]
        private int m_NumberOfRandomPoints = 15;
        private int NumberOfRandomPoints { get { return m_NumberOfRandomPoints; } set { m_NumberOfRandomPoints = value; } }

        [SerializeField]
        private string m_CommandUrl = "http://localhost:8000";
        private string CommandUrl { get { return m_CommandUrl; } set { m_CommandUrl = value; } }

        [SerializeField]
        private Vector3 m_BoundScale = Vector3.one;
        private Vector3 BoundScale { get { return m_BoundScale; } set { m_BoundScale = value; } }

        private void Start()
        {
            /*
            
            List<Vector3> points = new List<Vector3>();
            for (int i=0; i<NumberOfRandomPoints; i++)
                points.Add((UnityEngine.Random.insideUnitSphere+Vector3.one)*50f);


            List<MutableObject> schemaList = new List<MutableObject>();

            MutableObject testSchema = new MutableObject();
            testSchema.Add("Entries", schemaList);

            MutableObject schemaEntry = new MutableObject();
            schemaEntry.Add("x position", 0f);
            schemaEntry.Add("y position", 0f);
            schemaEntry.Add("z position", 0f);

            schemaList.Add(schemaEntry);

            MutableObject mutable = new MutableObject();
            System.Random newRandom = new System.Random(1337);
            var pointsData = points.Select(point => new MutableObject()
            {
                new KeyValuePair<string, object>("x position", point.x), 
                new KeyValuePair<string, object>("y position", point.y), 
                new KeyValuePair<string, object>("z position", point.z),
                new KeyValuePair<string, object>("color", ColorUtility.HsvtoRgb((float)newRandom.NextDouble(), .8f, .6f ))
            }).ToList();

            mutable.Add("Entries", pointsData);

            var payload = new VisualPayload(mutable,
                new VisualDescription(BoundingBox.ConstructBoundingBox( GetType().Name )));

            

            // generate chain
            CommandProcessor.Url = CommandUrl;
            ExecutionAdapter executionAdapter = new ExecutionAdapter();

            ExecutionToTraceMutator executionMutator = new ExecutionToTraceMutator();
            executionAdapter.DefaultState.AddTarget(executionMutator);

            //MutableToDebugMutator debugMutator = new MutableToDebugMutator();
            //debugMutator.MaxElements = 100;
            //executionMutator.TraceState.AddTarget(debugMutator);

           // ScatterPlotController scatterController = new ScatterPlotController();
           // scatterController.XAxis.AbsoluteKey = "Entries.x position";
           // scatterController.YAxis.AbsoluteKey = "Entries.y position";
           // scatterController.ZAxis.AbsoluteKey = "Entries.z position";
           // passThrough.DefaultState.AddTarget(scatterController);
            
            TransformBoundMutator transformBoundMutator = new TransformBoundMutator();
            transformBoundMutator.ScaleMultiplier.LiteralValue = BoundScale;
            executionMutator.TraceState.AddTarget(transformBoundMutator);

            DistinctUIntToIndexAxis eipIndexMutator = new DistinctUIntToIndexAxis();
            //eipIndexMutator.EntryField.AbsoluteKey = "Instructions";
            eipIndexMutator.AxisKey.AbsoluteKey = "Instructions.Eip";
            eipIndexMutator.IndexAxis.AbsoluteKey = "Instructions.Eip Index";
            transformBoundMutator.DefaultState.AddTarget(eipIndexMutator);

            InsertIndexMutator indexMutator = new InsertIndexMutator();
            indexMutator.Entries.AbsoluteKey = "Instructions";
            indexMutator.IndexFieldName = "Sequential Index";
            eipIndexMutator.DefaultState.AddTarget(indexMutator);

            PassThroughMutator debugPassThrough = new PassThroughMutator();
            indexMutator.DefaultState.AddTarget(debugPassThrough);

            ValueToXYHilbertMutator hilbertMutator = new ValueToXYHilbertMutator();
            //hilbertMutator.EntryField.AbsoluteKey = "Instructions";
            hilbertMutator.IndexValue.AbsoluteKey = "Instructions.Eip Index";
            debugPassThrough.DefaultState.AddTarget(hilbertMutator);
            //eipIndexMutator.DefaultState.AddTarget(hilbertMutator);

            CastIntToFloatAxis zAxisMutator = new CastIntToFloatAxis();
            zAxisMutator.EntryField.AbsoluteKey = "Instructions";
            zAxisMutator.AxisKey.AbsoluteKey = "Instructions.Sequential Index";
            zAxisMutator.AxisFieldName = "Z Axis";
            hilbertMutator.DefaultState.AddTarget(zAxisMutator);


            FloatToProportionAxis xAxisProportion = new FloatToProportionAxis();
            xAxisProportion.EntryField.AbsoluteKey = "Instructions";
            xAxisProportion.ValueField.AbsoluteKey = "Instructions.X Axis";
            xAxisProportion.ProportionFieldName = "X Axis Proportion";
            zAxisMutator.DefaultState.AddTarget(xAxisProportion);

            FloatToProportionAxis yAxisProportion = new FloatToProportionAxis();
            yAxisProportion.EntryField.AbsoluteKey = "Instructions";
            yAxisProportion.ValueField.AbsoluteKey = "Instructions.Y Axis";
            yAxisProportion.ProportionFieldName = "Y Axis Proportion";
            xAxisProportion.DefaultState.AddTarget(yAxisProportion);

            FloatToProportionAxis zAxisProportion = new FloatToProportionAxis();
            zAxisProportion.EntryField.AbsoluteKey = "Instructions";
            zAxisProportion.ValueField.AbsoluteKey = "Instructions.Z Axis";
            zAxisProportion.ProportionFieldName = "Z Axis Proportion";
            yAxisProportion.DefaultState.AddTarget(zAxisProportion);

            RectangularVolumeController drawBoxController = new RectangularVolumeController();
            drawBoxController.Color.LiteralValue = Color.grey;
            drawBoxController.XAxis.LiteralValue = 1f;
            drawBoxController.YAxis.LiteralValue = 1f;
            drawBoxController.ZAxis.LiteralValue = 1f;
            zAxisProportion.DefaultState.AddTarget(drawBoxController);


            RelativePositionBoundMutator setAtTopMutator = new RelativePositionBoundMutator();
            setAtTopMutator.ScaledOffset.LiteralValue = Vector3.up;
            zAxisProportion.DefaultState.AddTarget(setAtTopMutator);


            LineGraphController lineGraphController = new LineGraphController();
            lineGraphController.EntryField.AbsoluteKey = "Instructions";
            lineGraphController.XAxis.AbsoluteKey = "Instructions.X Axis Proportion";
            lineGraphController.YAxis.AbsoluteKey = "Instructions.Y Axis Proportion";
            lineGraphController.ZAxis.AbsoluteKey = "Instructions.Z Axis Proportion";
            //lineGraphController.PointColor.AbsoluteKey = "Instructions.color";
            lineGraphController.PointColor.LiteralValue = Color.red;
            setAtTopMutator.DefaultState.AddTarget(lineGraphController);
            
            
            // Order dependent...
            //traceAdapter.Schema = new MutableObject();


            var haxxisChain = new Chain();
            haxxisChain.AddRootNode(executionAdapter);
            haxxisChain.InitializeSchema();
            haxxisChain.EvaluateRootNodes();

            ChainViews.ChainView.Instance.Chain = haxxisChain;

            //traceAdapter.ReceivePayload(new VisualPayload(new MutableObject(), new VisualDescription(BoundingBox.ConstructBoundingBox())));
            
            //
            // transmit schema
            //ssThrough.Schema = testSchema;
            //
            // transmit payload
            //ssThrough.ReceivePayload(payload);
            */
        }

    }
}
