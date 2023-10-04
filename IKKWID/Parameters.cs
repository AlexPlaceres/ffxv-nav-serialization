using System.IO;
using System.Data;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;
using System.Runtime.CompilerServices;
using System;
using System.Runtime.InteropServices;
using Utilities.Extensions;
using System.Diagnostics;

namespace IKKWIDI
{
    [StructLayout(LayoutKind.Sequential, Pack = 1, Size = 0xC)]
    public record struct Vertex
    {
        float x;
        float y;
        float z;
    }

    public struct LmVector3
    {


        public float x {get; set;}
        public float y {get; set;}
        public float z {get; set;}
        public float w {get; set;}
    }

    public struct LmVector3i
    {


        public int x {get; set;}
        public int y {get; set;}
        public int z {get; set;}
        public int w {get; set;}
    }

    public struct HashFile
    {
        public uint Hash1;
        public uint Hash2;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1, Size = 0x10)]
    public record struct LayerMeshFile
    {
        public HashFile meshHash;
        public HashFile remeshHash;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1, Size = 0x10)]
    public record struct AutoGenerationDataFile
    {
        public int magic;
        public int version;
        public int cellInfoFileCount;
        public int layerMeshFileCount;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1, Size = 0x20)]
    public record struct NavInternal
    {
        public int flags;
        public int priority;
        public int minPriority;
        public uint collisionDataTypeFilter;
        public int xSize;
        public int zSize;
        public int xBegin;
        public int zBegin;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1, Size = 0x18)]
    public record struct CellInfoFile
    {
        public HashFile hash;
        public uint nextRemeshLayer;
        public uint nextMeshLayer;
        public int x;
        public int z;
    }

    [StructLayout(LayoutKind.Sequential, Pack=1, Size=0x9C)]
    public record struct NavParameters
    {
        public static int StructSize {get;} = Unsafe.SizeOf<NavParameters>();
        public float originX;
        public float originY;
        public float originZ;
        public float originW;
        public float cellSizeX;
        public float cellSizeY;
        public float cellSizeZ;
        public float cellSizeW;
        public int chunkCellSizeX;
        public int chunkCellSizeY;
        public int chunkCellSizeZ;
        public int chunkCellSizeW;
        public int type;
        public float agentRadius;
        public float agentHeight;
        public float walkableSlopeAngle;
        public float agentMaxClimb;
        public float edgeMaxLen;
        public float edgeMaxError;
        public float detailSampleDist;
        public float detailSampleMaxError;
        public int regionMinSize;
        public int regionMergeSize;
        public int vertsPerPoly;
        public float wallFacingAngleThreshold;
        public float minCoverHeight;
        public float coverDownExtensionDist;
        public byte erodeAlgorithm;
        public byte ceilingHeightSize;
        public short ceilingHeightMask;
        public float narrowPassageSize;
        public float largePassageMargin;
        public float narrowPassageMargin;
        public uint narrowPassageMaterialId;
        public uint ceilingHeightMaterialId;
        public float ceilingHeights1;
        public float ceilingHeights2;
        public float ceilingHeights3;
        public float ceilingHeights4;
        public float ceilingHeights5;
        public float ceilingHeights6;
        /*
        public NavParameters()
        {
            this.origin = new float[6];
            this.cellSize = new float[6];
            this.chunkCellSize = new int[6];
            this.type = 0;
            this.agentRadius = 0;
            this.agentHeight = 0;
            this.walkableSlopeAngle = 0;
            this.agentMaxClimb = 0;
            this.edgeMaxLen = 0;
            this.edgeMaxError = 0;
            this.detailSampleDist = 0;
            this.detailSampleMaxError = 0;
            this.regionMinSize = 0;
            this.regionMergeSize = 0;
            this.vertsPerPoly = 0;
            this.wallFacingAngleThreshold = 0;
            this.minCoverHeight = 0;
            this.coverDownExtensionDist = 0;
            this.erodeAlgorithm = 0;
            this.ceilingHeightSize = 0;
            this.ceilingHeightMask = 0;
            this.narrowPassageSize = 0;
            this.largePassageMargin = 0;
            this.narrowPassageMargin = 0;
            this.narrowPassageMaterialId = 0;
            this.ceilingHeightMaterialId = 0;
            this.ceilingHeights = new float[6];
        }
        */
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1, Size = 0x64)]
    public record struct MeshHeader
    {
        public int magic;
        public int version;
        public int x;
        public int y;
        public int layer;
        public int tileIndex;
        public int polyCount;
        public int vertCount;
        public int maxLinkCount;
        public int detailMeshCount;
        public int detailVertCount;
        public int detailTriCount;
        public int bvNodeCount;
        public int offMeshConCount;
        public int offMeshBase;
        public float walkableHeight;
        public float walkableRadius;
        public float walkableClimb;

        public float bminX;
        public float bminY;
        public float bminZ;

        public float bmaxX;
        public float bmaxY;
        public float bmaxZ;

        public float bvQuantFactor;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1, Size = 0x20)]
    public record struct MeshLoadHelper
    {
        public int magic;
        public int version;
        public int size;
        public long addressPadding1;
        public long addressPadding2;
        public int addressPadding3;

    }

    [StructLayout(LayoutKind.Sequential, Pack = 1, Size = 0x10)]
    public record struct MeshLink
    {
        public ulong refer;
        public uint next;
        public byte edge;
        public byte side;
        public byte bmin;
        public byte bmax;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1, Size = 0xC)]
    public record struct DetailMesh
    {
        public uint vertBase;
        public uint triBase;
        public byte vertCount;
        public byte triCount;
        public short padding;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1, Size = 0x4)]
    public record struct MeshTri
    {
        public uint data;
    }

    public struct MeshPoly
    {
        public uint firstLink;
        public ushort[] verts;
        public ushort[] neis;
        public ushort flags;
        public byte vertCount;
        public byte areaAndType;
    }


    public class NavMesh
    {
        public MeshLoadHelper loadHelper;
        public MeshHeader header;
        public Vertex[] vertices;
        public MeshPoly[] Polys;
        public MeshLink[] Links;
        public DetailMesh[] DetailMeshes;
        //public MeshTri[] Tris; //Not sure

        public uint[] Tris;

        public NavMesh(FileStream stream)
        {
            this.loadHelper = default;
            this.header = default;
            stream.ReadExactly(MemoryMarshal.AsBytes(new Span<MeshLoadHelper>(ref loadHelper)));
            stream.ReadExactly(MemoryMarshal.AsBytes(new Span<MeshHeader>(ref header)));

            this.vertices = new Vertex[this.header.vertCount];
            for (int i = 0; i < this.header.vertCount; i++)
            {
                stream.ReadExactly(MemoryMarshal.AsBytes(new Span<Vertex>(ref vertices[i])));
            }

            this.Polys = new MeshPoly[this.header.polyCount];
            for (int i = 0; i < this.header.polyCount; i++)
            {
                Console.WriteLine("Reading Poly " + i.ToString());
                this.Polys[i] = default;
                this.Polys[i].firstLink = default;
                this.Polys[i].verts = new ushort[7];
                this.Polys[i].neis = new ushort[7];
                this.Polys[i].flags = default;
                this.Polys[i].vertCount = default;
                this.Polys[i].areaAndType = default;

                stream.ReadExactly(MemoryMarshal.AsBytes(new Span<uint>(ref this.Polys[i].firstLink)));

                for (int j = 0; j < 7; j++)
                {
                    stream.ReadExactly(MemoryMarshal.AsBytes(new Span<ushort>(ref this.Polys[i].verts[j])));
                }
                for (int j = 0; j < 7; j++)
                {
                    stream.ReadExactly(MemoryMarshal.AsBytes(new Span<ushort>(ref this.Polys[i].neis[j])));
                }

                stream.ReadExactly(MemoryMarshal.AsBytes(new Span<ushort>(ref this.Polys[i].flags)));
                stream.ReadExactly(MemoryMarshal.AsBytes(new Span<byte>(ref this.Polys[i].vertCount)));
                stream.ReadExactly(MemoryMarshal.AsBytes(new Span<byte>(ref this.Polys[i].areaAndType)));
            }

            this.Links = new MeshLink[this.header.maxLinkCount];
            for (int i = 0; i < this.header.maxLinkCount; i++)
            {
                stream.ReadExactly(MemoryMarshal.AsBytes(new Span<MeshLink>(ref this.Links[i])));
            }

            this.DetailMeshes = new DetailMesh[this.header.detailMeshCount];
            for (int i = 0; i < this.header.detailMeshCount; i++)
            {
                stream.ReadExactly(MemoryMarshal.AsBytes(new Span<DetailMesh>(ref this.DetailMeshes[i])));
                //Utilities.Extensions.StreamExtensions.Align(stream, 12);
            }

            this.Tris = new uint[this.header.detailTriCount];
            for (int i = 0; i < this.header.detailTriCount; i++)
            {
                stream.ReadExactly(MemoryMarshal.AsBytes(new Span<uint>(ref this.Tris[i])));
            }

        }
    }



    public class NavigationFile
    {

        public NavParameters navParameters;
        public NavInternal navInternal;
        public AutoGenerationDataFile autoGenerationDataFile;
        public CellInfoFile[] cellInfoFiles;
        public LayerMeshFile[] layerMeshFiles;
        public uint[] offsetMap;
        public uint endOfFile;
        public NavMesh[] Meshes;

        public NavigationFile(string Path)
        {
            this.navParameters = default;
            this.navInternal = default;
            this.autoGenerationDataFile = default;


            using (FileStream stream = File.Open(Path, FileMode.Open))
            {
                stream.Position = 0x80;
                stream.ReadExactly(MemoryMarshal.AsBytes(new Span<NavParameters>(ref navParameters)));
                stream.Position = 0x180;
                stream.ReadExactly(MemoryMarshal.AsBytes(new Span<NavInternal>(ref navInternal)));

                stream.ReadExactly(MemoryMarshal.AsBytes(new Span<AutoGenerationDataFile>(ref autoGenerationDataFile)));

                this.cellInfoFiles = new CellInfoFile[autoGenerationDataFile.cellInfoFileCount];
                for (int i = 0; i < cellInfoFiles.Length; i++)
                {
                    stream.ReadExactly(MemoryMarshal.AsBytes(new Span<CellInfoFile>(ref cellInfoFiles[i])));
                }

                this.layerMeshFiles = new LayerMeshFile[autoGenerationDataFile.layerMeshFileCount];
                for (int i = 0; i < layerMeshFiles.Length; i++)
                {
                    stream.ReadExactly(MemoryMarshal.AsBytes(new Span<LayerMeshFile>(ref layerMeshFiles[i])));
                }

                this.offsetMap = new uint[autoGenerationDataFile.layerMeshFileCount];
                for (int i = 0; i < offsetMap.Length; i++)
                {
                    stream.ReadExactly(MemoryMarshal.AsBytes(new Span<uint>(ref offsetMap[i])));
                }

                this.endOfFile = default;
                stream.ReadExactly(MemoryMarshal.AsBytes(new Span<uint>(ref endOfFile)));

                this.Meshes = new NavMesh[this.offsetMap.Length];
                for (int i = 0; i < this.offsetMap.Length; ++i)
                { 
                    stream.Position = this.offsetMap[i];
                    this.Meshes[i] = new NavMesh(stream);
                }
                


            }
        }

    }
}