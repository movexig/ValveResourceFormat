﻿using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace ValveResourceFormat.Blocks
{
    /// <summary>
    /// "NTRO" block. CResourceIntrospectionManifest
    /// </summary>
    public class ResourceIntrospectionManifest : Block
    {
        public class ResourceDiskStruct
        {
            public class Field
            {
                public string FieldName { get; set; }
                public short Count { get; set; }
                public short OnDiskOffset { get; set; }
                public List<byte> Indirections { get; private set; }
                public uint TypeData { get; set; }
                public DataType Type { get; set; } // TODO: make this an enum?

                public Field()
                {
                    Indirections = new List<byte>();
                }

                public void WriteText(IndentedTextWriter writer)
                {
                    writer.WriteLine("CResourceDiskStructField");
                    writer.WriteLine("{");
                    writer.Indent++;
                    writer.WriteLine("CResourceString m_pFieldName = \"{0}\"", FieldName);
                    writer.WriteLine("int16 m_nCount = {0}", Count);
                    writer.WriteLine("int16 m_nOnDiskOffset = {0}", OnDiskOffset);

                    writer.WriteLine("uint8[{0}] m_Indirection =", Indirections.Count);
                    writer.WriteLine("[");
                    writer.Indent++;

                    foreach (var dep in Indirections)
                    {
                        writer.WriteLine("{0:D2}", dep);
                    }

                    writer.Indent--;
                    writer.WriteLine("]");

                    writer.WriteLine("uint32 m_nTypeData = 0x{0:X8}", TypeData);
                    writer.WriteLine("int16 m_nType = {0}", (int)Type);
                    writer.Indent--;
                    writer.WriteLine("}");
                }
            }

            public uint IntrospectionVersion { get; set; }
            public uint Id { get; set; }
            public string Name { get; set; }
            public uint DiskCrc { get; set; }
            public int UserVersion { get; set; }
            public ushort DiskSize { get; set; }
            public ushort Alignment { get; set; }
            public uint BaseStructId { get; set; }
            public byte StructFlags { get; set; }
            public List<Field> FieldIntrospection { get; private set; }

            public ResourceDiskStruct()
            {
                FieldIntrospection = new List<Field>();
            }

            public void WriteText(IndentedTextWriter writer)
            {
                writer.WriteLine("CResourceDiskStruct");
                writer.WriteLine("{");
                writer.Indent++;
                writer.WriteLine("uint32 m_nIntrospectionVersion = 0x{0:X8}", IntrospectionVersion);
                writer.WriteLine("uint32 m_nId = 0x{0:X8}", Id);
                writer.WriteLine("CResourceString m_pName = \"{0}\"", Name);
                writer.WriteLine("uint32 m_nDiskCrc = 0x{0:X8}", DiskCrc);
                writer.WriteLine("int32 m_nUserVersion = {0}", UserVersion);
                writer.WriteLine("uint16 m_nDiskSize = 0x{0:X4}", DiskSize);
                writer.WriteLine("uint16 m_nAlignment = 0x{0:X4}", Alignment);
                writer.WriteLine("uint32 m_nBaseStructId = 0x{0:X8}", BaseStructId);

                writer.WriteLine("Struct m_FieldIntrospection[{0}] =", FieldIntrospection.Count);
                writer.WriteLine("[");
                writer.Indent++;

                foreach (var field in FieldIntrospection)
                {
                    field.WriteText(writer);
                }

                writer.Indent--;
                writer.WriteLine("]");
                writer.WriteLine("uint8 m_nStructFlags = 0x{0:X2}", StructFlags);
                writer.Indent--;
                writer.WriteLine("}");
            }
        }

        public class ResourceDiskEnum
        {
            public class Value
            {
                public string EnumValueName { get; set; }
                public int EnumValue { get; set; }

                public void WriteText(IndentedTextWriter writer)
                {
                    writer.WriteLine("CResourceDiskEnumValue");
                    writer.WriteLine("{");
                    writer.Indent++;
                    writer.WriteLine("CResourceString m_pEnumValueName = \"{0}\"", EnumValueName);
                    writer.WriteLine("int32 m_nEnumValue = {0}", EnumValue);
                    writer.Indent--;
                    writer.WriteLine("}");
                }
            }

            public uint IntrospectionVersion { get; set; }
            public uint Id { get; set; }
            public string Name { get; set; }
            public uint DiskCrc { get; set; }
            public int UserVersion { get; set; }
            public List<Value> EnumValueIntrospection { get; private set; }

            public ResourceDiskEnum()
            {
                EnumValueIntrospection = new List<Value>();
            }

            public void WriteText(IndentedTextWriter writer)
            {
                writer.WriteLine("CResourceDiskEnum");
                writer.WriteLine("{");
                writer.Indent++;
                writer.WriteLine("uint32 m_nIntrospectionVersion = 0x{0:X8}", IntrospectionVersion);
                writer.WriteLine("uint32 m_nId = 0x{0:X8}", Id);
                writer.WriteLine("CResourceString m_pName = \"{0}\"", Name);
                writer.WriteLine("uint32 m_nDiskCrc = 0x{0:X8}", DiskCrc);
                writer.WriteLine("int32 m_nUserVersion = {0}", UserVersion);

                writer.WriteLine("Struct m_EnumValueIntrospection[{0}] =", EnumValueIntrospection.Count);
                writer.WriteLine("[");
                writer.Indent++;

                foreach (var value in EnumValueIntrospection)
                {
                    value.WriteText(writer);
                }

                writer.Indent--;
                writer.WriteLine("]");
                writer.Indent--;
                writer.WriteLine("}");
            }
        }

        public uint IntrospectionVersion { get; private set; }

        public List<ResourceDiskStruct> ReferencedStructs { get; }
        public List<ResourceDiskEnum> ReferencedEnums { get; }

        public ResourceIntrospectionManifest()
        {
            ReferencedStructs = new List<ResourceDiskStruct>();
            ReferencedEnums = new List<ResourceDiskEnum>();
        }

        public override BlockType GetChar()
        {
            return BlockType.NTRO;
        }

        public override void Read(BinaryReader reader, Resource resource)
        {
            reader.BaseStream.Position = Offset;

            IntrospectionVersion = reader.ReadUInt32();

            ReadStructs(reader);

            reader.BaseStream.Position = Offset + 12; // skip 3 ints

            ReadEnums(reader);
        }

        private void ReadStructs(BinaryReader reader)
        {
            var entriesOffset = reader.ReadUInt32();
            var entriesCount = reader.ReadUInt32();

            if (entriesCount == 0)
            {
                return;
            }

            reader.BaseStream.Position += entriesOffset - 8; // offset minus 2 ints we just read

            for (var i = 0; i < entriesCount; i++)
            {
                var diskStruct = new ResourceDiskStruct();
                diskStruct.IntrospectionVersion = reader.ReadUInt32();
                diskStruct.Id = reader.ReadUInt32();
                diskStruct.Name = reader.ReadOffsetString(Encoding.UTF8);
                diskStruct.DiskCrc = reader.ReadUInt32();
                diskStruct.UserVersion = reader.ReadInt32();
                diskStruct.DiskSize = reader.ReadUInt16();
                diskStruct.Alignment = reader.ReadUInt16();
                diskStruct.BaseStructId = reader.ReadUInt32();

                var fieldsOffset = reader.ReadUInt32();
                var fieldsSize = reader.ReadUInt32();

                // jump to fields
                if (fieldsSize > 0)
                {
                    var prev = reader.BaseStream.Position;
                    reader.BaseStream.Position += fieldsOffset - 8; // offset minus 2 ints we just read

                    for (var y = 0; y < fieldsSize; y++)
                    {
                        var field = new ResourceDiskStruct.Field();

                        field.FieldName = reader.ReadOffsetString(Encoding.UTF8);
                        field.Count = reader.ReadInt16();
                        field.OnDiskOffset = reader.ReadInt16();

                        var indirectionOffset = reader.ReadUInt32();
                        var indirectionSize = reader.ReadUInt32();

                        if (indirectionSize > 0)
                        {
                            // jump to indirections
                            var prev2 = reader.BaseStream.Position;
                            reader.BaseStream.Position += indirectionOffset - 8; // offset minus 2 ints we just read

                            for (var x = 0; x < indirectionSize; x++)
                            {
                                field.Indirections.Add(reader.ReadByte());
                            }

                            reader.BaseStream.Position = prev2;
                        }

                        field.TypeData = reader.ReadUInt32();
                        field.Type = (DataType)reader.ReadInt16();

                        reader.ReadBytes(2); // TODO: ????

                        diskStruct.FieldIntrospection.Add(field);
                    }

                    reader.BaseStream.Position = prev;
                }

                diskStruct.StructFlags = reader.ReadByte();

                reader.ReadBytes(3); // TODO: ????

                ReferencedStructs.Add(diskStruct);
            }
        }

        private void ReadEnums(BinaryReader reader)
        {
            var entriesOffset = reader.ReadUInt32();
            var entriesCount = reader.ReadUInt32();

            if (entriesCount == 0)
            {
                return;
            }

            reader.BaseStream.Position += entriesOffset - 8; // offset minus 2 ints we just read

            for (var i = 0; i < entriesCount; i++)
            {
                var diskEnum = new ResourceDiskEnum();
                diskEnum.IntrospectionVersion = reader.ReadUInt32();
                diskEnum.Id = reader.ReadUInt32();
                diskEnum.Name = reader.ReadOffsetString(Encoding.UTF8);
                diskEnum.DiskCrc = reader.ReadUInt32();
                diskEnum.UserVersion = reader.ReadInt32();

                var fieldsOffset = reader.ReadUInt32();
                var fieldsSize = reader.ReadUInt32();

                // jump to fields
                if (fieldsSize > 0)
                {
                    var prev = reader.BaseStream.Position;
                    reader.BaseStream.Position += fieldsOffset - 8; // offset minus 2 ints we just read

                    for (var y = 0; y < fieldsSize; y++)
                    {
                        var field = new ResourceDiskEnum.Value();

                        field.EnumValueName = reader.ReadOffsetString(Encoding.UTF8);
                        field.EnumValue = reader.ReadInt32();

                        diskEnum.EnumValueIntrospection.Add(field);
                    }

                    reader.BaseStream.Position = prev;
                }

                ReferencedEnums.Add(diskEnum);
            }
        }

        public override void WriteText(IndentedTextWriter writer)
        {
            writer.WriteLine("CResourceIntrospectionManifest");
            writer.WriteLine("{");
            writer.Indent++;

            writer.WriteLine("uint32 m_nIntrospectionVersion = 0x{0:x8}", IntrospectionVersion);
            writer.WriteLine("Struct m_ReferencedStructs[{0}] =", ReferencedStructs.Count);
            writer.WriteLine("[");
            writer.Indent++;

            foreach (var refStruct in ReferencedStructs)
            {
                refStruct.WriteText(writer);
            }

            writer.Indent--;
            writer.WriteLine("]");

            writer.WriteLine("Struct m_ReferencedEnums[{0}] =", ReferencedEnums.Count);
            writer.WriteLine("[");
            writer.Indent++;

            foreach (var refEnum in ReferencedEnums)
            {
                refEnum.WriteText(writer);
            }

            writer.Indent--;
            writer.WriteLine("]");

            writer.Indent--;
            writer.WriteLine("}");
        }
    }
}
