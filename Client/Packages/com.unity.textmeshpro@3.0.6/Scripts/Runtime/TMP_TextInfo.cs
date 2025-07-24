using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Pool;
using System.Globalization;


namespace TMPro
{
    /// <summary>
    /// Class which contains information about every element contained within the text object.
    /// </summary>
    [Serializable]
    public class TMP_TextInfo
    {
        internal static Vector2 k_InfinityVectorPositive = new Vector2(32767, 32767);
        internal static Vector2 k_InfinityVectorNegative = new Vector2(-32767, -32767);

        public TMP_Text textComponent;

        public int characterCount;
        public int spriteCount;
        public int spaceCount;
        public int wordCount;
        public int linkCount;
        public int lineCount;
        public int pageCount;

        public int materialCount;

        public TMP_CharacterInfo[] characterInfo;
        public TMP_WordInfo[] wordInfo;
        public TMP_LinkInfo[] linkInfo;
        public TMP_LineInfo[] lineInfo;
        public TMP_PageInfo[] pageInfo;
        public TMP_MeshInfo[] meshInfo;

        private TMP_MeshInfo[] m_CachedMeshInfo;

        // Default Constructor
        public TMP_TextInfo()
        {
            characterInfo = new TMP_CharacterInfo[8];
            wordInfo = new TMP_WordInfo[16];
            linkInfo = new TMP_LinkInfo[0];
            lineInfo = new TMP_LineInfo[2];
            pageInfo = new TMP_PageInfo[4];

            meshInfo = new TMP_MeshInfo[1];
        }

        internal TMP_TextInfo(int characterCount)
        {
            characterInfo = new TMP_CharacterInfo[characterCount];
            wordInfo = new TMP_WordInfo[16];
            linkInfo = new TMP_LinkInfo[0];
            lineInfo = new TMP_LineInfo[2];
            pageInfo = new TMP_PageInfo[4];

            meshInfo = new TMP_MeshInfo[1];
        }

        public TMP_TextInfo(TMP_Text textComponent)
        {
            this.textComponent = textComponent;

            characterInfo = new TMP_CharacterInfo[8];

            wordInfo = new TMP_WordInfo[4];
            linkInfo = new TMP_LinkInfo[0];

            lineInfo = new TMP_LineInfo[2];
            pageInfo = new TMP_PageInfo[4];

            meshInfo = new TMP_MeshInfo[1];
            meshInfo[0].mesh = textComponent.mesh;
            materialCount = 1;
        }

        #region Pool

        static TMP_TextInfo_Pool m_TextInfoPool;
        public static bool TextInfoPoolSwitch = true;
        public static TMP_TextInfo_Pool Pool
        {
            get
            {
                if (m_TextInfoPool == null)
                {
                    InitPool();
                }
                return m_TextInfoPool;
            }
        }

        public static void InitPool()
        {
            ClearPool();
            if (m_TextInfoPool == null)
            {
                m_TextInfoPool = new TMP_TextInfo_Pool(6, new int[] { 8, 16, 32, 64, 128, 256 }, new int[] { 4, 64, 128, 8, 4, 1}, new int[] {  4, 64, 128, 8, 4, 1 }, 256);
            }
        }

        public static void InitPool(int levelNum, int[] arraylevelThreshold, int[] arrayInitialCapacity, int[] arrayMaxCapacity, int characterSizeThreshold)
        {
            ClearPool();
            m_TextInfoPool = new TMP_TextInfo_Pool(levelNum, arraylevelThreshold, arrayInitialCapacity, arrayMaxCapacity, characterSizeThreshold);
        }

        public static void ClearPool()
        {
            if (m_TextInfoPool == null)
            {
                return;
            }
            m_TextInfoPool.Clear();
        }
        public static void GetPoolInfo(ref List<int> thresholdInfo, ref List<int> poolCount)
        {
            if (m_TextInfoPool == null)
            {
                return;
            }

            m_TextInfoPool.GetPoolInfo(ref thresholdInfo, ref poolCount);
            return;
        }

        public static TMP_TextInfo defaultInfo = new TMP_TextInfo(0) { materialCount = 0};
        public static bool IsDefaultInfo(TMP_TextInfo info)
        {
            if (!TextInfoPoolSwitch)
            {
                return false;
            }
            return info == defaultInfo;
        }
        public static TMP_TextInfo CreateTMP_TextInfo(TMP_Text textComponent) 
        {
            if (!TextInfoPoolSwitch)
            {
                return new TMP_TextInfo(textComponent);
            }
            if (textComponent == null)
            {
                return null;
            }
            //if (textComponent.m_InternalTextProcessingArraySize == 0)
            //{
            //    return defaultInfo;
            //}
            TMP_TextInfo textInfo;
            textInfo = Pool.Get(textComponent.m_InternalTextProcessingArraySize);
            textInfo.meshInfo[0].mesh = textComponent.mesh;
            textInfo.textComponent = textComponent;
            textInfo.materialCount = 0;

            return textInfo;
        }
        public static TMP_TextInfo CreateTMP_TextInfo(int characterCount)
        {
            if (!TextInfoPoolSwitch)
            {
                return new TMP_TextInfo(characterCount);
            }
            TMP_TextInfo textInfo;
            //if (characterCount == 0)
            //{
            //    return defaultInfo ;
            //}
            textInfo = Pool.Get(characterCount);

            return textInfo;
        }

        public static void Pool_Release(TMP_TextInfo info)
        {
            if (!TextInfoPoolSwitch)
            {
                return;
            }
            if (info == null || IsDefaultInfo(info))
            {
                return;
            }
            //info和里面的数组都不可能为空，强制把数组里面的数据清空
            Array.Fill(info.pageInfo, new TMP_PageInfo());
            Array.Fill(info.characterInfo, new TMP_CharacterInfo());
            Array.Fill(info.lineInfo, new TMP_LineInfo());
            Array.Fill(info.linkInfo, new TMP_LinkInfo());
            Array.Fill(info.wordInfo, new TMP_WordInfo());
            // meshinfo需要清理
            for (int i = 0; i < info.meshInfo.Length; i++)
            {
                info.meshInfo[i].mesh = null;
                info.meshInfo[i].vertices = null;
                info.meshInfo[i].triangles = null;
                info.meshInfo[i].uvs0 = null;
                info.meshInfo[i].uvs2 = null;
                info.meshInfo[i].uvs2 = null;
                info.meshInfo[i].colors32 = null;
            }
            info.meshInfo = new TMP_MeshInfo[1];
            info.textComponent = null;
            info.characterCount = 0;
            info.spriteCount = 0;
            info.spaceCount = 0;
            info.wordCount = 0;
            info.linkCount = 0;
            info.lineCount = 0;
            info.pageCount = 0;
            info.materialCount = 0;
            Pool.Release(info, info.characterInfo.Length);
        }

        public static void ExchangeTextInfo(TMP_TextInfo info, int characterCount, bool block = false, bool setCharacterCount = false)
        {
            if (!TextInfoPoolSwitch)
            {
                TMP_TextInfo.Resize(ref info.characterInfo, characterCount, block);
                return;
            }
            //size up to power of 2
            if (block)
            {
                characterCount = characterCount > 1024 ? characterCount + 256 : Mathf.NextPowerOfTwo(characterCount);
            }
            TMP_TextInfo tmpInfo = CreateTMP_TextInfo(characterCount);
            Array.Copy(info.characterInfo, tmpInfo.characterInfo, Math.Min(info.characterInfo.Length, tmpInfo.characterInfo.Length));
            var var1 = info.characterInfo;
            info.characterInfo = tmpInfo.characterInfo;
            tmpInfo.characterInfo = var1;

            //因为一些奇怪的用法，charactercount在一些情况下还是要设置成对应的desired characterCount
            if (setCharacterCount)
            {
                info.characterCount = characterCount;
            }
            Pool_Release(tmpInfo);
            return;
        }

        #endregion

        /// <summary>
        /// Function to clear the counters of the text object.
        /// </summary>
        public void Clear()
        {
            characterCount = 0;
            spaceCount = 0;
            wordCount = 0;
            linkCount = 0;
            lineCount = 0;
            pageCount = 0;
            spriteCount = 0;

            for (int i = 0; i < this.meshInfo.Length; i++)
            {
                this.meshInfo[i].vertexCount = 0;
            }
        }


        /// <summary>
        ///
        /// </summary>
        internal void ClearAllData()
        {
            characterCount = 0;
            spaceCount = 0;
            wordCount = 0;
            linkCount = 0;
            lineCount = 0;
            pageCount = 0;
            spriteCount = 0;

            this.characterInfo = new TMP_CharacterInfo[4];
            this.wordInfo = new TMP_WordInfo[1];
            this.lineInfo = new TMP_LineInfo[1];
            this.pageInfo = new TMP_PageInfo[1];
            this.linkInfo = new TMP_LinkInfo[0];

            materialCount = 0;

            this.meshInfo = new TMP_MeshInfo[1];
        }


        /// <summary>
        /// Function to clear the content of the MeshInfo array while preserving the Triangles, Normals and Tangents.
        /// </summary>
        public void ClearMeshInfo(bool updateMesh)
        {
            for (int i = 0; i < this.meshInfo.Length; i++)
                this.meshInfo[i].Clear(updateMesh);
        }


        /// <summary>
        /// Function to clear the content of all the MeshInfo arrays while preserving their Triangles, Normals and Tangents.
        /// </summary>
        public void ClearAllMeshInfo()
        {
            for (int i = 0; i < this.meshInfo.Length; i++)
                this.meshInfo[i].Clear(true);
        }


        /// <summary>
        ///
        /// </summary>
        public void ResetVertexLayout(bool isVolumetric)
        {
            for (int i = 0; i < this.meshInfo.Length; i++)
                this.meshInfo[i].ResizeMeshInfo(0, isVolumetric);
        }


        /// <summary>
        /// Function used to mark unused vertices as degenerate.
        /// </summary>
        /// <param name="materials"></param>
        public void ClearUnusedVertices(MaterialReference[] materials)
        {
            for (int i = 0; i < meshInfo.Length; i++)
            {
                int start = 0; // materials[i].referenceCount * 4;
                meshInfo[i].ClearUnusedVertices(start);
            }
        }


        /// <summary>
        /// Function to clear and initialize the lineInfo array.
        /// </summary>
        public void ClearLineInfo()
        {
            if (this.lineInfo == null)
                this.lineInfo = new TMP_LineInfo[2];

            int length = this.lineInfo.Length;

            for (int i = 0; i < length; i++)
            {
                this.lineInfo[i].characterCount = 0;
                this.lineInfo[i].spaceCount = 0;
                this.lineInfo[i].wordCount = 0;
                this.lineInfo[i].controlCharacterCount = 0;
                this.lineInfo[i].width = 0;

                this.lineInfo[i].ascender = k_InfinityVectorNegative.x;
                this.lineInfo[i].descender = k_InfinityVectorPositive.x;

                this.lineInfo[i].marginLeft = 0;
                this.lineInfo[i].marginRight = 0;

                this.lineInfo[i].lineExtents.min = k_InfinityVectorPositive;
                this.lineInfo[i].lineExtents.max = k_InfinityVectorNegative;

                this.lineInfo[i].maxAdvance = 0;
                //this.lineInfo[i].maxScale = 0;
            }
        }

        internal void ClearPageInfo()
        {
            if (this.pageInfo == null)
                this.pageInfo = new TMP_PageInfo[2];

            int length = this.pageInfo.Length;

            for (int i = 0; i < length; i++)
            {
                this.pageInfo[i].firstCharacterIndex = 0;
                this.pageInfo[i].lastCharacterIndex = 0;
                this.pageInfo[i].ascender = -32767;
                this.pageInfo[i].baseLine = 0;
                this.pageInfo[i].descender = 32767;
            }
        }


        /// <summary>
        /// Function to copy the MeshInfo Arrays and their primary vertex data content.
        /// </summary>
        /// <returns>A copy of the MeshInfo[]</returns>
        public TMP_MeshInfo[] CopyMeshInfoVertexData()
        {
            if (m_CachedMeshInfo == null || m_CachedMeshInfo.Length != meshInfo.Length)
            {
                m_CachedMeshInfo = new TMP_MeshInfo[meshInfo.Length];

                // Initialize all the vertex data arrays
                for (int i = 0; i < m_CachedMeshInfo.Length; i++)
                {
                    int length = meshInfo[i].vertices.Length;

                    m_CachedMeshInfo[i].vertices = new Vector3[length];
                    m_CachedMeshInfo[i].uvs0 = new Vector2[length];
                    m_CachedMeshInfo[i].uvs2 = new Vector2[length];
                    m_CachedMeshInfo[i].uvs3 = new Vector2[length];
                    m_CachedMeshInfo[i].colors32 = new Color32[length];

                    //m_CachedMeshInfo[i].normals = new Vector3[length];
                    //m_CachedMeshInfo[i].tangents = new Vector4[length];
                    //m_CachedMeshInfo[i].triangles = new int[meshInfo[i].triangles.Length];
                }
            }

            for (int i = 0; i < m_CachedMeshInfo.Length; i++)
            {
                int length = meshInfo[i].vertices.Length;

                if (m_CachedMeshInfo[i].vertices.Length != length)
                {
                    m_CachedMeshInfo[i].vertices = new Vector3[length];
                    m_CachedMeshInfo[i].uvs0 = new Vector2[length];
                    m_CachedMeshInfo[i].uvs2 = new Vector2[length];
                    m_CachedMeshInfo[i].uvs3 = new Vector2[length];
                    m_CachedMeshInfo[i].colors32 = new Color32[length];

                    //m_CachedMeshInfo[i].normals = new Vector3[length];
                    //m_CachedMeshInfo[i].tangents = new Vector4[length];
                    //m_CachedMeshInfo[i].triangles = new int[meshInfo[i].triangles.Length];
                }


                // Only copy the primary vertex data
                Array.Copy(meshInfo[i].vertices, m_CachedMeshInfo[i].vertices, length);
                Array.Copy(meshInfo[i].uvs0, m_CachedMeshInfo[i].uvs0, length);
                Array.Copy(meshInfo[i].uvs2, m_CachedMeshInfo[i].uvs2, length);
                Array.Copy(meshInfo[i].uvs3, m_CachedMeshInfo[i].uvs3, length);
                Array.Copy(meshInfo[i].colors32, m_CachedMeshInfo[i].colors32, length);

                //Array.Copy(meshInfo[i].normals, m_CachedMeshInfo[i].normals, length);
                //Array.Copy(meshInfo[i].tangents, m_CachedMeshInfo[i].tangents, length);
                //Array.Copy(meshInfo[i].triangles, m_CachedMeshInfo[i].triangles, meshInfo[i].triangles.Length);
            }

            return m_CachedMeshInfo;
        }



        /// <summary>
        /// Function to resize any of the structure contained in the TMP_TextInfo class.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="array"></param>
        /// <param name="size"></param>
        public static void Resize<T> (ref T[] array, int size)
        {
            // Allocated to the next power of two
            int newSize = size > 1024 ? size + 256 : Mathf.NextPowerOfTwo(size);

            Array.Resize(ref array, newSize);
        }


        /// <summary>
        /// Function to resize any of the structure contained in the TMP_TextInfo class.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="array"></param>
        /// <param name="size"></param>
        /// <param name="isFixedSize"></param>
        public static void Resize<T>(ref T[] array, int size, bool isBlockAllocated)
        {
            if (isBlockAllocated) size = size > 1024 ? size + 256 : Mathf.NextPowerOfTwo(size);

            if (size == array.Length) return;

            //Debug.Log("Resizing TextInfo from [" + array.Length + "] to [" + size + "]");

            Array.Resize(ref array, size);
        }

    }
}
