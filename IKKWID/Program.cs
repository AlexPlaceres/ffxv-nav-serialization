using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Utilities.Extensions;
using IKKWIDI;

namespace IKKWID
{
    internal static class Program
    {
        
        
        private static void Main(string[] args)
        {
            foreach (var binaryPath in args)
            {
                if(File.Exists(binaryPath))
                {
                    string fileExtension = Path.GetExtension(binaryPath);
                    if(fileExtension.Equals(".nav", StringComparison.OrdinalIgnoreCase))
                    {
                        string FileName = Path.GetFileNameWithoutExtension(binaryPath) + "_serialized.bin";
                        Console.ReadKey();

                        //Literally useless stuff I gotta get rid of
                        /*
                        Console.WriteLine("Reading File: " + Path.GetFileNameWithoutExtension(binaryPath) + ".nav");

                        NavParameters navParameters = ParamReadMarshal(binaryPath);
                        displayInfo(navParameters);

                        NavInternal navInternal = InternalReadMarshal(binaryPath);
                        AutoGenerationDataFile dataFile = DataFileReadMarshal(binaryPath);
                        CellInfoFile[] cellInfoFiles = ReadCellInfoFiles(binaryPath, dataFile);
                        LayerMeshFile[] layerMeshFiles = ReadLayerMeshFiles(binaryPath, dataFile);

                        navParameters.originX = 69.0f;

                        Console.WriteLine("Re-Serialized Name: " + FileName);
                        WriteMarshal(FileName, navParameters, navInternal, dataFile, cellInfoFiles, layerMeshFiles);
                        Console.ReadKey();
                        */

                        NavigationFile navFile = new NavigationFile(binaryPath);
                        Console.WriteLine("Agent Radius: " + navFile.navParameters.agentRadius.ToString());
                        Console.WriteLine("Cell Info File Count: " + navFile.cellInfoFiles.Length.ToString());
                        Console.WriteLine("Layer Mesh File Count: " + navFile.layerMeshFiles.Length.ToString());
                        Console.WriteLine("Offset Count: " + navFile.offsetMap.Length.ToString());
                        Console.ReadKey();

                        Console.WriteLine("Total NavMeshes: " + navFile.Meshes.Length.ToString());
                        Console.ReadKey();

                        for (int i = 0; i < navFile.Meshes.Length; i++)
                        {
                            Console.Write("NavMesh " + i.ToString() + " Size: " + navFile.Meshes[i].loadHelper.size.ToString() + "\n");
                        }

                        Console.WriteLine("Attempting to Write to " + FileName + "...");
                        Console.ReadKey();

                        WriteNavigationFile(FileName, navFile);

                    }
                    else
                    {
                        throw new IOException("Unrecognized File Type");
                    }
                }
            }
        }

        public static void WriteNavigationFile(string Path, NavigationFile navFile)
        {
            Byte[] opening = { 0x4E, 0x45, 0x47, 0x4E, 0x5, 0x0, 0x0, 0x0, 0x2E, 0x2E, 0x2E, 0x50, 0x61, 0x64, 0x64, 0x69, 0x6E, 0x67, 0x2E, 0x2E, 0x2E, 0x0, 0x0, 0x0 };
            //Byte[] begin = new byte[104];
            //Byte[] padding = new byte[100];

            using (FileStream stream = File.Open(Path, FileMode.Create))
            {
                stream.Write(opening);
                Utilities.Extensions.StreamExtensions.Align(stream, 128);
                stream.Write(MemoryMarshal.AsBytes(new Span<NavParameters>(ref navFile.navParameters)));
                
                Utilities.Extensions.StreamExtensions.Align(stream, 128);
                stream.Write(MemoryMarshal.AsBytes(new Span<NavInternal>(ref navFile.navInternal)));
                stream.Write(MemoryMarshal.AsBytes(new Span<AutoGenerationDataFile>(ref navFile.autoGenerationDataFile)));

                for (int i = 0; i < navFile.autoGenerationDataFile.cellInfoFileCount; i++)
                {
                    stream.Write(MemoryMarshal.AsBytes(new Span<CellInfoFile>(ref navFile.cellInfoFiles[i])));
                }

                for (int i = 0; i < navFile.autoGenerationDataFile.layerMeshFileCount; i++)
                {
                    stream.Write(MemoryMarshal.AsBytes(new Span<LayerMeshFile>(ref navFile.layerMeshFiles[i])));
                }

                for (int i = 0; i < navFile.autoGenerationDataFile.layerMeshFileCount; i++)
                {
                    stream.Write(MemoryMarshal.AsBytes(new Span<uint>(ref navFile.offsetMap[i])));
                }

                stream.Write(MemoryMarshal.AsBytes(new Span<uint>(ref navFile.endOfFile)));
                Utilities.Extensions.StreamExtensions.Align(stream, 8);

                for (int i = 0; i < navFile.Meshes.Length; i++)
                {
                    Console.WriteLine("Writing Mesh " + i.ToString() + " to File");

                    stream.Write(MemoryMarshal.AsBytes(new Span<MeshLoadHelper>(ref navFile.Meshes[i].loadHelper)));
                    stream.Write(MemoryMarshal.AsBytes(new Span<MeshHeader>(ref navFile.Meshes[i].header)));
                    for (int j = 0; j < navFile.Meshes[i].header.vertCount; j++)
                    {
                        
                        stream.Write(MemoryMarshal.AsBytes(new Span<Vertex>(ref navFile.Meshes[i].vertices[j])));
                    }

                    for (int j = 0; j < navFile.Meshes[i].Polys.Length; j++)
                    {
                        stream.Write(MemoryMarshal.AsBytes(new Span<uint>(ref navFile.Meshes[i].Polys[j].firstLink)));

                        for (int h = 0; h < 7; h++)
                        {
                            stream.Write(MemoryMarshal.AsBytes(new Span<ushort>(ref navFile.Meshes[i].Polys[j].verts[h])));
                        }

                        for (int h = 0; h < 7; h++)
                        {
                            stream.Write(MemoryMarshal.AsBytes(new Span<ushort>(ref navFile.Meshes[i].Polys[j].neis[h])));
                        }

                        stream.Write(MemoryMarshal.AsBytes(new Span<ushort>(ref navFile.Meshes[i].Polys[j].flags)));
                        stream.Write(MemoryMarshal.AsBytes(new Span<byte>(ref navFile.Meshes[i].Polys[j].vertCount)));
                        stream.Write(MemoryMarshal.AsBytes(new Span<byte>(ref navFile.Meshes[i].Polys[j].areaAndType)));
                    }

                    for (int j = 0; j < navFile.Meshes[i].Links.Length; j++)
                    {
                        stream.Write(MemoryMarshal.AsBytes(new Span<MeshLink>(ref navFile.Meshes[i].Links[j])));
                    }

                    for (int j = 0; j < navFile.Meshes[i].DetailMeshes.Length; j++)
                    {
                        stream.Write(MemoryMarshal.AsBytes(new Span<DetailMesh>(ref navFile.Meshes[i].DetailMeshes[j])));
                        //Utilities.Extensions.StreamExtensions.Align(stream, 12);
                    }

                    for (int j = 0; j < navFile.Meshes[i].Tris.Length; j++)
                    {
                        stream.Write(MemoryMarshal.AsBytes(new Span<uint>(ref navFile.Meshes[i].Tris[j])));
                        
                    }
                    if (i != navFile.Meshes.Length - 1)
                    {
                        Utilities.Extensions.StreamExtensions.Align(stream, 8);
                    }
                    
                }
                uint EOF = 0;
                stream.Write(MemoryMarshal.AsBytes(new Span<uint>(ref EOF)));
                Console.ReadKey();
            }
        }


        //ALL of this isn't used and I've got to get rid of it
        public static LayerMeshFile[] ReadLayerMeshFiles(string Path, AutoGenerationDataFile dataFile)
        {
            LayerMeshFile[] layerMeshFiles = new LayerMeshFile[dataFile.layerMeshFileCount];
            using (FileStream stream = File.Open(Path, FileMode.Open))
            {
                stream.Position = 0x1B0 + (0x18 * dataFile.cellInfoFileCount);
                for (int i = 0; i < layerMeshFiles.Length; i++)
                {
                    stream.ReadExactly(MemoryMarshal.AsBytes(new Span<LayerMeshFile>(ref layerMeshFiles[i])));
                }
            }

            return layerMeshFiles;
        }

        public static CellInfoFile[] ReadCellInfoFiles(string Path, AutoGenerationDataFile dataFile)
        {
            CellInfoFile[] cellInfoFiles = new CellInfoFile[dataFile.cellInfoFileCount];
            using (FileStream stream = File.Open(Path, FileMode.Open))
            {
                stream.Position = 0x1B0;
                for (int i = 0; i < cellInfoFiles.Length; i++)
                {
                    stream.ReadExactly(MemoryMarshal.AsBytes(new Span<CellInfoFile>(ref cellInfoFiles[i])));
                    
                    
                }
            }
            return cellInfoFiles;
        }

        public static AutoGenerationDataFile DataFileReadMarshal(string Path)
        {
            AutoGenerationDataFile dataFile = default;
            using (FileStream stream = File.Open(Path, FileMode.Open))
            {
                stream.Position = 0x1A0;
                stream.ReadExactly(MemoryMarshal.AsBytes(new Span<AutoGenerationDataFile>(ref dataFile)));
            }

            return dataFile;
        }

        public static NavInternal InternalReadMarshal(string Path)
        {
            NavInternal navInternal = default;

            using (FileStream stream = File.Open(Path, FileMode.Open))
            {
                stream.Position = 0x180;
                stream.ReadExactly(MemoryMarshal.AsBytes(new Span<NavInternal>(ref navInternal)));
            }

            return navInternal;
        }


        public static NavParameters ParamReadMarshal(string Path)
        {
            NavParameters parameters = default;
            
            using (FileStream stream = File.Open(Path, FileMode.Open))
            {
                stream.Position = 0x80;
                stream.ReadExactly(MemoryMarshal.AsBytes(new Span<NavParameters>(ref parameters)));
                

            }

            return parameters;
        }

        public static void WriteMarshal(string Path, NavParameters parameters, NavInternal navInternal, AutoGenerationDataFile dataFile, CellInfoFile[] cellInfoFiles, LayerMeshFile[] layerMeshFiles)
        {
            Byte[] opening = { 0x4E, 0x45, 0x47, 0x4E, 0x5, 0x0, 0x0, 0x0, 0x2E, 0x2E, 0x2E, 0x50, 0x61, 0x64, 0x64, 0x69, 0x6E, 0x67, 0x2E, 0x2E, 0x2E, 0x0, 0x0, 0x0};
            Byte[] begin = new byte[104];
            Byte[] padding = new byte[100];
            using (FileStream stream = File.Open(Path, FileMode.Create))
            {
                stream.Write(opening);
                stream.Write(begin);
                stream.Write(MemoryMarshal.AsBytes(new Span<NavParameters>(ref parameters)));
                stream.Write(padding);
                stream.Write(MemoryMarshal.AsBytes(new Span<NavInternal>(ref navInternal)));
                stream.Write(MemoryMarshal.AsBytes(new Span<AutoGenerationDataFile>(ref dataFile)));
                for (int i = 0; i < cellInfoFiles.Length; i++)
                { 
                    stream.Write(MemoryMarshal.AsBytes(new Span<CellInfoFile>(ref cellInfoFiles[i])));
                }
                for (int i = 0; i < layerMeshFiles.Length; i++)
                {
                    stream.Write(MemoryMarshal.AsBytes(new Span<LayerMeshFile>(ref layerMeshFiles[i])));
                }
            }
            
        }

        public static void displayInfo(NavParameters parameters)
        {
            Console.WriteLine("Origin: " + parameters.originX.ToString() + ", " + parameters.originY.ToString() + ", " + parameters.originZ.ToString() + ", " + parameters.originW.ToString());
            Console.WriteLine("Cell Size: " + parameters.cellSizeX.ToString() + ", " + parameters.cellSizeY.ToString() + ", " + parameters.cellSizeZ.ToString() + ", " + parameters.cellSizeW.ToString());
            Console.WriteLine("Chunk Cell Size: " + parameters.chunkCellSizeX.ToString() + ", " + parameters.chunkCellSizeY.ToString() + ", " + parameters.chunkCellSizeZ.ToString() + ", " + parameters.chunkCellSizeW.ToString());
            Console.WriteLine("Type: " + parameters.type.ToString());
            Console.WriteLine("Agent Radius: " + parameters.agentRadius.ToString());
            Console.WriteLine("Agent Height: " + parameters.agentHeight.ToString());
            Console.WriteLine("Walkable Slope Angle: " + parameters.walkableSlopeAngle.ToString());
            Console.WriteLine("Agent Max Climb: " + parameters.agentMaxClimb.ToString());
            Console.WriteLine("Edge Max Length: " + parameters.edgeMaxLen.ToString());
            Console.WriteLine("Edge Max Error: " + parameters.edgeMaxError.ToString());
            Console.WriteLine("Detail Sample Dist: " + parameters.detailSampleDist.ToString());
            Console.WriteLine("Detail Sample Max Error: " + parameters.detailSampleMaxError.ToString());
            Console.WriteLine("Region Minimum Size: " + parameters.regionMinSize.ToString());
            Console.WriteLine("Region Merge Size: " + parameters.regionMergeSize.ToString());
            Console.WriteLine("Vertices Per Polygon: " + parameters.vertsPerPoly.ToString());
            Console.WriteLine("Wall Facing Angle Threshold: " + parameters.wallFacingAngleThreshold.ToString());
            Console.WriteLine("Minimum Cover Height: " + parameters.minCoverHeight.ToString());
            Console.WriteLine("Cover Down Extension Distance: " + parameters.coverDownExtensionDist.ToString());
            Console.WriteLine("Erode Algorithm: " + parameters.erodeAlgorithm.ToString());
            Console.WriteLine("Ceiling Heights Size: " + parameters.ceilingHeightSize.ToString());
            Console.WriteLine("Ceiling Heights Mask: " + parameters.ceilingHeightMask.ToString());
            Console.WriteLine("Narrow Passage Size: " + parameters.narrowPassageSize.ToString());
            Console.WriteLine("Large Passage Margin: " + parameters.largePassageMargin.ToString());
            Console.WriteLine("Narrow Passage Margin: " + parameters.narrowPassageMargin.ToString());
            Console.WriteLine("Narrow Passage Material FixID: " + parameters.narrowPassageMaterialId.ToString());
            Console.WriteLine("Ceiling Height Material FixID: " + parameters.ceilingHeightMaterialId.ToString());
            Console.WriteLine("Ceiling Heights: ");
            Console.WriteLine(parameters.ceilingHeights1.ToString());
            Console.WriteLine(parameters.ceilingHeights2.ToString());
            Console.WriteLine(parameters.ceilingHeights3.ToString());
            Console.WriteLine(parameters.ceilingHeights4.ToString());
            Console.WriteLine(parameters.ceilingHeights5.ToString());
            Console.WriteLine(parameters.ceilingHeights6.ToString());
        }
    }
    
}