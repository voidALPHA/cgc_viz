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
using System.IO;

namespace Adapters.TraceAdapters.Traces.Elements
{
    public abstract class TraceElement
    {
        public TraceElementHeader Header { get; private set; }

        protected TraceElement( TraceElementHeader header )
        {
            Header = header;
        }

        protected enum ReadToLengthFormat
        {
            String,
            Hex
        }

        protected string ReadToLength( BinaryReader reader, uint bytesReadSoFar, ReadToLengthFormat format = ReadToLengthFormat.String )
        {
            var totalElementLength = (int) Header.Length;

            int bytesRemaining = totalElementLength - (int)bytesReadSoFar;

            if ( bytesRemaining == 0 )
                return String.Empty;

            if ( bytesRemaining < 0 )
                return null;

            if ( format == ReadToLengthFormat.String )
            {
                var chars = reader.ReadChars( bytesRemaining );
                return new string( chars );
            }

            if ( format == ReadToLengthFormat.Hex )
            {
                var bytes = reader.ReadBytes( bytesRemaining );
                return ByteArrayToHex( bytes );
            }


            throw new NotImplementedException( "Could not parse string in format " + format );
        }

        private static string ByteArrayToHex( byte[] barray )
        {
            char[] c = new char[barray.Length * 2];
            byte b;
            for ( int i = 0; i < barray.Length; ++i )
            {
                b = ( (byte)( barray[i] >> 4 ) );
                c[i * 2] = (char)( b > 9 ? b + 0x37 : b + 0x30 );
                b = ( (byte)( barray[i] & 0xF ) );
                c[i * 2 + 1] = (char)( b > 9 ? b + 0x37 : b + 0x30 );
            }

            return new string( c );
        }

        public override string ToString()
        {
            return string.Format( GetType().Name +
                                  "\n{0}", Header );
        }
    }


    public abstract class InstructionElement : TraceElement
    {
        public virtual uint Eip { get; set; }

        protected InstructionElement( TraceElementHeader header )
            : base( header )
        {
        }
    }

    // TODO: Make common base for call and return...
    
    public class CallElement : InstructionElement
    {
        public uint ToEip { get; private set; }

        public uint Esp { get; private set; }

        public CallElement( TraceElementHeader header )
            : base( header )
        {
        }

        public static CallElement Generate( TraceElementHeader header, BinaryReader reader )
        {
            var element = new CallElement( header )
            {
                Eip = reader.ReadUInt32(),
                ToEip = reader.ReadUInt32(),
                Esp = reader.ReadUInt32()
            };

            return element;
        }
    }

    public class ReturnElement : InstructionElement
    {
        public uint ToEip { get; private set; }

        public uint Esp { get; private set; }

        public ReturnElement( TraceElementHeader header )
            : base( header )
        {
        }

        public static ReturnElement Generate( TraceElementHeader header, BinaryReader reader )
        {
            var element = new ReturnElement( header )
            {
                Eip = reader.ReadUInt32(),
                ToEip = reader.ReadUInt32(),
                Esp = reader.ReadUInt32()
            };

            return element;
        }
    }

    public class BranchElement : InstructionElement
    {
        public uint BranchType { get; private set; }

        public uint ToEip { get; private set; }

        
        // Not part of binary data.
        public BranchTypes BranchTypeName { get; private set; }


        public BranchElement( TraceElementHeader header )
            : base( header )
        {
        }

        public enum BranchTypes
        {
            Call=0,
            Return=1,
            Jump=2,
            ConditionalNotTaken=3,
            ConditionalTaken=4,
        };

        //private static readonly string[] s_BranchTypeNames =
        //{
        //    "Call",
        //    "Return",
        //    "Jump",
        //    "Conditional (not taken)",
        //    "Conditional (taken)",
        //};

        public static BranchElement Generate( TraceElementHeader header, BinaryReader reader )
        {
            var branchElement = new BranchElement( header )
            {
                BranchType = reader.ReadUInt32(),
                Eip = reader.ReadUInt32(),
                ToEip = reader.ReadUInt32()
            };

            branchElement.BranchTypeName = (BranchTypes)branchElement.BranchType;

            return branchElement;
        }
    }
    

    public class InstructionOnlyElement : InstructionElement, IByteStringElement
    {
        public string ByteString { get; private set; }

        public InstructionOnlyElement( TraceElementHeader header )
            : base( header )
        {
        }

        public static InstructionOnlyElement Generate( TraceElementHeader header, BinaryReader reader )
        {
            uint bytesReadSoFar = 0;

            var instructionElement = new InstructionOnlyElement( header );
            //bytesReadSoFar += TraceElementHeader.SizeInBytes;

            instructionElement.Eip = reader.ReadUInt32();
            bytesReadSoFar += sizeof ( UInt32 );

            instructionElement.ByteString = instructionElement.ReadToLength( reader, bytesReadSoFar, ReadToLengthFormat.Hex );

            return instructionElement;
        }

        public override string ToString()
        {
            return string.Format( base.ToString() +
                                  "\n\tEip: {0}" +
                                  "\n\tByte String: {1}",
                                  Eip, ByteString );
        }
    }


    public class InstructionDisasmElement : InstructionElement, IDisasmElement
    {
        public string Disassembly { get; set; }

        public InstructionDisasmElement( TraceElementHeader header )
            : base( header )
        {
        }

        public static InstructionDisasmElement Generate( TraceElementHeader header, BinaryReader reader )
        {
            uint bytesReadSoFar = 0;

            var instructionDisasmElement = new InstructionDisasmElement( header );
            //bytesReadSoFar += TraceElementHeader.SizeInBytes;

            instructionDisasmElement.Eip = reader.ReadUInt32();
            bytesReadSoFar += sizeof ( UInt32 );

            instructionDisasmElement.Disassembly = instructionDisasmElement.ReadToLength( reader, bytesReadSoFar );

            return instructionDisasmElement;
        }

        public override string ToString()
        {
            return base.ToString() + "\n\tDisassembly: " + Disassembly;
        }
    }

    public class InstructionDisasmSchemaElement : InstructionDisasmElement
    {
        public InstructionDisasmSchemaElement(TraceElementHeader header)
            : base( header )
        {
            Disassembly = "schema";
        }
    }

    public class AnnotationElement : TraceElement, IByteStringElement
    {
        public AnnotationElement(TraceElementHeader header) : base(header)
        {
        }

        public AnnotationElement(TraceElementHeader header, string elementType, string byteString) : base (header)
        {
            ByteString = byteString;
            EventType = elementType;
        }

        public string ByteString { get; private set; }

        public string EventType { get; private set; }

        //public int InstructionIndex { get; set; }

        public static AnnotationElement Generate(TraceElementHeader header, BinaryReader reader)
        {
            var annotationElement = new AnnotationElement(header);
            
            uint bytesReadSoFar = 0;
            
            var eventCharacters = (int)reader.ReadUInt32();
            bytesReadSoFar += sizeof(UInt32);

            //bytesReadSoFar += TraceElementHeader.SizeInBytes;
            var descString = annotationElement.ReadToLength(reader, bytesReadSoFar);

            annotationElement.EventType = descString.Substring(0, eventCharacters);

            annotationElement.ByteString = descString.Substring(eventCharacters);

            return annotationElement;
        }

    }

    public class InstructionRegsElement : InstructionElement, IRegsElement, IByteStringElement
    {
        public override uint Eip { get { return Regs.Eip; } }

        public TraceElementRegs Regs { get; private set; }

        public string ByteString { get; private set; }

        public InstructionRegsElement( TraceElementHeader header )
            : base( header )
        {
        }

        public static InstructionRegsElement Generate( TraceElementHeader header, BinaryReader reader )
        {
            uint bytesReadSoFar = 0;

            var instructionRegsElement = new InstructionRegsElement( header );
            //bytesReadSoFar += TraceElementHeader.SizeInBytes;

            instructionRegsElement.Regs = TraceElementRegs.Generate( reader );
            bytesReadSoFar += TraceElementRegs.SizeInBytes;

            instructionRegsElement.ByteString = instructionRegsElement.ReadToLength( reader, bytesReadSoFar );

            return instructionRegsElement;
        }

        public override string ToString()
        {
            return base.ToString() + "\n" + Regs;
        }
    }

    


    public class InstructionRegsDisasmElement : InstructionElement, IRegsElement, IDisasmElement
    {
        public override uint Eip { get { return Regs.Eip; } }

        public TraceElementRegs Regs { get; private set; }

        public string Disassembly { get; private set; }

        public InstructionRegsDisasmElement( TraceElementHeader header )
            : base( header )
        {
        }

        public static InstructionRegsDisasmElement Generate( TraceElementHeader header, BinaryReader reader )
        {
            uint bytesReadSoFar = 0;
            
            var instructionRegsDisasmElement = new InstructionRegsDisasmElement( header );
            //bytesReadSoFar += TraceElementHeader.SizeInBytes;

            instructionRegsDisasmElement.Regs = TraceElementRegs.Generate( reader );
            bytesReadSoFar += TraceElementRegs.SizeInBytes;

            instructionRegsDisasmElement.Disassembly = instructionRegsDisasmElement.ReadToLength( reader, bytesReadSoFar );

            return instructionRegsDisasmElement;
        }

        public override string ToString()
        {
            var disasmString ="\n\tDisassembly: " + Disassembly;
            return base.ToString() + disasmString + "\n" + Regs;
        }
    }

    public interface IRegsElement
    {
        TraceElementRegs Regs { get; }
    }

    public interface IDisasmElement
    {
        string Disassembly { get; }
    }

    public interface IByteStringElement
    {
        string ByteString { get; }
    }

    #region Metadata Element Types

    public class TypeDeclarationElement : TraceElement
    {
        public uint ElementTypeIndex { get; private set; }

        public string ElementTypeStringIdentifier { get; private set; }

        public TypeDeclarationElement( TraceElementHeader header )
            : base( header )
        {
        }

        public static TypeDeclarationElement Generate( TraceElementHeader header, BinaryReader reader )
        {
            uint bytesReadSoFar = 0;

            var element = new TypeDeclarationElement( header );
            //bytesReadSoFar += TraceElementHeader.SizeInBytes;

            element.ElementTypeIndex = reader.ReadUInt32();
            bytesReadSoFar += sizeof( UInt32 );

            element.ElementTypeStringIdentifier = element.ReadToLength( reader, bytesReadSoFar );

            return element;
        }
    }


    public class PerfElement : TraceElement
    {
        public uint MemPages { get; private set; }

        public PerfElement( TraceElementHeader header )
            : base( header )
        {
        }

        public static PerfElement Generate( TraceElementHeader header, BinaryReader reader )
        {
            var element = new PerfElement( header )
            {
                MemPages = reader.ReadUInt32()
            };

            return element;
        }
    }

    #endregion
}
