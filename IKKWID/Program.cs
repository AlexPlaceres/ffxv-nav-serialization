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

                        NavigationFile navFile = new NavigationFile(binaryPath);
                        Console.WriteLine("Cell Info File Count: " + navFile.cellInfoFiles.Length.ToString());
                        Console.WriteLine("Layer Mesh File Count: " + navFile.layerMeshFiles.Length.ToString());
                        Console.WriteLine("Offset Count: " + navFile.offsetMap.Length.ToString());

                        Console.WriteLine("Total NavMeshes: " + navFile.Meshes.Length.ToString());

                        for (int i = 0; i < navFile.Meshes.Length; i++)
                        {
                            Console.WriteLine("NavMesh " + i.ToString() + " Size: " + navFile.Meshes[i].loadHelper.size.ToString() + " bytes");
                        }

                        Console.WriteLine("Attempting to Write to " + FileName + "...");

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
            }
        }

    }
    
}